using System;
using System.Collections.Generic;
using System.IO;

namespace HelpCleaner.Cleaner
{
    public class FileScanner
    {
        public List<string> ScanFiles()
        {
            // Example: Scan the system for files to clean
            var files = new List<string>();
            // Add logic to scan for files
            return files;
        }

        public void DeleteFilesWithConfirmation(List<string> files)
        {
            var filesToDelete = new List<string>();
            foreach (var file in files)
            {
                var fi = new FileInfo(file);
                if (fi.Length > 50 * 1024 * 1024)
                {
                    var result = System.Windows.Forms.MessageBox.Show(
                        $"File {fi.FullName} is {fi.Length / (1024*1024)} MB. Delete?",
                        "Confirm Deletion",
                        System.Windows.Forms.MessageBoxButtons.YesNo,
                        System.Windows.Forms.MessageBoxIcon.Warning);

                    if (result == System.Windows.Forms.DialogResult.Yes)
                        filesToDelete.Add(file);
                }
                else
                {
                    filesToDelete.Add(file);
                }
            }

            foreach (var file in filesToDelete)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete {file}: {ex.Message}");
                }
            }
        }
    }
}
