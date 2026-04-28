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
using static HFApiWrapper.HFTypesManagedEquivalents;

namespace WpfApp.Views
{
    /// <summary>
    /// Interaction logic for IdentificationPage.xaml
    /// </summary>
    public partial class IdentificationPage : Page
    {
        private CameraApi api;
        private byte[] image = default;
        VideoStream videoStream;

        public IdentificationPage(VideoStream videoStream, CameraApi api)
        {
            InitializeComponent();
            this.api = api;
            this.videoStream = videoStream;
            api.StopOperationFlag = false;
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
            Identify(Detail, canvasOverlay, liveStreamImage, captureImage);
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

        public Task Identify(TextBlock detail, Canvas overlay, System.Windows.Controls.Image liveImage, System.Windows.Controls.Image captureImage)
        {
            return Task.Run(async () =>
            {
                HFMatchRecord record = new HFMatchRecord();

                App.Current.Dispatcher.Invoke(() =>
                {
                    MatchPanel.Background = new SolidColorBrush(Color.FromRgb(255, 248, 225));
                    MatchPanel.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 224, 130));
                    MatchIconBg.Background = new SolidColorBrush(Color.FromRgb(255, 167, 38));

                    MatchIcon.Text = "🔍";
                    MatchTitle.Text = "Scanning Face";
                    MatchScoreText.Text = "Please look at the camera...";

                    PreviewPersonName.Text = "Scanning face...";
                    PreviewScore.Text = "...";

                    PreviewBadge.Background = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // orange
                    PreviewBadgeIcon.Text = "🔎";


                });


                image = await api.CaptureImageFromLiveStream(overlay, liveImage, captureImage, detail);

                if(image != null)
                {
                    record = await api.IdentifyImage(image);
                }
                else
                {
                    record = null;
                }
                


                App.Current.Dispatcher.Invoke(() =>
                {
                    captureImage.Source = Helper.ByteToBitMapImage(image);
                    detail.Text = "Finished";
                    overlay.Children.Clear();

                    if (record is null)
                    {
                        MatchPanel.Background = new SolidColorBrush(Color.FromRgb(253, 236, 234));
                        MatchPanel.BorderBrush = new SolidColorBrush(Color.FromRgb(244, 143, 177));
                        MatchIconBg.Background = new SolidColorBrush(Color.FromRgb(229, 57, 53));

                        MatchIcon.Text = "⚠";
                        MatchTitle.Text = "Face Already Exists";
                        MatchScoreText.Text = "Please already enroll in camera.";

                        PreviewPersonName.Text = "Unknown person";
                        PreviewScore.Text = "-- %";

                        PreviewBadge.Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)); // red
                        PreviewBadgeIcon.Text = "✖";
                    }
                    else
                    {

                        MatchPanel.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233));
                        MatchPanel.BorderBrush = new SolidColorBrush(Color.FromRgb(165, 214, 167));
                        MatchIconBg.Background = new SolidColorBrush(Color.FromRgb(67, 160, 71));

                        MatchIcon.Text = "✔";
                        MatchTitle.Text = "Detected!";
                        MatchScoreText.Text = "found the profile.";

                        PreviewPersonName.Text = record.header.recordID;
                        PreviewScore.Text = $"{record.matchScore * 100.00} %";

                        PreviewBadge.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // green
                        PreviewBadgeIcon.Text = "✔";
                    }



                });


            });
        }
    }
}
