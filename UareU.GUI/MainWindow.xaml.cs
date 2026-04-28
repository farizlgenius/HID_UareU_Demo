using MaskDetection.Core;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace UareU.GUI
{
    public partial class MainWindow : System.Windows.Window
    {
        private VideoCapture _capture;
        private CancellationTokenSource _cts;
        private MaskDetectorEngine _engine;
        private CascadeClassifier _faceDetector;


        public MainWindow()
        {
            InitializeComponent();

            // load ONNX model once
            _engine = new MaskDetectorEngine("Models/mask_detector.onnx");
            _faceDetector = new CascadeClassifier("Models/haarcascade_frontalface_default.xml");
        }

        // START CAMERA BUTTON
        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            _capture = new VideoCapture(0);

            if (!_capture.IsOpened())
            {
                MessageBox.Show("Camera not found 😢");
                return;
            }

            _capture.FrameWidth = 640;
            _capture.FrameHeight = 480;
            _capture.Fps = 30;
            _capture.AutoFocus = true;

            _cts = new CancellationTokenSource();
            Task.Run(() => CameraLoop(_cts.Token));
        }

        // STOP CAMERA BUTTON
        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();

            _capture?.Release();
            _capture?.Dispose();
            _capture = null;
        }

        // CAMERA LOOP (background thread)
        private void CameraLoop(CancellationToken token)
        {
            int frameCounter = 0;
            string lastResult = "Detecting...";

            using var frame = new Mat();

            Thread.Sleep(1000); // camera warmup

            while (!token.IsCancellationRequested)
            {
                bool success = _capture.Grab();
                if (!success) continue;

                _capture.Retrieve(frame);

                if (frame.Empty())
                    continue;

                frameCounter++;

                // 🔥 Run AI every 10 frames (performance safe)
                if (frameCounter % 10 == 0)
                {
                    OpenCvSharp.Rect[] faces = _faceDetector.DetectMultiScale(
    frame,
    1.1,
    5,
    HaarDetectionTypes.ScaleImage,
    new OpenCvSharp.Size(100, 100));

                    if (faces.Length > 0)
                    {
                        var face = new Mat(frame, faces[0]);
                        lastResult = _engine.Detect(face);
                    }
                    else
                    {
                        lastResult = "No face detected";
                    }
                }

                var imageSource = frame.ToBitmapSource();
                imageSource.Freeze();

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    CameraImage.Source = imageSource;
                    ResultText.Text = lastResult;
                }));
            }
        }
    }
}