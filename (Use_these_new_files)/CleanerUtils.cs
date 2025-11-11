using System;
using System.IO;
using HelpCleaner.Utils;

namespace HelpCleaner.Cleaner
{
    public static class CleanerUtils
    {
        public static bool DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Logger.Log($"Deleted file: {filePath}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to delete {filePath}: {ex.Message}");
                return false;
            }
        }

        public static bool MoveFile(string sourcePath, string destPath)
        {
            try
            {
                if (File.Exists(sourcePath))
                {
                    var destDir = Path.GetDirectoryName(destPath);
                    if (!Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);

                    File.Move(sourcePath, destPath);
                    Logger.Log($"Moved file: {sourcePath} -> {destPath}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to move {sourcePath}: {ex.Message}");
                return false;
            }
        }
    }
}
