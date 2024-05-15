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
            InitializeListView();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DatabaseManager.Instance.InitializeDatabaseIfNeeded();

            var programs = GetAllProgramsInStartUp();

            var programsInAutoStart = programs.FindAll(p => p.IsInAutoStart);

            foreach (var program in programsInAutoStart)
            {
                ListViewItem item = new ListViewItem(program.Name);
                item.SubItems.Add(program.StartUpType.ToString());
                lV_programsInNormalStartup.Items.Add(item);
            }

            // Resize columns to fit content
            lV_programsInNormalStartup.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            // Set specific width if needed
            lV_programsInNormalStartup.Columns[0].Width = 200; // Set the width of the Name column
            lV_programsInNormalStartup.Columns[1].Width = 100; // Set the width of the StartupType column
        }

        private void InitializeListView()
        {
            lV_programsInNormalStartup.View = View.Details;
            lV_programsInNormalStartup.Columns.Add("Name", -2, HorizontalAlignment.Left);
            lV_programsInNormalStartup.Columns.Add("StartupType", -2, HorizontalAlignment.Left);
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

                    if(File.Exists(program.Path))
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
            if(lV_programsInNormalStartup.SelectedItems.Count == 0)
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
    }
}