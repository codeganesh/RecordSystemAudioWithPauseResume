using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;

namespace RecordSystemAudioWithPauseResume
{
    public partial class Form1 : Form
    {
        private string? outputFileName;
        private WasapiLoopbackCapture? capture;
        private WaveFileWriter? writer;
        private bool paused = false;
        public Form1()
        {
            InitializeComponent();
            LoadDevices();
        }

        private void LoadDevices()
        {
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            OutputDeviceComboBox.Items.AddRange(devices.ToArray());
            OutputDeviceComboBox.SelectedIndex = 0;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Wave files | *.wav";

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            outputFileName = dialog.FileName;

            var device = (MMDevice)OutputDeviceComboBox.SelectedItem;
            capture = new WasapiLoopbackCapture(device);
            writer = new WaveFileWriter(outputFileName, capture.WaveFormat);
            capture.DataAvailable += OnDataAvailable;
            capture.RecordingStopped += (s, e) =>
            {
                writer.Dispose();
                capture.Dispose();
                StartButton.Enabled = true;
                StopButton.Enabled = false;
                PauseResumeButton.Enabled = false;

                var startInfo = new ProcessStartInfo
                {
                    FileName = Path.GetDirectoryName(outputFileName),
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            };

            capture.StartRecording();
            StartButton.Enabled = false;
            StopButton.Enabled = true;
            PauseResumeButton.Enabled = true;

        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (writer != null)
                writer.Write(e.Buffer, 0, e.BytesRecorded);
        }
        private void StopButton_Click(object sender, EventArgs e)
        {
            if (capture != null)
                capture.StopRecording();
        }

        private void PauseResumeButton_Click(object sender, EventArgs e)
        {

            if (!paused)
            {
                if (capture != null)
                {
                    capture.DataAvailable -= OnDataAvailable;
                    paused = true;
                    PauseResumeButton.Text = "Resume";
                }
            }
            else
            {
                if (capture != null)
                {
                    capture.DataAvailable += OnDataAvailable;
                    paused = false;
                    PauseResumeButton.Text = "Pause";
                }
            }



        }
    }
}