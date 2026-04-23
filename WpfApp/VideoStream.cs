using Accord.Imaging.Filters;
using Accord.Video;
using Accord.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UareU.Helpers;
using static HFApiWrapper.HFTypesManagedEquivalents;


namespace UareU
{
    public class VideoStream
    {
        private System.Drawing.Rectangle cropArea;
        private VideoCaptureDevice _videoSource;
        private static System.Windows.Controls.Image _imageTarget;
        public string _cameraName = "";
        private Bitmap _currentFrame;

        #region Start And Stop Streaming Function
        public void StartStreamingVideo(System.Windows.Controls.Image target)
        {
            _imageTarget = target;

            try
            {
                // Get the list of video camera attached to this computer
                var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                foreach(var cam in videoDevices)
                {
                    Console.WriteLine(cam.Name);
                    if(cam.Name == "UVC Camera")
                    {
                        _cameraName = "UVC Camera";
                    }
                    else
                    {
                        _cameraName = "Integrated Camera";
                    }
                }
                Console.WriteLine(_cameraName);

                // Get moniker for our video camera
                string videoCameraMoniker = videoDevices.Where(d => d.Name == _cameraName).Single().MonikerString;

                // Create our capture device
                _videoSource = new VideoCaptureDevice(videoCameraMoniker);

                // Assign the event handler that will be called everytime
                _videoSource.NewFrame += HandleNewVideoFrame;

                // Start the streaming
                _videoSource.Start();

            }
            catch (Exception ex) 
            {
                Console.WriteLine("Start Streaming Video exception :" + ex.Message);
            }
        }// StartStreamingVideo()

        public void StopStreamingVideo() 
        {
            if(_videoSource != null)
            {
                _videoSource.SignalToStop();
            }
            
        }

        public byte[] CaptureImage() 
        {
            Bitmap clonedBitmap;
            byte[] result;
            if (_currentFrame != null)
            {
                clonedBitmap = (Bitmap)_currentFrame.Clone();
                using (MemoryStream stream = new MemoryStream())
                {
                    clonedBitmap.Save(stream, ImageFormat.Jpeg);
                    result = stream.ToArray();
                }
                clonedBitmap.Dispose();
                _currentFrame.Dispose();
                return result;
            }
            else
            {
                MessageBox.Show("No frame to capture.");
                return null;
            }
        }

        public byte[] CropImage(HFPoint[] position, string path, byte[] imageData,int width = 650,int height = 650)
        {
            byte[] result;
            Console.WriteLine("Capturing...");
            int x1 = position[0].x; // top-left X
            int y1 = position[0].y;  // top-left Y
            int x2 = position[1].x; // bottom-right X
            int y2 = position[1].y; // bottom-right Y

            cropArea = new System.Drawing.Rectangle(x: x1-100, y: y1-100, width: width, height: height);
            try
            {
                string file = path + $"\\{DateTime.Now.ToString("yyyy-MM-dd-HHmmss")}.png";
                if (imageData != null)
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    if (File.Exists(file))
                        File.Delete(file);

                    Crop cropFilter = new Crop(cropArea);

                    Bitmap croppedImage = cropFilter.Apply(Helper.ByteToBitMap(imageData));

                    croppedImage.Save(file, ImageFormat.Png);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        //croppedImage.Save(stream, ImageFormat.Jpeg);
                        result = stream.ToArray();
                    }
                    croppedImage.Dispose();
                    return result;
                }
                else
                {
                    MessageBox.Show("No frame to capture.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show($"Failed to save image: {ex.Message}");
                return null;
            }
        } 

        public byte[] CaptureAndCropImage(HFPoint[] position,string path)
        {
            byte[] result;
            Console.WriteLine("Capturing...");
            int x1 = position[0].x + 25; // top-left X
            int y1 = position[0].y + 25;  // top-left Y
            int x2 = position[1].x + 25; // bottom-right X
            int y2 = position[1].y + 25; // bottom-right Y

            int width = x2 - x1;
            int height = y2 - y1;

            cropArea = new System.Drawing.Rectangle(x:x1,y:y1, width: width, height: height);
            try
            {
                if (_currentFrame != null)
                {
                    if (File.Exists(path))
                        File.Delete(path);

                    Crop cropFilter = new Crop(cropArea);
                    Bitmap croppedImage = cropFilter.Apply(_currentFrame);

                    croppedImage.Save(path, ImageFormat.Jpeg);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        croppedImage.Save(stream, ImageFormat.Jpeg);
                        result = stream.ToArray();
                    }
                    croppedImage.Dispose();
                    return result;
                }
                else
                {
                    MessageBox.Show("No frame to capture.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save image: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Handle each new video frame
        private void HandleNewVideoFrame(object sender,NewFrameEventArgs eventArgs)
        {
            // Capture Image
            _currentFrame?.Dispose(); // Dispose previous frame to avoid memory leak

            Bitmap bmp = (Bitmap)eventArgs.Frame;

            bmp.SetResolution(96, 96);

            _currentFrame = (Bitmap)bmp.Clone();
            try
            {
                if (App.Current == null)
                {
                    return;
                }
                else
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            // Convert the image received from the camera to a format that is compatible with our GUI
                            ImageSource img = BitmapToImageSource(bmp);
                            // Display the new image on the GUI
                            _imageTarget.Source = img;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Handle New Frame : " + ex.Message);
                        }
                    });
                }

            }
            catch (Exception ex) 
            {
                MessageBox.Show("Handle New Video Frame : " + ex.Message);
            }
        }
        #endregion

        #region Image format conversion
        private BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            BitmapImage bitmapimage = new BitmapImage();
            try
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("BitmapToImageSource() exception: " + ex.Message);
            }
            return bitmapimage;
        }
        #endregion


    }
}
