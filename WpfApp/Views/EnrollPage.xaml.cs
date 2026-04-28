using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace WpfApp.Views
{
    /// <summary>
    /// Interaction logic for EnrollPage.xaml
    /// </summary>
    public partial class EnrollPage : Page
    {
        private CameraApi api;
        private byte[] image = default;
        VideoStream videoStream;
        public EnrollPage(VideoStream videoStream,CameraApi api)
        {
            InitializeComponent();
            this.api = api;
            api.StopOperationFlag = false;
            this.videoStream = videoStream;
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
            Enroll(Detail, canvasOverlay, liveStreamImage, captureImage);
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

        public Task Enroll(TextBlock detail, Canvas overlay, System.Windows.Controls.Image liveImage, System.Windows.Controls.Image captureImage)
        {
            return Task.Run(async () =>
            {
                bool isExists = false;

                App.Current.Dispatcher.Invoke(() =>
                {
                    StatusPanel.Background = new SolidColorBrush(Color.FromRgb(255, 248, 225));
                    StatusPanel.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 224, 130));
                    StatusIconBg.Background = new SolidColorBrush(Color.FromRgb(255, 167, 38));

                    StatusIcon.Text = "🔍";
                    StatusTitle.Text = "Scanning Face";
                    StatusMessage.Text = "Please look at the camera...";

                });


                image = await api.CaptureImageFromLiveStream(overlay, liveImage, captureImage, detail);

                if (image != null) 
                {
                    isExists = await api.CheckExistsFace(image);
                }

                


                App.Current.Dispatcher.Invoke(() =>
                {
                    

                    if (isExists)
                    {
                        StatusPanel.Background = new SolidColorBrush(Color.FromRgb(253, 236, 234));
                        StatusPanel.BorderBrush = new SolidColorBrush(Color.FromRgb(244, 143, 177));
                        StatusIconBg.Background = new SolidColorBrush(Color.FromRgb(229, 57, 53));

                        StatusIcon.Text = "⚠";
                        StatusTitle.Text = "Face Already Exists";
                        StatusMessage.Text = "Please already enroll in camera.";

                        detail.Text = "Face Already Exists";
                        overlay.Children.Clear();
                    }else if(image == null)
                    {

                    }
                    else
                    {
                        captureImage.Source = Helper.ByteToBitMapImage(image);
                        detail.Text = "Finished";
                        overlay.Children.Clear();

                        BtnSave.IsEnabled = true;
                        StatusPanel.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233));
                        StatusPanel.BorderBrush = new SolidColorBrush(Color.FromRgb(165, 214, 167));
                        StatusIconBg.Background = new SolidColorBrush(Color.FromRgb(67, 160, 71));

                        StatusIcon.Text = "✔";
                        StatusTitle.Text = "Face Captured!";
                        StatusMessage.Text = "You can now save the profile.";
                    }

                    

                });


            });
        }

        private void Click_Recap(object sender, RoutedEventArgs e)
        {
            Enroll( Detail, canvasOverlay, liveStreamImage, captureImage);
        }

        private async void Click_Save(object sender, RoutedEventArgs e)
        {
            await api.AddRecord(Helper.ByteToBitMapImage(image));
            App.Current.Dispatcher.Invoke(() =>
            {
                StatusPanel.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233));
                StatusPanel.BorderBrush = new SolidColorBrush(Color.FromRgb(165, 214, 167));
                StatusIconBg.Background = new SolidColorBrush(Color.FromRgb(67, 160, 71));

                StatusIcon.Text = "✔";
                StatusTitle.Text = "Save Success!";
                StatusMessage.Text = "Save record to camera success.";


            });


        }
    }
}
