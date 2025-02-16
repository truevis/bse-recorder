using NAudio.Wave;
using NAudio.Lame;
using NAudio.Wave.SampleProviders;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

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
        private bool isPaused;
        private WaveFileReader? playbackReader;
        private WaveOut? playbackDevice;
        private FlowLayoutPanel? buttonPanel;

        public MainForm()
        {
            InitializeComponent();
            SetupTimer();
            SetupUI();
            UpdateButtonStates();
        }

        private static Image CreateImageFromBase64(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            using var ms = new MemoryStream(bytes);
            return Image.FromStream(ms);
        }

        private void SetupTimer()
        {
            recordingTimer.Interval = 1000;
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
            this.MinimumSize = new Size(500, 300);
            this.Size = new Size(600, 400);
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterScreen;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

            var instructionsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10),
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(20)
            };

            var instructionsLabel = new Label
            {
                Text = "This application records your system audio (what you hear).\n" +
                      "1. Click Record to start recording\n" +
                      "2. Click Stop when finished\n" +
                      "3. Choose where to save your MP3 file",
                Font = new Font("Segoe UI", 10f),
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };
            instructionsPanel.Controls.Add(instructionsLabel);

            buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                Padding = new Padding(20),
                Margin = new Padding(0, 10, 0, 10),
                BackColor = Color.White
            };

            buttonPanel.SizeChanged += (s, e) =>
            {
                if (buttonPanel.Controls.Count > 0)
                {
                    int totalWidth = buttonPanel.Controls.Cast<Control>().Sum(c => c.Width + c.Margin.Horizontal);
                    int x = (buttonPanel.Width - totalWidth) / 2;
                    int y = (buttonPanel.Height - buttonPanel.Controls[0].Height) / 2;
                    
                    foreach (Control control in buttonPanel.Controls)
                    {
                        control.Location = new Point(x, y);
                        x += control.Width + control.Margin.Horizontal;
                    }
                }
            };

            var statusPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 245, 245),
                Margin = new Padding(0, 10, 0, 0)
            };

            statusLabel.Text = "Ready to record";
            statusLabel.Dock = DockStyle.Fill;
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            statusLabel.Font = new Font("Segoe UI", 10f);
            statusPanel.Controls.Add(statusLabel);

            CreateButton("Record", "rec-button", BtnRecord_Click);
            CreateButton("Pause", "pause-button", BtnPause_Click, false);
            CreateButton("Stop", "stop-button", BtnStop_Click, false);
            CreateButton("Play", "play-button", BtnPlay_Click, false);
            CreateButton("Save", "save-button", BtnSave_Click, false);

            mainLayout.Controls.Add(instructionsPanel, 0, 0);
            mainLayout.Controls.Add(buttonPanel, 0, 1);
            mainLayout.Controls.Add(statusPanel, 0, 2);

            this.Controls.Add(mainLayout);
        }

        private void CreateButton(string name, string iconName, EventHandler clickHandler, bool enabled = true)
        {
            var button = new Button
            {
                Name = name,
                Text = "",
                Size = new Size(56, 56),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ImageAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(8),
                Enabled = enabled,
                Cursor = Cursors.Hand
            };

            // Load image from embedded resource
            using (var stream = GetType().Assembly.GetManifestResourceStream($"AudioRecorder.Images.{iconName}.png"))
            {
                if (stream != null)
                {
                    using (var originalImage = Image.FromStream(stream))
                    {
                        var destRect = new Rectangle(0, 0, 40, 40);
                        var destImage = new Bitmap(40, 40);

                        destImage.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);

                        using (var graphics = Graphics.FromImage(destImage))
                        {
                            graphics.CompositingMode = CompositingMode.SourceCopy;
                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = SmoothingMode.HighQuality;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                            using (var wrapMode = new ImageAttributes())
                            {
                                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                                graphics.DrawImage(originalImage, destRect, 0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, wrapMode);
                            }
                        }

                        button.Image = destImage;
                    }
                }
            }

            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(230, 230, 230);

            button.MouseEnter += (s, e) => {
                if (button.Enabled)
                    button.BackColor = Color.FromArgb(245, 245, 245);
            };
            button.MouseLeave += (s, e) => {
                if (button.Enabled)
                    button.BackColor = Color.White;
            };

            button.Click += clickHandler;
            buttonPanel?.Controls.Add(button);
        }

        private void BtnRecord_Click(object? sender, EventArgs e)
        {
            if (isRecording) return;

            recordingStartTime = DateTime.Now;
            StartRecording();
            recordingTimer.Start();
            UpdateButtonStates();
        }

        private void BtnStop_Click(object? sender, EventArgs e)
        {
            if (!isRecording) return;
            
            StopRecording();
            recordingTimer.Stop();
            statusLabel.Text = "Ready to record";
            UpdateButtonStates();
        }

        private void BtnPause_Click(object? sender, EventArgs e)
        {
            if (!isRecording) return;

            isPaused = !isPaused;
            if (isPaused)
            {
                capture?.StopRecording();
                statusLabel.Text = "Recording paused";
                recordingTimer.Stop();
            }
            else
            {
                StartRecording();
                statusLabel.Text = $"Recording: {DateTime.Now - recordingStartTime:hh\\:mm\\:ss}";
                recordingTimer.Start();
            }
            UpdateButtonStates();
        }

        private void StartRecording()
        {
            try
            {
                if (!isPaused)
                {
                    tempWavPath = Path.Combine(Path.GetTempPath(), "temp_recording.wav");
                    capture = new WasapiLoopbackCapture();
                    writer = new WaveFileWriter(tempWavPath, capture.WaveFormat);

                    capture.DataAvailable += Capture_DataAvailable;
                    capture.RecordingStopped += Capture_RecordingStopped;
                }

                isRecording = true;
                capture?.StartRecording();
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting recording: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                StopRecording();
            }
        }

        private void Capture_DataAvailable(object? sender, WaveInEventArgs e)
        {
            if (writer != null)
            {
                writer.Write(e.Buffer, 0, e.BytesRecorded);
            }
        }

        private void Capture_RecordingStopped(object? sender, StoppedEventArgs e)
        {
            if (!isPaused)
            {
                writer?.Dispose();
                writer = null;
                capture?.Dispose();
                capture = null;
            }
        }

        private void StopRecording()
        {
            try
            {
                isRecording = false;
                isPaused = false;
                
                capture?.StopRecording();
                writer?.Dispose();
                writer = null;
                capture?.Dispose();
                capture = null;
                
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping recording: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateButtonStates()
        {
            if (buttonPanel == null) return;

            var recordButton = buttonPanel.Controls["Record"] as Button;
            var pauseButton = buttonPanel.Controls["Pause"] as Button;
            var stopButton = buttonPanel.Controls["Stop"] as Button;
            var playButton = buttonPanel.Controls["Play"] as Button;
            var saveButton = buttonPanel.Controls["Save"] as Button;

            if (recordButton != null) recordButton.Enabled = !isRecording;
            if (pauseButton != null) pauseButton.Enabled = isRecording;
            if (stopButton != null) stopButton.Enabled = isRecording;
            if (playButton != null) playButton.Enabled = !isRecording && tempWavPath != null && File.Exists(tempWavPath);
            if (saveButton != null) saveButton.Enabled = !isRecording && tempWavPath != null && File.Exists(tempWavPath);
        }

        private void BtnPlay_Click(object? sender, EventArgs e)
        {
            if (playbackDevice?.PlaybackState == PlaybackState.Playing)
            {
                playbackDevice?.Stop();
                return;
            }

            if (tempWavPath != null && File.Exists(tempWavPath))
            {
                try
                {
                    playbackReader?.Dispose();
                    playbackDevice?.Dispose();

                    playbackReader = new WaveFileReader(tempWavPath);
                    playbackDevice = new WaveOut();
                    playbackDevice.Init(playbackReader);
                    playbackDevice.Play();
                    statusLabel.Text = "Playing recording...";

                    playbackDevice.PlaybackStopped += (s, args) =>
                    {
                        if (IsDisposed) return;
                        Invoke(() => statusLabel.Text = "Ready to record");
                    };
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error playing recording: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (tempWavPath == null || !File.Exists(tempWavPath)) return;

            using var saveFileDialog = new SaveFileDialog
            {
                Filter = "MP3 files (*.mp3)|*.mp3",
                DefaultExt = "mp3",
                Title = "Save recording as MP3"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    statusLabel.Text = "Converting to MP3...";
                    Application.DoEvents();
                    ConvertToMp3(tempWavPath, saveFileDialog.FileName);
                    statusLabel.Text = "Ready to record";
                    MessageBox.Show("Recording saved successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving recording: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statusLabel.Text = "Error saving recording";
                }
            }
        }

        private void ConvertToMp3(string wavPath, string mp3Path)
        {
            using var reader = new AudioFileReader(wavPath);
            using var writer = new LameMP3FileWriter(mp3Path, reader.WaveFormat, LAMEPreset.STANDARD);
            reader.CopyTo(writer);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isRecording)
            {
                StopRecording();
            }

            if (capture != null)
            {
                capture.DataAvailable -= Capture_DataAvailable;
                capture.RecordingStopped -= Capture_RecordingStopped;
            }

            playbackReader?.Dispose();
            playbackDevice?.Dispose();

            if (tempWavPath != null && File.Exists(tempWavPath))
            {
                try
                {
                    File.Delete(tempWavPath);
                }
                catch
                {
                    // Ignore cleanup errors on exit
                }
            }

            base.OnFormClosing(e);
        }
    }
} 