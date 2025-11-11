using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace HelpCleaner.Cleaner
{
    public class FileScanner
    {
        private const long LargeFileThreshold = 1L * 1024 * 1024 * 1024; // 1 GB
        private readonly string logFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "HelpCleanerDeletedFiles.log");

        /// <summary>
        /// Scan all drives for files >= 1 GB, show progress, ask confirmation, then delete.
        /// </summary>
        public void ScanAndDelete()
        {
            var largeFiles = new List<FileInfo>();
            int driveIndex = 0;

            // Step 1: Scan all drives with progress
            foreach (var drive in DriveInfo.GetDrives())
            {
                driveIndex++;
                if (!drive.IsReady) continue;

                try
                {
                    AddLargeFilesWithProgress(drive.RootDirectory, largeFiles, driveIndex, DriveInfo.GetDrives().Length);
                }
                catch { /* skip inaccessible drives */ }
            }

            if (largeFiles.Count == 0)
            {
                MessageBox.Show("No files larger than 1 GB were found.", "Scan Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Step 2: Build message for user
            string message = "The following files are larger than 1 GB:\n\n";
            foreach (var fi in largeFiles)
            {
                message += $"{fi.FullName} ({fi.Length / (1024 * 1024 * 1024)} GB)\n";
            }
            message += "\n⚠ Are you sure you want to delete these files? Make a backup drive first!";

            // Step 3: Confirm deletion
            var confirm = MessageBox.Show(message, "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes)
            {
                MessageBox.Show("Operation cancelled. No files were deleted.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Step 4: Delete files and log
            int deletedCount = 0;
            long deletedBytes = 0;
            using (StreamWriter log = new StreamWriter(logFile, true))
            {
                foreach (var fi in largeFiles)
                {
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
                }
            }

            // Step 5: Show summary
            MessageBox.Show(
                $"Deleted {deletedCount} files, totaling {deletedBytes / (1024 * 1024 * 1024)} GB.\n" +
                $"Details logged to {logFile}.",
                "Cleanup Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        /// <summary>
        /// Recursively scan a directory for large files, showing a progress message box.
        /// </summary>
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

            // Update user with progress for large scans
            Application.DoEvents();
            string progressMessage = $"Scanning drive {dir.Root.FullName} ({driveIndex}/{totalDrives})...\nFound {filesList.Count} large files so far.";
            Console.WriteLine(progressMessage); // optional: for console
        }
    }
}
