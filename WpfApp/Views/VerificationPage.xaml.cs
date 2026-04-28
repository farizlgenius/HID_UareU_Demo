using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UareU;
using UareU.Helpers;
using WpfApp.HFApi;
using WpfApp.Models;

namespace WpfApp.Views
{
    /// <summary>
    /// Interaction logic for VerificationPage.xaml
    /// </summary>
    public partial class VerificationPage : Page
    {
        CameraApi api;
        VideoStream videoStream;
        BitmapImage img;
        VerifyImage res;
        public VerificationPage(VideoStream videoStream,CameraApi api)
        {
            this.videoStream = videoStream;
            this.api = api;
            api.StopOperationFlag = false;
            InitializeComponent();
            liveStreamImage.SizeChanged += (s, e) =>
            {
                canvasOverlay.Width = liveStreamImage.ActualWidth;
                canvasOverlay.Height = liveStreamImage.ActualHeight;
            };
            this.videoStream.OnFpsUpdated += UpdateFpsUI;
            this.videoStream.StartStreamingVideo(liveStreamImage);

            //Authentication(canvasOverlay, liveStreamImage);
        }

        // 🔥 Runs AFTER Frame.Navigate()
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            api.StopOperationFlag = false;
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

        private async void Image_Path(object sender, RoutedEventArgs e)
        {
            // Open file dialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = true;
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                foreach (var filename in dlg.FileNames)
                {
                    string extension = System.IO.Path.GetExtension(filename).ToLower();
                    if (extension == ".jpeg" || extension == ".jpg" || extension == ".png")
                    {
                        BitmapImage bitmap = new BitmapImage();

                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(filename, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad; // Optional: forces full load
                        bitmap.EndInit();

                        ImagePreview.Source = bitmap;
                        
                        img = bitmap;
                        VerifyBtn.IsEnabled = true;



                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Unsupport file type");
                    }

                }
            }
        }

        private void ShowVerificationResult(bool isMatch, double score)
        {
            if (isMatch)
            {
                ResultIcon.Text = "✔";
                ResultIcon.Foreground = Brushes.Green;
                ResultIconBg.Background = new SolidColorBrush(Color.FromRgb(232, 248, 238));

                MatchSuccess.Text = "Face Matched";
                MatchSuccess.Foreground = Brushes.Green;
            }
            else
            {
                ResultIcon.Text = "✖";
                ResultIcon.Foreground = Brushes.Red;
                ResultIconBg.Background = new SolidColorBrush(Color.FromRgb(255, 235, 235));

                MatchSuccess.Text = "Face Not Match";
                MatchSuccess.Foreground = Brushes.Red;
            }

            MatchScore.Text = $"{score:F2}%";
        }

        private async void Click_Verify(object sender, RoutedEventArgs e)
        {
            res = await api.VerifyWithImage(canvasOverlay, liveStreamImage, img);
            App.Current.Dispatcher.Invoke(() =>
            {
                CapturePreview.Source = Helper.ByteArrayToBitmapImage(res.Image);
                MatchSuccess.Text = res.MatchSuccess == 1 ? "Matched" : "Not Match";
                MatchScore.Text = res.MatchScore.ToString();
                canvasOverlay.Children.Clear();
                ShowVerificationResult(res.MatchSuccess == 1, res.MatchScore);


            });
        }
    }
}
