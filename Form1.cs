using ExtendedAutoStart.Data;
using ExtendedAutoStart.Models;
using ExtendedAutoStart.Models.Enums;
using Microsoft.Win32;
using System.Diagnostics;

namespace ExtendedAutoStart
{
    public partial class Form1 : Form
    {
        private Mutex singleInstanceMutex;

        public Form1()
        {
            InitializeComponent();
            InitializeListViews();
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
                AddToStartupRegistry(exePath);
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

        private void InitializeListViews()
        {
            InitializeListView(lV_programsInNormalStartup, new[] { "Name", "StartupType" });
            InitializeListView(lV_programsInExtendedStartup, new[] { "Name", "Activated" });
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

        private IEnumerable<ComputerProgram> GetProgramsFromRegistry()
        {
            string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(runKey))
            {
                if (rk == null) yield break;
                foreach (string skName in rk.GetValueNames())
                {
                    string programName = skName;
                    string? programPath = rk.GetValue(skName)?.ToString();
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

        private IEnumerable<ComputerProgram> GetProgramsFromStartupFolder()
        {
            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            foreach (string startupFile in Directory.GetFiles(startupFolderPath))
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
            string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(runKey, true))
            {
                rk?.DeleteValue(programName);
            }
        }

        private void RemoveFromStartupFolder(ComputerProgram program)
        {
            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string startupFilePathWithLnk = Path.Combine(startupFolderPath, program.Name + ".lnk");

            if (File.Exists(startupFilePathWithLnk))
            {
                File.Delete(startupFilePathWithLnk);
            }

            if (File.Exists(program.Path))
            {
                File.Delete(program.Path);
            }
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
                    foreach (var program in programs)
                    {
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
                string value = key.GetValue("ProfileNavisionIntegration") as string;
                return value != null && value.Equals(exePath, StringComparison.OrdinalIgnoreCase);
            }
        }

        private void AddToStartupRegistry(string exePath)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("ProfileNavisionIntegration", exePath);
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            singleInstanceMutex?.Close();
            base.OnFormClosing(e);
        }
    }
}