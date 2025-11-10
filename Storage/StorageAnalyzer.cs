using System;
using System.IO;
using HelpCleaner.Utils;

namespace HelpCleaner.Storage
{
    public class StorageAnalyzer
    {
        public long GetTotalSize(string driveLetter)
        {
            try
            {
                DriveInfo drive = new DriveInfo(driveLetter);
                return drive.TotalSize;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error getting total size for {driveLetter}: {ex.Message}");
                return 0;
            }
        }

        public long GetAvailableFreeSpace(string driveLetter)
        {
            try
            {
                DriveInfo drive = new DriveInfo(driveLetter);
                return drive.AvailableFreeSpace;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error getting free space for {driveLetter}: {ex.Message}");
                return 0;
            }
        }

        public double GetUsedPercentage(string driveLetter)
        {
            long total = GetTotalSize(driveLetter);
            long free = GetAvailableFreeSpace(driveLetter);

            if (total == 0) return 0;

            return ((double)(total - free) / total) * 100;
        }
    }
}
