using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HelpCleaner.Cleaner
{
    public class FileScanner
    {
        private readonly string systemDrive = @"C:\";

        // Minimum file size to prompt for deletion (in bytes)
        private const long LargeFileThreshold = 50 * 1024 * 1024; // 50 MB

        // Folders to exclude for safety
        private readonly string[] excludedDirectories = new string[]
        {
            @"C:\Windows",
            @"C:\Program Files",
            @"C:\Program Files (x86)"
        };

        public List<string> ScanFiles()
        {
            var files = new List<string>();
            ScanDirectory(systemDrive, files);
            return files;
        }

        private void ScanDirectory(string path, List<string> files)
        {
            try
            {
                if (excludedDirectories.Any(e => path.StartsWith(e, StringComparison.OrdinalIgnoreCase)))
                    return;

                // Add files in this directory
                files.AddRange(Directory.GetFiles(path));

                // Recurse into subdirectories
                foreach (var dir in Directory.GetDirectories(path))
                {
                    ScanDirectory(dir, files);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied to {path}, skipping.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning {path}: {ex.Message}");
            }
        }

        public void DeleteFilesWithConfirmation(List<string> files)
        {
            var filesToDelete = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var fi = new FileInfo(file);
                    if (!fi.Exists) continue;

                    if (fi.Length > LargeFileThreshold)
                    {
                        var result = MessageBox.Show(
                            $"File {fi.FullName} is {fi.Length / (1024 * 1024)} MB. Delete?",
                            "Confirm Deletion",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (result == DialogResult.Yes)
                            filesToDelete.Add(file);
                    }
                    else
                    {
                        filesToDelete.Add(file);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to process {file}: {ex.Message}");
                }
            }

            foreach (var file in filesToDelete)
            {
                try
                {
                    File.Delete(file);
                    Console.WriteLine($"Deleted: {file}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete {file}: {ex.Message}");
                }
            }
        }
    }
}
