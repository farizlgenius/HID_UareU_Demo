using Accord.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UareU.Helpers;
using WpfApp.HFApi;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace UareU.Views
{
    /// <summary>
    /// Interaction logic for AutoCapturePage.xaml
    /// </summary>
    public partial class AutoCapturePage : Page
    {
  
        CameraApi api;
        private byte[] image { get; set; }
        private double _minQuality = 0.5;
        private double _maxSpoofing = 0.5;

        private VideoStream videoStream;
    
        public AutoCapturePage(VideoStream videoStream,CameraApi api)
        {
            this.api = api; 
            this.videoStream = videoStream;
            InitializeComponent();
            liveStreamImage.SizeChanged += (s, e) =>
            {
                canvasOverlay.Width = liveStreamImage.ActualWidth;
                canvasOverlay.Height = liveStreamImage.ActualHeight;
            };
            this.videoStream.OnFpsUpdated += UpdateFpsUI;
            this.videoStream.StartStreamingVideo(liveStreamImage);
           

        }

        // 🔥 Runs AFTER Frame.Navigate()
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            api.StopOperationFlag = false;
            Capture(this.videoStream, detail, canvasOverlay, liveStreamImage, captureImage, _minQuality, _maxSpoofing);
        }

        // 🔴 Runs when navigating away
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            api.StopOperation();
        }



        private void UpdateFpsUI(double fps)
        {
            Dispatcher.Invoke(() =>
            {
                FpsText.Text = $"{fps:F1} FPS";

                // Optional color indicator 🔥
                if (fps >= 25)
                    FpsText.Foreground = System.Windows.Media.Brushes.LimeGreen;
                else if (fps >= 15)
                    FpsText.Foreground = System.Windows.Media.Brushes.Orange;
                else
                    FpsText.Foreground = System.Windows.Media.Brushes.Red;
            });
        }

        private void Select_Path(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    Console.WriteLine(dialog.SelectedPath);
                    Environment.SpecialFolder root = dialog.RootFolder;

                    string file = System.IO.Path.Combine(dialog.SelectedPath, $"{DateTime.Now:yyyy-MM-dd-HHmmss}.png");

                    if (image != null)
                    {

                        if (File.Exists(file))
                            File.Delete(file);


                        // Save to byte array
                        using (MemoryStream stream = new MemoryStream(image))
                        {
                            new Bitmap(stream).Save(file, ImageFormat.Png);
                        }


                    }
                    else
                    {
                        MessageBox.Show("No frame to capture.");
                    }
                }

            }

        }

        public Task Capture(VideoStream stream, TextBlock detail, Canvas overlay, System.Windows.Controls.Image liveImage, System.Windows.Controls.Image captureImage,double Quality,double Spoofing)
        {
            return Task.Run(async () =>
            {

                App.Current.Dispatcher.Invoke(() =>
                {
                    detail.Text = "Capturing...";

                });


                image = await api.CaptureImage(overlay, liveImage, captureImage, detail,Quality,Spoofing);

                // Show on page
                App.Current.Dispatcher.Invoke(() =>
                {
                    captureImage.Source = Helper.ByteToBitMapImage(image);
                    detail.Text = "Finished";
                    overlay.Children.Clear();

                });



            });

        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            Capture(videoStream, detail, canvasOverlay, liveStreamImage, captureImage,_minQuality,_maxSpoofing);
        }

        private void Apply_Setting(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(MinQualitySlider.Value / 100.00);
            _minQuality = MinQualitySlider.Value / 100.00;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            api.StopOperation();

            // Show on page
            App.Current.Dispatcher.Invoke(() =>
            {
                captureImage.Source = Helper.ByteToBitMapImage(image);
                detail.Text = "Stoped";
                canvasOverlay.Children.Clear();

            });
        }
    }
}
