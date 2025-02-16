using NAudio.Wave;
using NAudio.Lame;
using NAudio.Wave.SampleProviders;
using System.Drawing.Drawing2D;

namespace AudioRecorder
{
    public partial class MainForm : Form
    {
        private WasapiLoopbackCapture? capture;
        private WaveFileWriter? writer;
        private string? tempWavPath;
        private bool isRecording;
        private readonly Label statusLabel = new();
        private readonly System.Windows.Forms.Timer recordingTimer = new();
        private DateTime recordingStartTime;

        // Embedded icons as base64 strings
        private const string RecordIconBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAAA7AAAAOwBeShxvQAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAAHUSURBVFiF7Ze9ThtBFIW/WQwCUiANosAigYgGhKwUKE+A8gYpkpQUKSLxDqSiSIWUJ0h4A0SVmg4bCrATAU0kaBYTwGDvTrEr4LLZH+wKN/mko5k7Z+Z+OzN3Z1ZUlf+J0LQDqGoHMAHcBw4Bq+0LgE/AO+C1iPyqm0dVGzWgBawDWqN9BFrN5KmxeRwYBb4C74FPwPfkfDdwBhgGHgIDwGERWa0KVtUFj5QvgHFgJ7Vkd4BpoAv4CbSBd8CyiKzFa/uBEeAhcA7YBM6KyHuPvBFgDrgK/AYuiMjXrEiqmgUWPYvmVbXPE7NPVec9sYuqmvn7qnoa+OzZ8RER+ZBnHAKuAVeAXuA5MCkiv3OAvcBt4BZwEvgBXBaRpRzfMWAWGAI2gIsi8jbLNwQ8Ae4BPcBLYEpENnPieoAJ4DFwAPgGXBORN1m+jVTQDPDCKxURWcgBngZmgH5gCbghIu0c31bgJvAU2A8sAw9E5EOe/x5gGrgPHAfeALdFZM0Xn3oKhoGXwFVgFRgVkc85wD7gDnAD2AcsAOMislIQvw94RHQPHCKqkkkR+V0Um/kYAheJxuoZ4ATRS/UCOCci7SLgfwUR2QSmgCngZ9PxfwDWNgpfOYWw5QAAAABJRU5ErkJggg==";
        private const string StopIconBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAAA7AAAAOwBeShxvQAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAABYSURBVFiF7dexDcAgDERRf5ZhGS/DMLl0KVIgubGFoPgnuYGnO8k2iIiIiIhYxd0l6ZS0m9klqf7NzAzA8ny0YNXrv+RR0vHlQl/q3mePpHvqElW1Zw8RsZ0HThwVJ2d0xSYAAAAASUVORK5CYII=";

        public MainForm()
        {
            InitializeComponent();
            SetupTimer();
            SetupUI();
        }

        private static Image CreateImageFromBase64(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            using var ms = new MemoryStream(bytes);
            return Image.FromStream(ms);
        }

        private void SetupTimer()
        {
            recordingTimer.Interval = 1000; // Update every second
            recordingTimer.Tick += RecordingTimer_Tick;
        }

        private void RecordingTimer_Tick(object? sender, EventArgs e)
        {
            var elapsed = DateTime.Now - recordingStartTime;
            statusLabel.Text = $"Recording: {elapsed:hh\\:mm\\:ss}";
        }

        private void SetupUI()
        {
            this.Text = "System Audio Recorder";
            this.Size = new Size(500, 300);
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Main TableLayoutPanel to organize the layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(10),
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Instructions Panel
            var instructionsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(20, 10, 20, 10)
            };

            var instructionsLabel = new Label
            {
                Text = "This application records your system audio (what you hear).\n" +
                      "1. Click Record to start recording\n" +
                      "2. Click Stop when finished\n" +
                      "3. Choose where to save your MP3 file",
                Font = new Font("Segoe UI", 9f),
                Dock = DockStyle.Fill,
                AutoSize = false
            };
            instructionsPanel.Controls.Add(instructionsLabel);

            // Controls Panel
            var controlsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            var btnRecord = new Button
            {
                Name = "Record",
                Text = "Record",
                Image = Image.FromFile("Icons/record.ico"),
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                Size = new Size(140, 40),
                Location = new Point(20, 20),
                FlatStyle = FlatStyle.Standard,
                BackColor = Color.FromArgb(240, 240, 240),
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(10, 0, 10, 0),
                Cursor = Cursors.Hand
            };
            btnRecord.Click += BtnRecord_Click;

            var btnStop = new Button
            {
                Name = "Stop",
                Text = "Stop",
                Image = Image.FromFile("Icons/stop.ico"),
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                Size = new Size(140, 40),
                Location = new Point(180, 20),
                FlatStyle = FlatStyle.Standard,
                BackColor = Color.FromArgb(240, 240, 240),
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(10, 0, 10, 0),
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnStop.Click += BtnStop_Click;

            statusLabel.Text = "Ready to record";
            statusLabel.Font = new Font("Segoe UI", 9f);
            statusLabel.Location = new Point(20, 70);
            statusLabel.AutoSize = true;
            statusLabel.ForeColor = Color.FromArgb(100, 100, 100);

            // Add controls to panels
            controlsPanel.Controls.AddRange(new Control[] { btnRecord, btnStop, statusLabel });

            // Add panels to TableLayoutPanel
            mainLayout.Controls.Add(instructionsPanel, 0, 0);
            mainLayout.Controls.Add(controlsPanel, 0, 1);

            // Add TableLayoutPanel to form
            Controls.Add(mainLayout);
        }

        private void BtnRecord_Click(object? sender, EventArgs e)
        {
            if (!isRecording)
            {
                StartRecording();
                if (sender is Button btn) btn.Enabled = false;
                var stopButton = Controls.Find("Stop", false).FirstOrDefault();
                if (stopButton != null) stopButton.Enabled = true;
                
                recordingStartTime = DateTime.Now;
                recordingTimer.Start();
                statusLabel.Text = "Recording: 00:00:00";
            }
        }

        private void BtnStop_Click(object? sender, EventArgs e)
        {
            if (isRecording && tempWavPath != null)
            {
                StopRecording();
                if (sender is Button btn) btn.Enabled = false;
                var recordButton = Controls.Find("Record", false).FirstOrDefault();
                if (recordButton != null) recordButton.Enabled = true;

                recordingTimer.Stop();
                statusLabel.Text = "Ready to record";

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "MP3 files (*.mp3)|*.mp3",
                    DefaultExt = "mp3",
                    Title = "Save recording as MP3"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    statusLabel.Text = "Converting to MP3...";
                    Application.DoEvents();
                    
                    ConvertToMp3(tempWavPath, saveFileDialog.FileName);
                    File.Delete(tempWavPath);
                    
                    statusLabel.Text = "Ready to record";
                }
            }
        }

        private void StartRecording()
        {
            tempWavPath = Path.Combine(Path.GetTempPath(), "temp_recording.wav");
            capture = new WasapiLoopbackCapture();
            writer = new WaveFileWriter(tempWavPath, capture.WaveFormat);

            capture.DataAvailable += (s, e) =>
            {
                writer?.Write(e.Buffer, 0, e.BytesRecorded);
            };

            capture.RecordingStopped += (s, e) =>
            {
                writer?.Dispose();
                writer = null;
                capture?.Dispose();
                capture = null;
            };

            isRecording = true;
            capture.StartRecording();
        }

        private void StopRecording()
        {
            isRecording = false;
            capture?.StopRecording();
        }

        private void ConvertToMp3(string wavPath, string mp3Path)
        {
            using (var reader = new AudioFileReader(wavPath))
            using (var writer = new LameMP3FileWriter(mp3Path, reader.WaveFormat, LAMEPreset.STANDARD))
            {
                reader.CopyTo(writer);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isRecording)
            {
                StopRecording();
            }
            base.OnFormClosing(e);
        }
    }
} 