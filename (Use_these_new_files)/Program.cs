using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using HelpCleaner.Dashboard; // This references your DashboardForm.cs

namespace HelpCleaner
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1️⃣ Check admin
            if (!IsAdministrator())
            {
                MessageBox.Show(
                    "HelpCleaner requires administrator privileges to install properly.\nPlease restart as Administrator.",
                    "Admin Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // 2️⃣ Ask for install path
            string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "HelpCleaner");
            string installPath = PromptForInstallPath(defaultPath);
            if (string.IsNullOrEmpty(installPath)) return;

            try
            {
                Directory.CreateDirectory(installPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create install folder:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 3️⃣ Copy current installer EXE to install folder
            string sourceExe = Process.GetCurrentProcess().MainModule!.FileName!;
            string targetExe = Path.Combine(installPath, "HelpCleanerInstaller.exe");

            try
            {
                File.Copy(sourceExe, targetExe, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy installer to install folder:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 4️⃣ Create desktop shortcut pointing to installer (optional)
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string shortcutPath = Path.Combine(desktop, "HelpCleaner.lnk");

                Type wshShellType = Type.GetTypeFromProgID("WScript.Shell")!;
                dynamic wsh = Activator.CreateInstance(wshShellType);
                var shortcut = wsh.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetExe;
                shortcut.WorkingDirectory = installPath;
                shortcut.WindowStyle = 1;
                shortcut.Description = "Launch HelpCleaner";
                shortcut.Save();
            }
            catch
            {
                // ignore errors
            }

            // 5️⃣ Launch the actual dashboard directly
            try
            {
                Application.Run(new DashboardForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch dashboard:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static bool IsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static string PromptForInstallPath(string defaultPath)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select installation folder for HelpCleaner",
                SelectedPath = defaultPath,
                ShowNewFolderButton = true
            };
            return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
        }
    }
}
