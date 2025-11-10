using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using HelpCleaner.Cleaner;
using HelpCleaner.VirtualDrive;
using HelpCleaner.Storage;
using HelpCleaner.Utils; // Needed for CleanerUtils

namespace HelpCleaner.Dashboard
{
    public class DashboardForm : Form
    {
        private Button scanButton;
        private Button cleanButton;
        private Button dangerousFilesButton;
        private Button vhdButton;
        private Label storageLabel;
        private StorageAnalyzer storageAnalyzer;

        public DashboardForm()
        {
            this.Text = "HelpCleaner";
            this.Width = 600;
            this.Height = 450; // Taller to fit storage info
            storageAnalyzer = new StorageAnalyzer();
            InitializeComponents();
            UpdateStorageStats();
        }

        private void InitializeComponents()
        {
            // Buttons
            scanButton = new Button() { Text = "Check My System for Viruses", Left = 50, Top = 50, Width = 200 };
            cleanButton = new Button() { Text = "Clean System", Left = 50, Top = 100, Width = 200 };
            dangerousFilesButton = new Button() { Text = "Access Dangerous Files", Left = 50, Top = 150, Width = 200 };
            vhdButton = new Button() { Text = "Manage Virtual Drive", Left = 50, Top = 200, Width = 200 };

            scanButton.Click += ScanButton_Click;
            cleanButton.Click += CleanButton_Click;
            dangerousFilesButton.Click += DangerousFilesButton_Click;
            vhdButton.Click += VhdButton_Click;

            // Storage info label
            storageLabel = new Label()
            {
                Left = 300,
                Top = 50,
                Width = 250,
                Height = 300,
                Font = new Font("Arial", 10),
                Text = "Loading storage info..."
            };

            this.Controls.Add(scanButton);
            this.Controls.Add(cleanButton);
            this.Controls.Add(dangerousFilesButton);
            this.Controls.Add(vhdButton);
            this.Controls.Add(storageLabel);
        }

        // Virus scan button
        private void ScanButton_Click(object? sender, EventArgs? e)
        {
            var virusScanner = new VirusScanner();
            virusScanner.ScanSystem();
            MessageBox.Show("Virus scan completed. Check HelpCleaner.log for details.");
        }

        // Clean system button
        private void CleanButton_Click(object? sender, EventArgs? e)
        {
            var scanner = new FileScanner();

            // Scan entire system
            var files = scanner.ScanFiles();

            // Ask confirmation before deleting
            scanner.DeleteFilesWithConfirmation(files);

            MessageBox.Show($"Deleted {files.Count} files (or skipped some). Check HelpCleaner.log for details.");
        }

        // Dangerous files button
        private void DangerousFilesButton_Click(object? sender, EventArgs? e)
        {
            var manager = new DangerousFilesManager();
            string path = "C:\\"; // Or allow user to pick a path
            var files = manager.ListDangerousFiles(path);

            string message = files.Count > 0
                ? $"Found {files.Count} dangerous files. See HelpCleaner.log for details."
                : "No dangerous files found.";

            MessageBox.Show(message);
        }

        // Virtual drive button
        private void VhdButton_Click(object? sender, EventArgs? e)
        {
            var vhdManager = new VHDManager();
            vhdManager.CreateVHD();
            vhdManager.MountVHD();

            MessageBox.Show("Virtual Drive created and mounted. Check HelpCleaner.log for details.");
        }

        // Storage info
        private void UpdateStorageStats()
        {
            string drivesInfo = "";
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    double usedPct = storageAnalyzer.GetUsedPercentage(drive.Name);
                    drivesInfo += $"{drive.Name} - Used: {usedPct:F2}% ({FormatBytes(drive.TotalSize - drive.AvailableFreeSpace)} / {FormatBytes(drive.TotalSize)})\n";
                }
            }
            storageLabel.Text = drivesInfo;
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:F2} {sizes[order]}";
        }
    }
}
