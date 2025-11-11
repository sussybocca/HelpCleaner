using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using HelpCleaner.Cleaner;
using HelpCleaner.VirtualDrive;
using HelpCleaner.Storage;
using HelpCleaner.Utils;

namespace HelpCleaner.Dashboard
{
    public class DashboardForm : Form
    {
        private Button? scanButton;
        private Button? cleanButton;
        private Button? dangerousFilesButton;
        private Button? vhdButton;
        private Label? storageLabel;
        private StorageAnalyzer storageAnalyzer;

        // Emoji buttons
        private Button? btnClose;
        private Button? btnMinimize;
        private Button? btnInstall;

        // Dragging variables
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        public DashboardForm()
        {
            this.Text = "HelpCleaner";
            this.Width = 600;
            this.Height = 450; // Taller to fit storage info
            this.FormBorderStyle = FormBorderStyle.None; // Borderless for draggable feature

            storageAnalyzer = new StorageAnalyzer();

            InitializeComponents();
            UpdateStorageStats();
        }

        private void InitializeComponents()
        {
            // --- Old working buttons ---
            scanButton = new Button() { Text = "Check My System for Viruses", Left = 50, Top = 50, Width = 200 };
            cleanButton = new Button() { Text = "Clean System", Left = 50, Top = 100, Width = 200 };
            dangerousFilesButton = new Button() { Text = "Access Dangerous Files", Left = 50, Top = 150, Width = 200 };
            vhdButton = new Button() { Text = "Manage Virtual Drive", Left = 50, Top = 200, Width = 200 };

            scanButton.Click += ScanButton_Click;
            cleanButton.Click += CleanButton_Click;
            dangerousFilesButton.Click += DangerousFilesButton_Click;
            vhdButton.Click += VhdButton_Click;

            this.Controls.Add(scanButton);
            this.Controls.Add(cleanButton);
            this.Controls.Add(dangerousFilesButton);
            this.Controls.Add(vhdButton);

            // --- Storage label ---
            storageLabel = new Label()
            {
                Left = 300,
                Top = 50,
                Width = 250,
                Height = 300,
                Font = new Font("Arial", 10),
                Text = "Loading storage info..."
            };
            this.Controls.Add(storageLabel);

            // --- Emoji buttons ---
            btnClose = new Button() { Text = "❌", Width = 40, Height = 30, Top = 10, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent };
            btnClose.Click += (s, e) => this.Close();
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.Red;
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent;

            btnMinimize = new Button() { Text = "➖", Width = 40, Height = 30, Top = 10, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent };
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            btnMinimize.MouseEnter += (s, e) => btnMinimize.BackColor = Color.Gray;
            btnMinimize.MouseLeave += (s, e) => btnMinimize.BackColor = Color.Transparent;

            btnInstall = new Button() { Text = "🛠 Install", Width = 60, Height = 30, Top = 10, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent };
            btnInstall.Click += (s, e) => MessageBox.Show("Installer requires elevated permissions.", "Install", MessageBoxButtons.OK, MessageBoxIcon.Information);
            btnInstall.MouseEnter += (s, e) => btnInstall.BackColor = Color.DarkOrange;
            btnInstall.MouseLeave += (s, e) => btnInstall.BackColor = Color.Transparent;

            this.Controls.Add(btnClose);
            this.Controls.Add(btnMinimize);
            this.Controls.Add(btnInstall);

            // --- Drag window events ---
            this.MouseDown += DashboardForm_MouseDown;
            this.MouseMove += DashboardForm_MouseMove;
            this.MouseUp += DashboardForm_MouseUp;

            // --- Adjust emoji buttons on resize ---
            this.Resize += (s, e) =>
            {
                if (btnClose != null) btnClose.Left = this.ClientSize.Width - 50;
                if (btnMinimize != null) btnMinimize.Left = this.ClientSize.Width - 100;
                if (btnInstall != null) btnInstall.Left = this.ClientSize.Width - 170;
            };
        }

        // --- Button handlers ---
        private void ScanButton_Click(object? sender, EventArgs e)
        {
            var virusScanner = new VirusScanner();
            virusScanner.ScanSystem();
            MessageBox.Show("Virus scan completed. Check HelpCleaner.log for details.");
        }

        private void CleanButton_Click(object? sender, EventArgs e)
        {
            var scanner = new FileScanner();
            scanner.ScanAndDelete(); // new unified method handles everything
        }

        private void DangerousFilesButton_Click(object? sender, EventArgs e)
        {
            var manager = new DangerousFilesManager();
            string path = "C:\\";
            var files = manager.ListDangerousFiles(path);
            string message = files.Count > 0 ? $"Found {files.Count} dangerous files. See HelpCleaner.log for details." : "No dangerous files found.";
            MessageBox.Show(message);
        }

        private void VhdButton_Click(object? sender, EventArgs e)
        {
            var vhdManager = new VHDManager();
            vhdManager.CreateVHD();
            vhdManager.MountVHD();
            MessageBox.Show("Virtual Drive created and mounted. Check HelpCleaner.log for details.");
        }

        private void UpdateStorageStats()
        {
            if (storageLabel == null) return;

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

        // --- Draggable window methods ---
        private void DashboardForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void DashboardForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (!dragging) return;
            Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
            this.Location = Point.Add(dragFormPoint, new Size(diff));
        }

        private void DashboardForm_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
    }
}
