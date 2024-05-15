using ExtendedAutoStart.Data;
using ExtendedAutoStart.Models;
using ExtendedAutoStart.Models.Enums;
using Microsoft.Win32;
using System.Diagnostics;

namespace ExtendedAutoStart
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeListViews();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DatabaseManager.Instance.InitializeDatabaseIfNeeded();

            using (MainDbContext context = new MainDbContext())
            {
                var programsInExtendedStartup = context.ProgramsInExtendedStartup.ToList();

                foreach (var program in programsInExtendedStartup)
                {
                    ListViewItem item = new ListViewItem(program.Name);
                    item.SubItems.Add(program.Activated.ToString());
                    lV_programsInExtendedStartup.Items.Add(item);
                }
            }

            var programs = GetAllProgramsInStartUp();

            var programsInAutoStart = programs.FindAll(p => p.IsInAutoStart);

            foreach (var program in programsInAutoStart)
            {
                ListViewItem item = new ListViewItem(program.Name);
                item.SubItems.Add(program.StartUpType.ToString());
                lV_programsInNormalStartup.Items.Add(item);
            }
        }

        private void InitializeListViews()
        {
            lV_programsInNormalStartup.View = View.Details;
            lV_programsInNormalStartup.Columns.Add("Name", -2, HorizontalAlignment.Left);
            lV_programsInNormalStartup.Columns.Add("StartupType", -2, HorizontalAlignment.Left);

            lV_programsInExtendedStartup.View = View.Details;
            lV_programsInExtendedStartup.Columns.Add("Name", -2, HorizontalAlignment.Left);
            lV_programsInExtendedStartup.Columns.Add("Activated", -2, HorizontalAlignment.Left);


            lV_programsInNormalStartup.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            lV_programsInNormalStartup.Columns[0].Width = 150;
            lV_programsInNormalStartup.Columns[1].Width = 100;

            lV_programsInExtendedStartup.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            lV_programsInExtendedStartup.Columns[0].Width = 150;
            lV_programsInExtendedStartup.Columns[1].Width = 100;
        }

        private List<ComputerProgram> GetAllProgramsInStartUp()
        {
            List<ComputerProgram> programs = GetAllInstalledPrograms();
            string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(runKey))
            {
                if (rk != null)
                {
                    foreach (string skName in rk.GetValueNames())
                    {
                        string programName = skName;
                        string? programPath = rk.GetValue(skName)?.ToString();

                        if (programPath == null)
                            continue;

                        programs.Add(new ComputerProgram()
                        {
                            Name = programName,
                            Path = programPath,
                            IsInAutoStart = true,
                            StartUpType = StartUpType.Registry
                        });
                    }
                }
            }

            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string[] startupFiles = Directory.GetFiles(startupFolderPath);

            foreach (string startupFile in startupFiles)
            {
                string programName = Path.GetFileNameWithoutExtension(startupFile);
                programs.Add(new ComputerProgram()
                {
                    Name = programName,
                    Path = startupFile,
                    IsInAutoStart = true,
                    StartUpType = StartUpType.StartUpFolder
                });
            }

            return programs;
        }

        private List<ComputerProgram> GetAllInstalledPrograms()
        {
            List<ComputerProgram> programs = new List<ComputerProgram>();
            string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {
                if (rk == null)
                    return programs;

                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        try
                        {
                            if (sk == null)
                            {
                                continue;
                            }

                            if (sk.GetValue("DisplayName") == null || sk.GetValue("InstallLocation") == null)
                            {
                                continue;
                            }

                            string? programName = sk.GetValue("DisplayName")?.ToString();
                            string? programPath = sk.GetValue("InstallLocation")?.ToString();

                            if (programName == null || programPath == null)
                            {
                                continue;
                            }

                            programs.Add(new ComputerProgram()
                            {
                                Name = programName,
                                Path = programPath,
                                IsInAutoStart = false
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
            }
            return programs;
        }

        private void removeFromStartupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (lV_programsInNormalStartup.SelectedItems.Count == 0)
                {
                    MessageBox.Show("No program selected");
                    return;
                }

                var selectedProgram = lV_programsInNormalStartup.SelectedItems[0];

                string programName = selectedProgram.SubItems[0].Text;

                if (MessageBox.Show($"Are you sure you want to remove {programName} from startup?", "Remove from startup", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }

                if (programName == null)
                {
                    MessageBox.Show("No program selected");
                    return;
                }

                ComputerProgram program = GetAllProgramsInStartUp().Find(p => p.Name == programName);

                if (program == null || program.Name == null)
                {
                    MessageBox.Show("Program not found");
                    return;
                }

                if (program.StartUpType == StartUpType.Registry)
                {
                    string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                    using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(runKey, true))
                    {
                        rk.DeleteValue(program.Name);
                    }
                }
                else if (program.StartUpType == StartUpType.StartUpFolder)
                {
                    string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

                    //If .lnk file exists, delete it. But also delete the file with the same name as the program
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


                MessageBox.Show("Program removed from startup");

                lV_programsInNormalStartup.Items.Remove(selectedProgram);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void openLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lV_programsInNormalStartup.SelectedItems.Count == 0)
            {
                MessageBox.Show("No program selected");
                return;
            }

            var selectedProgram = lV_programsInNormalStartup.SelectedItems[0];

            string programName = selectedProgram.SubItems[0].Text;

            var ComputerProgram = GetAllProgramsInStartUp().Find(p => p.Name == programName);

            if (ComputerProgram == null || ComputerProgram.Path == null)
            {
                MessageBox.Show("Program not found");
                return;
            }

            Process.Start("explorer.exe", $"/select, {ComputerProgram.Path}");
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (lV_programsInExtendedStartup.SelectedItems.Count == 0)
                {
                    MessageBox.Show("No program selected");
                    return;
                }

                var selectedProgram = lV_programsInExtendedStartup.SelectedItems[0];

                string programName = selectedProgram.SubItems[0].Text;

                if (MessageBox.Show($"Are you sure you want to remove {programName} from extended startup?", "Remove from extended startup", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }

                using (MainDbContext context = new MainDbContext())
                {
                    var program = context.ProgramsInExtendedStartup.FirstOrDefault(p => p.Name == programName);

                    if (program == null)
                    {
                        MessageBox.Show("Program not found");
                        return;
                    }

                    context.ProgramsInExtendedStartup.Remove(program);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void activateDeactivateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (lV_programsInExtendedStartup.SelectedItems.Count == 0)
                {
                    MessageBox.Show("No program selected");
                    return;
                }

                var selectedProgram = lV_programsInExtendedStartup.SelectedItems[0];

                string programName = selectedProgram.SubItems[0].Text;

                using (MainDbContext context = new MainDbContext())
                {
                    var program = context.ProgramsInExtendedStartup.FirstOrDefault(p => p.Name == programName);

                    if (program == null)
                    {
                        MessageBox.Show("Program not found");
                        return;
                    }

                    program.Activated = !program.Activated;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                lV_programsInExtendedStartup.Items.Clear();

                using (MainDbContext context = new MainDbContext())
                {
                    var programsInExtendedStartup = context.ProgramsInExtendedStartup.ToList();

                    foreach (var program in programsInExtendedStartup)
                    {
                        ListViewItem item = new ListViewItem(program.Name);
                        item.SubItems.Add(program.Activated.ToString());
                        lV_programsInExtendedStartup.Items.Add(item);
                    }
                }
            }
        }

        private void addNewProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string programName = string.Empty;
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Executable Files|*.exe|All Files|*.*";
                    openFileDialog.Title = "Wähle ein Programm aus";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedFile = openFileDialog.FileName;

                        programName = Path.GetFileNameWithoutExtension(selectedFile);

                        AddProgramToDatabase(selectedFile);
                    }
                }

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

        private void AddProgramToDatabase(string filePath)
        {
            try
            {
                using (var context = new MainDbContext())
                {
                    var program = new ExtendedStartupProgram
                    {
                        Name = Path.GetFileNameWithoutExtension(filePath)
                    };

                    context.ProgramsInExtendedStartup.Add(program);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while adding program to database", ex);
            }
        }
    }
}