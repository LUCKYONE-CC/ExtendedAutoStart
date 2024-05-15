using ExtendedAutoStart.Data;
using ExtendedAutoStart.Models;
using ExtendedAutoStart.Models.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using System.Diagnostics;
using System.Management;

namespace ExtendedAutoStart
{
    public partial class Form1 : Form
    {
        private Mutex singleInstanceMutex;

        public Form1()
        {
            InitializeComponent();
            InitializeListViews();
            InitializeNotifyIcon();

            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                PerformF5Action();
                e.Handled = true;
            }
        }

        private void PerformF5Action()
        {
            ReloadListViewItems();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!InitializeMutex())
            {
                return;
            }

            string exePath = GetExecutablePath();
            if (!IsInStartupRegistry(exePath))
            {
                ShowForm();
                AddToStartupRegistry(exePath);
            }
            else
            {
                if (GetSystemUptime().TotalMinutes > 2)
                {
                    Task.Run(StartExtendedAutoStartPrograms);
                }
            }

            DatabaseManager.Instance.InitializeDatabaseIfNeeded();
            LoadExtendedStartupPrograms();
            LoadNormalStartupPrograms();
        }

        private bool InitializeMutex()
        {
            singleInstanceMutex = new Mutex(true, "ExtendedAutoStart", out bool isNewInstance);
            if (!isNewInstance)
            {
                MessageBox.Show("ExtendedAutoStart is already running.", "ExtendedAutoStart", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
                return false;
            }
            return true;
        }
        private TimeSpan GetSystemUptime()
        {
            return TimeSpan.FromMilliseconds(Environment.TickCount64);
        }

        private void InitializeListViews()
        {
            InitializeListView(lV_programsInNormalStartup, ["Name", "StartupType"]);
            InitializeListView(lV_programsInExtendedStartup, ["Name", "Activated"]);
        }
        private async void StartExtendedAutoStartPrograms()
        {
            try
            {
                using (var context = new MainDbContext())
                {
                    var programs = context.ProgramsInExtendedStartup.Where(p => p.Activated).ToList();

                    foreach (var program in programs)
                    {
                        if (!File.Exists(program.Path))
                        {
                            continue;
                        }

                        await Task.Run(() => Process.Start(program.Path));

                        await Task.Delay(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void InitializeListView(ListView listView, string[] columns)
        {
            listView.View = View.Details;
            foreach (var column in columns)
            {
                listView.Columns.Add(column, -2, HorizontalAlignment.Left);
            }
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.Columns[0].Width = 150;
            listView.Columns[1].Width = 100;
        }

        private void LoadExtendedStartupPrograms()
        {
            using (var context = new MainDbContext())
            {
                var programs = context.ProgramsInExtendedStartup.ToList();
                foreach (var program in programs)
                {
                    ListViewItem item = new ListViewItem(program.Name);
                    item.SubItems.Add(program.Activated.ToString());
                    lV_programsInExtendedStartup.Items.Add(item);
                }
            }
        }

        private void LoadNormalStartupPrograms()
        {
            var programs = GetAllProgramsInStartUp().FindAll(p => p.IsInAutoStart);
            foreach (var program in programs)
            {
                if (program.Name == "ExtendedAutoStart") continue;
                ListViewItem item = new ListViewItem(program.Name);
                item.SubItems.Add(program.StartUpType.ToString());
                lV_programsInNormalStartup.Items.Add(item);
            }
        }

        private List<ComputerProgram> GetAllProgramsInStartUp()
        {
            var programs = GetAllInstalledPrograms();
            programs.AddRange(GetProgramsFromRegistry());
            programs.AddRange(GetProgramsFromStartupFolder());
            return programs;
        }

        private RegistryKey GetRegistryKey(string path, bool writable = false)
        {
            if (path.StartsWith(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"))
            {
                return Registry.LocalMachine.OpenSubKey(path, writable);
            }
            else if (path.StartsWith(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run"))
            {
                return Registry.CurrentUser.OpenSubKey(path.Replace("HKEY_CURRENT_USER\\", ""), writable);
            }
            return null;
        }

        private IEnumerable<ComputerProgram> GetProgramsFromRegistry()
        {
            string[] runKeys = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run"
            };

            foreach (var runKey in runKeys)
            {
                using (RegistryKey rk = GetRegistryKey(runKey))
                {
                    if (rk == null) continue;
                    foreach (string skName in rk.GetValueNames())
                    {
                        string programName = skName;
                        string programPath = rk.GetValue(skName)?.ToString();
                        if (programPath == null) continue;
                        yield return new ComputerProgram
                        {
                            Name = programName,
                            Path = programPath,
                            IsInAutoStart = true,
                            StartUpType = StartUpType.Registry
                        };
                    }
                }
            }
        }

        private IEnumerable<ComputerProgram> GetProgramsFromStartupFolder()
        {
            string adminStartupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string userStartupFolderPath = GetUserStartupFolder();

            var startupFolders = new List<string> { adminStartupFolderPath, userStartupFolderPath };

            foreach (var startupFolder in startupFolders)
            {
                if (Directory.Exists(startupFolder))
                {
                    foreach (string startupFile in Directory.GetFiles(startupFolder))
                    {
                        string programName = Path.GetFileNameWithoutExtension(startupFile);
                        yield return new ComputerProgram
                        {
                            Name = programName,
                            Path = startupFile,
                            IsInAutoStart = true,
                            StartUpType = StartUpType.StartUpFolder
                        };
                    }
                }
            }
        }

        private List<ComputerProgram> GetAllInstalledPrograms()
        {
            var programs = new List<ComputerProgram>();
            string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {
                if (rk == null) return programs;
                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        if (sk == null) continue;
                        string? programName = sk.GetValue("DisplayName")?.ToString();
                        string? programPath = sk.GetValue("InstallLocation")?.ToString();
                        if (programName == null || programPath == null) continue;
                        programs.Add(new ComputerProgram
                        {
                            Name = programName,
                            Path = programPath,
                            IsInAutoStart = false
                        });
                    }
                }
            }
            return programs;
        }

        private void removeFromStartupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveProgramFromStartup(lV_programsInNormalStartup);
        }

        private void RemoveProgramFromStartup(ListView listView)
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show("No program selected");
                return;
            }

            var selectedProgram = listView.SelectedItems[0];
            string programName = selectedProgram.SubItems[0].Text;

            if (ConfirmAction($"Are you sure you want to remove {programName} from startup?"))
            {
                var program = GetAllProgramsInStartUp().Find(p => p.Name == programName);
                if (program == null) return;

                RemoveProgramFromStartupRegistryOrFolder(program);
                listView.Items.Remove(selectedProgram);
                MessageBox.Show("Program removed from startup");
            }
        }

        private bool ConfirmAction(string message)
        {
            return MessageBox.Show(message, "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        private void RemoveProgramFromStartupRegistryOrFolder(ComputerProgram program)
        {
            if (program.StartUpType == StartUpType.Registry)
            {
                RemoveFromRegistry(program.Name);
            }
            else if (program.StartUpType == StartUpType.StartUpFolder)
            {
                RemoveFromStartupFolder(program);
            }
        }

        private void RemoveFromRegistry(string programName)
        {
            string[] runKeys = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run"
            };

            foreach (var runKey in runKeys)
            {
                using (RegistryKey rk = GetRegistryKey(runKey, writable: true))
                {
                    rk?.DeleteValue(programName, throwOnMissingValue: false);
                }
            }
        }

        private void RemoveFromStartupFolder(ComputerProgram program)
        {
            string userStartupFolderPath = GetUserStartupFolder();
            string startupFilePathWithLnk = Path.Combine(userStartupFolderPath, program.Name + ".lnk");

            if (File.Exists(startupFilePathWithLnk))
            {
                File.Delete(startupFilePathWithLnk);
            }

            if (File.Exists(program.Path))
            {
                File.Delete(program.Path);
            }
        }

        private static string GetUserStartupFolder()
        {
            string userName = GetLoggedInUserName();
            string userProfilePath = $@"C:\Users\{userName}";

            return Path.Combine(userProfilePath, @"AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup");
        }

        private static string GetLoggedInUserName()
        {
            string query = "SELECT UserName FROM Win32_ComputerSystem WHERE UserName IS NOT NULL";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            using (ManagementObjectCollection results = searcher.Get())
            {
                foreach (ManagementObject result in results)
                {
                    string userName = result["UserName"]?.ToString();
                    if (!string.IsNullOrEmpty(userName))
                    {
                        string[] parts = userName.Split('\\');
                        if (parts.Length > 1)
                        {
                            return parts[1];
                        }
                        return userName;
                    }
                }
            }

            throw new InvalidOperationException("Kein angemeldeter Benutzer gefunden.");
        }

        private void openLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenProgramLocation(lV_programsInNormalStartup);
        }

        private void OpenProgramLocation(ListView listView)
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show("No program selected");
                return;
            }

            var selectedProgram = listView.SelectedItems[0];
            string programName = selectedProgram.SubItems[0].Text;

            var program = GetAllProgramsInStartUp().Find(p => p.Name == programName);
            if (program != null && program.Path != null)
            {
                Process.Start("explorer.exe", $"/select, {program.Path}");
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveExtendedStartupProgram();
        }

        private void RemoveExtendedStartupProgram()
        {
            if (lV_programsInExtendedStartup.SelectedItems.Count == 0)
            {
                MessageBox.Show("No program selected");
                return;
            }

            var selectedProgram = lV_programsInExtendedStartup.SelectedItems[0];
            string programName = selectedProgram.SubItems[0].Text;

            if (ConfirmAction($"Are you sure you want to remove {programName} from extended startup?"))
            {
                using (var context = new MainDbContext())
                {
                    var program = context.ProgramsInExtendedStartup.FirstOrDefault(p => p.Name == programName);
                    if (program != null)
                    {
                        context.ProgramsInExtendedStartup.Remove(program);
                        context.SaveChanges();
                        lV_programsInExtendedStartup.Items.Remove(selectedProgram);
                    }
                }
            }
        }

        private void activateDeactivateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleExtendedStartupProgramActivation();
        }

        private void ToggleExtendedStartupProgramActivation()
        {
            if (lV_programsInExtendedStartup.SelectedItems.Count == 0)
            {
                MessageBox.Show("No program selected");
                return;
            }

            var selectedProgram = lV_programsInExtendedStartup.SelectedItems[0];
            string programName = selectedProgram.SubItems[0].Text;

            using (var context = new MainDbContext())
            {
                var program = context.ProgramsInExtendedStartup.FirstOrDefault(p => p.Name == programName);
                if (program != null)
                {
                    program.Activated = !program.Activated;
                    context.SaveChanges();
                    ReloadExtendedStartupPrograms();
                }
            }
        }

        private void ReloadExtendedStartupPrograms()
        {
            lV_programsInExtendedStartup.Items.Clear();
            LoadExtendedStartupPrograms();
        }

        private void addNewProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewExtendedStartupProgram();
        }

        private void AddNewExtendedStartupProgram()
        {
            try
            {
                string programName = SelectProgramFile(out string selectedFile);
                if (string.IsNullOrEmpty(programName) || string.IsNullOrEmpty(selectedFile)) return;

                AddProgramToDatabase(selectedFile);
                MessageBox.Show("Program added to extended startup");

                ListViewItem item = new ListViewItem(programName);
                item.SubItems.Add("True");
                lV_programsInExtendedStartup.Items.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string SelectProgramFile(out string selectedFile)
        {
            selectedFile = string.Empty;
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Executable Files|*.exe|All Files|*.*";
                openFileDialog.Title = "Wähle ein Programm aus";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFile = openFileDialog.FileName;
                    return Path.GetFileNameWithoutExtension(selectedFile);
                }
            }
            return string.Empty;
        }

        private void AddProgramToDatabase(string filePath)
        {
            using (var context = new MainDbContext())
            {
                var program = new ExtendedStartupProgram { Name = Path.GetFileNameWithoutExtension(filePath) };
                context.ProgramsInExtendedStartup.Add(program);
                context.SaveChanges();
            }
        }

        private void btn_transfer_Click(object sender, EventArgs e)
        {
            TransferProgramsToExtendedStartup();
        }

        private void TransferProgramsToExtendedStartup()
        {
            try
            {
                using (var context = new MainDbContext())
                {
                    var programs = GetAllProgramsInStartUp().FindAll(p => p.IsInAutoStart);
                    var programsFromDb = context.ProgramsInExtendedStartup.ToList();
                    foreach (var program in programs)
                    {
                        if (programsFromDb.Any(p => p.Name == program.Name))
                        {
                            var programFromDb = programsFromDb.First(p => p.Name == program.Name);
                            context.ProgramsInExtendedStartup.Remove(programFromDb);
                        }

                        if (program.Name == "ExtendedAutoStart") continue;
                        context.ProgramsInExtendedStartup.Add(new ExtendedStartupProgram
                        {
                            Name = program.Name,
                            Path = program.Path,
                            Activated = true
                        });
                        RemoveProgramFromStartupRegistryOrFolder(program);
                    }
                    context.SaveChanges();
                }

                MessageBox.Show("All programs transferred to extended startup", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                ReloadListViewItems();
            }
        }

        private void ReloadListViewItems()
        {
            lV_programsInNormalStartup.Items.Clear();
            lV_programsInExtendedStartup.Items.Clear();
            LoadNormalStartupPrograms();
            LoadExtendedStartupPrograms();
        }

        private bool IsInStartupRegistry(string exePath)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                string value = key.GetValue("ExtendedAutoStart") as string;

                //Passe den Pfad an, falls er nicht mit dem aktuellen Pfad übereinstimmt
                if (value != null && !value.Equals(exePath, StringComparison.OrdinalIgnoreCase))
                {
                    key.SetValue("ExtendedAutoStart", exePath);
                }

                return value != null && value.Equals(exePath, StringComparison.OrdinalIgnoreCase);
            }
        }

        private void AddToStartupRegistry(string exePath)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("ExtendedAutoStart", exePath);
            }
        }

        private string GetExecutablePath()
        {
            var mainModule = Process.GetCurrentProcess().MainModule;
            if (mainModule == null || string.IsNullOrEmpty(mainModule.FileName))
            {
                throw new Exception("Could not get executable path");
            }
            return mainModule.FileName;
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            this.Visible = false;
            this.Hide();
        }

        private void InitializeNotifyIcon()
        {
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            var contextMenu = new ContextMenuStrip();
            var openMenuItem = new ToolStripMenuItem("Open", null, OpenMenuItem_Click);
            var exitMenuItem = new ToolStripMenuItem("Exit", null, ExitMenuItem_Click);
            contextMenu.Items.AddRange(new[] { openMenuItem, exitMenuItem });
            notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            singleInstanceMutex?.Close();
            notifyIcon.Dispose();

            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Kill();
        }

        private void ShowForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            notifyIcon.ShowBalloonTip(1000, "ExtendedAutoStart", "The application is still running in the background.", ToolTipIcon.Info);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon.Dispose();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Form1_FormClosing(this, e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Form1_FormClosed(this, e);
            base.OnFormClosed(e);
        }

        private void refreshToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                ReloadListViewItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ReloadListViewItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btn_import_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "SQLite Database|*.db|All Files|*.*";
                openFileDialog.Title = "Select a SQLite Database";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    ImportDataFromSQLite(selectedFilePath);
                }
            }
        }

        private void ImportDataFromSQLite(string filePath)
        {
            try
            {
                var programsToImport = GetProgramsFromExternalDatabase(filePath);

                using (var context = new MainDbContext())
                {
                    foreach (var program in programsToImport)
                    {
                        if (!context.ProgramsInExtendedStartup.Any(p => p.Name == program.Name))
                        {
                            context.ProgramsInExtendedStartup.Add(program);
                        }
                    }
                    context.SaveChanges();
                }

                MessageBox.Show("Data imported successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ReloadListViewItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during import: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<ExtendedStartupProgram> GetProgramsFromExternalDatabase(string filePath)
        {
            var programs = new List<ExtendedStartupProgram>();

            var connectionString = $"Data Source={filePath}";
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Name, Path, Activated FROM ProgramsInExtendedStartup";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var program = new ExtendedStartupProgram
                        {
                            Name = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                            Path = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            Activated = reader.IsDBNull(2) ? false : reader.GetBoolean(2)
                        };
                        programs.Add(program);
                    }
                }
            }

            return programs;
        }
    }
}