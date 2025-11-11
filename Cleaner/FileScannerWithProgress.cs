using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using HelpCleaner.Utils;

namespace HelpCleaner.Cleaner
{
    public class FileScannerWithProgress
    {
        private const long LargeFileThreshold = 1L * 1024 * 1024 * 1024; // 1 GB
        private readonly string logFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "HelpCleanerDeletedFiles.log");

        private readonly Action<int> progressCallback;

        public FileScannerWithProgress(Action<int> progressCallback)
        {
            this.progressCallback = progressCallback;
        }

        public void ScanAndDelete()
        {
            var largeFiles = new List<FileInfo>();
            var drives = DriveInfo.GetDrives();
            int totalDrives = drives.Length;
            int driveIndex = 0;

            foreach (var drive in drives)
            {
                driveIndex++;
                if (!drive.IsReady) continue;

                try
                {
                    AddLargeFilesWithProgress(drive.RootDirectory, largeFiles, driveIndex, totalDrives);
                }
                catch { /* skip inaccessible drives */ }
            }

            if (largeFiles.Count == 0)
            {
                MessageBox.Show("No files larger than 1 GB were found.", "Scan Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                progressCallback?.Invoke(100);
                return;
            }

            string message = "The following files are larger than 1 GB:\n\n";
            foreach (var fi in largeFiles)
            {
                message += $"{fi.FullName} ({fi.Length / (1024 * 1024 * 1024)} GB)\n";
            }
            message += "\n⚠ Are you sure you want to delete these files? Make a backup drive first!";

            var confirm = MessageBox.Show(message, "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes)
            {
                MessageBox.Show("Operation cancelled. No files were deleted.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                progressCallback?.Invoke(100);
                return;
            }

            int deletedCount = 0;
            long deletedBytes = 0;
            using (StreamWriter log = new StreamWriter(logFile, true))
            {
                int totalFiles = largeFiles.Count;
                for (int i = 0; i < totalFiles; i++)
                {
                    var fi = largeFiles[i];
                    try
                    {
                        File.Delete(fi.FullName);
                        deletedCount++;
                        deletedBytes += fi.Length;
                        log.WriteLine($"{DateTime.Now}: Deleted {fi.FullName} ({fi.Length / (1024 * 1024 * 1024)} GB)");
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine($"{DateTime.Now}: Failed to delete {fi.FullName}: {ex.Message}");
                    }

                    int percent = (i + 1) * 100 / totalFiles;
                    progressCallback?.Invoke(percent);
                }
            }

            MessageBox.Show(
                $"Deleted {deletedCount} files, totaling {deletedBytes / (1024 * 1024 * 1024)} GB.\nDetails logged to {logFile}.",
                "Cleanup Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void AddLargeFilesWithProgress(DirectoryInfo dir, List<FileInfo> filesList, int driveIndex, int totalDrives)
        {
            try
            {
                foreach (var file in dir.GetFiles())
                {
                    if (file.Length >= LargeFileThreshold)
                        filesList.Add(file);
                }

                foreach (var subDir in dir.GetDirectories())
                {
                    AddLargeFilesWithProgress(subDir, filesList, driveIndex, totalDrives);
                }
            }
            catch { /* skip inaccessible folders */ }

            int progress = driveIndex * 100 / totalDrives;
            progressCallback?.Invoke(progress);
            Application.DoEvents();
        }
    }

    public class VirusScannerWithProgress
    {
        private readonly string defenderPath = @"C:\Program Files\Windows Defender\MpCmdRun.exe";
        private readonly Action<int> progressCallback;

        public VirusScannerWithProgress(Action<int> progressCallback)
        {
            this.progressCallback = progressCallback;
        }

        public void ScanSystem()
        {
            try
            {
                if (!File.Exists(defenderPath))
                {
                    Logger.Log("Windows Defender executable not found.");
                    return;
                }

                Logger.Log("Starting full system scan...");

                var process = new Process();
                process.StartInfo.FileName = defenderPath;
                process.StartInfo.Arguments = "-Scan -ScanType 2 -DisableRemediation 0";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                // Fake progress while scanning since Defender doesn't report real-time %
                int percent = 0;
                while (!process.HasExited)
                {
                    percent = Math.Min(percent + 2, 99); // increase slowly
                    progressCallback?.Invoke(percent);
                    System.Threading.Thread.Sleep(200);
                }

                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();

                if (!string.IsNullOrWhiteSpace(errors))
                    Logger.Log("Errors during scan: " + errors);

                Logger.Log("Virus scan completed. Output:\n" + output);
                progressCallback?.Invoke(100);
            }
            catch (Exception ex)
            {
                Logger.Log($"Virus scan failed: {ex.Message}");
            }
        }
    }
}
