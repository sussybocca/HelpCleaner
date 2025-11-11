using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using HelpCleaner.Cleaner;     // <-- FileScannerWithProgress & VirusScannerWithProgress
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

        private CircularProgressBar? progressBar;

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
            this.Height = 450;
            this.FormBorderStyle = FormBorderStyle.None;

            storageAnalyzer = new StorageAnalyzer();

            InitializeComponents();
            UpdateStorageStats();
        }

        private void InitializeComponents()
        {
            // --- Circular progress bar ---
            progressBar = new CircularProgressBar()
            {
                Location = new Point(20, 50),
                Size = new Size(100, 100),
                BackColor = Color.LightGray
            };
            this.Controls.Add(progressBar);

            // --- Buttons ---
            scanButton = new Button() { Text = "Check My System for Viruses", Left = 150, Top = 50, Width = 200 };
            cleanButton = new Button() { Text = "Clean System", Left = 150, Top = 100, Width = 200 };
            dangerousFilesButton = new Button() { Text = "Access Dangerous Files", Left = 150, Top = 150, Width = 200 };
            vhdButton = new Button() { Text = "Manage Virtual Drive", Left = 150, Top = 200, Width = 200 };

            scanButton.Click += async (s, e) => await RunVirusScanAsync();
            cleanButton.Click += async (s, e) => await RunFileCleanAsync();
            dangerousFilesButton.Click += DangerousFilesButton_Click;
            vhdButton.Click += VhdButton_Click;

            this.Controls.Add(scanButton);
            this.Controls.Add(cleanButton);
            this.Controls.Add(dangerousFilesButton);
            this.Controls.Add(vhdButton);

            // --- Storage label ---
            storageLabel = new Label()
            {
                Left = 370,
                Top = 50,
                Width = 200,
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

            // --- Drag window ---
            this.MouseDown += DashboardForm_MouseDown;
            this.MouseMove += DashboardForm_MouseMove;
            this.MouseUp += DashboardForm_MouseUp;

            this.Resize += (s, e) =>
            {
                if (btnClose != null) btnClose.Left = this.ClientSize.Width - 50;
                if (btnMinimize != null) btnMinimize.Left = this.ClientSize.Width - 100;
                if (btnInstall != null) btnInstall.Left = this.ClientSize.Width - 170;
            };
        }

        private async Task RunVirusScanAsync()
        {
            if (progressBar != null) progressBar.SetValue(0);
            scanButton!.Enabled = false;

            await Task.Run(() =>
            {
                var scanner = new VirusScannerWithProgress(progress =>
                {
                    this.Invoke(() => progressBar?.SetValue(progress));
                });
                scanner.ScanSystem();
            });

            scanButton!.Enabled = true;
            progressBar?.SetValue(100);
            MessageBox.Show("Virus scan completed. Check HelpCleaner.log for details.");
        }

        private async Task RunFileCleanAsync()
        {
            if (progressBar != null) progressBar.SetValue(0);
            cleanButton!.Enabled = false;

            await Task.Run(() =>
            {
                var scanner = new FileScannerWithProgress(progress =>
                {
                    this.Invoke(() => progressBar?.SetValue(progress));
                });
                scanner.ScanAndDelete();
            });

            cleanButton!.Enabled = true;
            progressBar?.SetValue(100);
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

        // --- Dragging ---
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

    /// <summary>
    /// Designer-safe CircularProgressBar
    /// </summary>
    public class CircularProgressBar : Control
    {
        private int _value;
        private Color _progressColor = Color.DodgerBlue;

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int Value => _value;

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Color ProgressColor => _progressColor;

        public void SetValue(int value)
        {
            _value = Math.Max(0, Math.Min(100, value));
            Invalidate();
        }

        public void SetColor(Color color)
        {
            _progressColor = color;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (var bgPen = new Pen(Color.LightGray, 10))
                g.DrawEllipse(bgPen, 5, 5, Width - 10, Height - 10);

            using (var fgPen = new Pen(_progressColor, 10))
                g.DrawArc(fgPen, 5, 5, Width - 10, Height - 10, -90, 360 * _value / 100);

            string text = $"{_value}%";
            var font = new Font("Arial", 14, FontStyle.Bold);
            var textSize = g.MeasureString(text, font);
            g.DrawString(text, font, Brushes.Black, (Width - textSize.Width) / 2, (Height - textSize.Height) / 2);
        }
    }
}
