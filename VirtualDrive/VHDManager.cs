using System;
using System.IO;
using System.Diagnostics;
using HelpCleaner.Utils;

namespace HelpCleaner.VirtualDrive
{
    public class VHDManager
    {
        private readonly string vhdPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HelpCleaner.vhdx");
        private const long DefaultSizeMB = 1024; // 1 GB

        public void CreateVHD(long sizeMB = DefaultSizeMB)
        {
            try
            {
                if (File.Exists(vhdPath))
                {
                    Logger.Log("VHD already exists.");
                    return;
                }

                // Create VHD using diskpart script
                string script = $"create vdisk file=\"{vhdPath}\" maximum={sizeMB} type=expandable\nattach vdisk\ncreate partition primary\nformat fs=NTFS quick\nassign letter=Z";
                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vhd_script.txt");
                File.WriteAllText(scriptPath, script);

                var process = new Process();
                process.StartInfo.FileName = "diskpart.exe";
                process.StartInfo.Arguments = $"/s \"{scriptPath}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                Logger.Log("VHD created successfully.");
                Logger.Log(output);

                File.Delete(scriptPath); // Clean up
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to create VHD: {ex.Message}");
            }
        }

        public void MountVHD()
        {
            if (!File.Exists(vhdPath))
            {
                Logger.Log("VHD does not exist.");
                return;
            }

            string script = $"select vdisk file=\"{vhdPath}\"\nattach vdisk";
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vhd_mount.txt");
            File.WriteAllText(scriptPath, script);

            var process = new Process();
            process.StartInfo.FileName = "diskpart.exe";
            process.StartInfo.Arguments = $"/s \"{scriptPath}\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Logger.Log("VHD mounted successfully.");
            Logger.Log(output);

            File.Delete(scriptPath);
        }

        public void DismountVHD()
        {
            if (!File.Exists(vhdPath))
            {
                Logger.Log("VHD does not exist.");
                return;
            }

            string script = $"select vdisk file=\"{vhdPath}\"\ndetach vdisk";
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vhd_dismount.txt");
            File.WriteAllText(scriptPath, script);

            var process = new Process();
            process.StartInfo.FileName = "diskpart.exe";
            process.StartInfo.Arguments = $"/s \"{scriptPath}\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Logger.Log("VHD dismounted successfully.");
            Logger.Log(output);

            File.Delete(scriptPath);
        }
    }
}
