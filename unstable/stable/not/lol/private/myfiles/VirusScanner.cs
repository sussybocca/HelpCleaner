using System;
using System.Diagnostics;
using HelpCleaner.Utils;

namespace HelpCleaner.Cleaner
{
    public class VirusScanner
    {
        private readonly string defenderPath = @"C:\Program Files\Windows Defender\MpCmdRun.exe";

        /// <summary>
        /// Runs a full system scan and removes detected threats automatically.
        /// </summary>
        public void ScanSystem()
        {
            try
            {
                if (!System.IO.File.Exists(defenderPath))
                {
                    Logger.Log("Windows Defender executable not found.");
                    return;
                }

                Logger.Log("Starting full system scan...");

                var process = new Process();
                process.StartInfo.FileName = defenderPath;

                // Full scan, remove threats automatically
                process.StartInfo.Arguments = "-Scan -ScanType 2 -DisableRemediation 0";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(errors))
                    Logger.Log("Errors during scan: " + errors);

                Logger.Log("Virus scan completed. Output:\n" + output);
            }
            catch (Exception ex)
            {
                Logger.Log($"Virus scan failed: {ex.Message}");
            }
        }
    }
}
