using Accord.Video;
using Accord.Video.DirectShow;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UareU
{
    public class VideoStream
    {
        private VideoCaptureDevice _videoSource;
        private static System.Windows.Controls.Image _imageTarget;
        public string _cameraName = "";
        private Bitmap _currentFrame;

        // ⭐ FPS COUNTER
        private Stopwatch _fpsWatch = new Stopwatch();
        private int _frameCounter = 0;
        public double CurrentFps = 0;

        // ⭐ Optional event to show FPS on UI
        public event Action<double> OnFpsUpdated;

        #region START STREAM
        public void StartStreamingVideo(System.Windows.Controls.Image target)
        {
            _imageTarget = target;

            try
            {
                var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (videoDevices.Count == 0)
                {
                    MessageBox.Show("No camera found");
                    return;
                }

                // Select camera (prefer UVC camera)
                foreach (var cam in videoDevices)
                {
                    Console.WriteLine("Found camera: " + cam.Name);

                    if (cam.Name.Contains("UVC"))
                        _cameraName = cam.Name;
                }

                if (_cameraName == "")
                    _cameraName = videoDevices[0].Name;

                Console.WriteLine("Using camera: " + _cameraName);

                string videoCameraMoniker =
                    videoDevices.Where(d => d.Name == _cameraName).Single().MonikerString;

                _videoSource = new VideoCaptureDevice(videoCameraMoniker);

                // 🔥 SHOW ALL SUPPORTED FPS / RESOLUTIONS
                Console.WriteLine("==== Supported Camera Modes ====");
                foreach (var cap in _videoSource.VideoCapabilities)
                {
                    Console.WriteLine(
                        $"{cap.FrameSize.Width}x{cap.FrameSize.Height} @ {cap.MaximumFrameRate} FPS");
                }

                // 🔥 AUTO SELECT HIGHEST FPS MODE
                var bestMode = _videoSource.VideoCapabilities
                                .OrderByDescending(c => c.MaximumFrameRate)
                                .First();

                _videoSource.VideoResolution = bestMode;

                Console.WriteLine($"Selected Mode: {bestMode.FrameSize.Width}x{bestMode.FrameSize.Height} @ {bestMode.MaximumFrameRate} FPS");

                _videoSource.NewFrame += HandleNewVideoFrame;
                _videoSource.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Start Streaming Video exception: " + ex.Message);
            }
        }
        #endregion

        #region STOP STREAM
        public void StopStreamingVideo()
        {
            try
            {
                if (_videoSource != null && _videoSource.IsRunning)
                {
                    _videoSource.SignalToStop();
                    _videoSource.WaitForStop();
                    _videoSource = null;
                }
            }
            catch { }
        }
        #endregion

        #region CAPTURE IMAGE
        public byte[] CaptureImage()
        {
            if (_currentFrame == null)
            {
                MessageBox.Show("No frame to capture.");
                return null;
            }

            Bitmap clonedBitmap = (Bitmap)_currentFrame.Clone();

            using (MemoryStream stream = new MemoryStream())
            {
                clonedBitmap.Save(stream, ImageFormat.Jpeg);
                return stream.ToArray();
            }
        }
        #endregion

        #region HANDLE FRAME (🔥 FPS ADDED HERE)
        private void HandleNewVideoFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // ===== FPS COUNTER =====
            if (!_fpsWatch.IsRunning)
                _fpsWatch.Start();

            _frameCounter++;

            if (_fpsWatch.ElapsedMilliseconds >= 1000)
            {
                CurrentFps = _frameCounter / (_fpsWatch.ElapsedMilliseconds / 1000.0);
                Console.WriteLine($"🔥 REAL FPS: {CurrentFps:F2}");

                OnFpsUpdated?.Invoke(CurrentFps);

                _frameCounter = 0;
                _fpsWatch.Restart();
            }
            // =======================

            _currentFrame?.Dispose();

            Bitmap bmp = (Bitmap)eventArgs.Frame.Clone();
            bmp.SetResolution(96, 96);
            _currentFrame = (Bitmap)bmp.Clone();

            if (App.Current == null) return;

            App.Current.Dispatcher.Invoke(() =>
            {
                _imageTarget.Source = BitmapToImageSource(bmp);
            });
        }
        #endregion

        #region CONVERT BITMAP → WPF IMAGE
        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;

                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
        #endregion
    }
}