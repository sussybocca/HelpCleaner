using System;
using System.Diagnostics;
using HelpCleaner.Utils;

namespace HelpCleaner.Cleaner
{
    public class VirusScanner
    {
        private readonly string defenderPath = @"C:\Program Files\Windows Defender\MpCmdRun.exe";

        public void ScanSystem()
        {
            try
            {
                if (!System.IO.File.Exists(defenderPath))
                {
                    Logger.Log("Windows Defender executable not found.");
                    return;
                }

                var process = new Process();
                process.StartInfo.FileName = defenderPath;
                process.StartInfo.Arguments = "-Scan -ScanType 2"; // Full system scan
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                Logger.Log("Virus scan completed.");
                Logger.Log(output);
            }
            catch (Exception ex)
            {
                Logger.Log($"Virus scan failed: {ex.Message}");
            }
        }
    }
}
