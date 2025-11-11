using System;
using System.Collections.Generic;
using System.IO;
using HelpCleaner.Utils;

namespace HelpCleaner.Cleaner
{
    public class DangerousFilesManager
    {
        // Threshold for large files (e.g., 500 MB)
        private const long LargeFileSize = 500 * 1024 * 1024;

        public List<string> ListDangerousFiles(string directory)
        {
            var dangerousFiles = new List<string>();
            try
            {
                foreach (var file in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
                {
                    var fileInfo = new FileInfo(file);

                    // Hidden or very large files
                    if ((fileInfo.Attributes & FileAttributes.Hidden) != 0 || fileInfo.Length > LargeFileSize)
                    {
                        dangerousFiles.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error listing dangerous files: {ex.Message}");
            }

            Logger.Log($"Listed {dangerousFiles.Count} dangerous files in {directory}");
            return dangerousFiles;
        }

        public void UserDeleteFile(string filePath)
        {
            // Only deletes if user explicitly decides
            CleanerUtils.DeleteFile(filePath);
        }

        public void UserMoveFile(string sourcePath, string destPath)
        {
            // Only moves if user explicitly decides
            CleanerUtils.MoveFile(sourcePath, destPath);
        }
    }
}
