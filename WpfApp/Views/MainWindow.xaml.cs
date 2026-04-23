using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp.Views;

namespace UareU.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        VideoStream _videoStream = new VideoStream();
        public MainWindow()
        {
            InitializeComponent();
            string imagesFolder = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Images"
            );
            // create if not exists
            Directory.CreateDirectory(imagesFolder);
            MainFrame.Navigate(new MainPage());
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //FetureOneService._videoStream = _videoStream;
            //_videoStream.StartStreamingVideo(liveStreamImage);
            // Get monitor where the app opens
            var screen = Screen.FromHandle(
                new System.Windows.Interop.WindowInteropHelper(this).Handle);

            var workArea = screen.WorkingArea;

            // 80% of screen
            this.Width = workArea.Width * 1.0;
            this.Height = workArea.Height * 1.0;

            // Center the window
            this.Left = workArea.Left + (workArea.Width - this.Width) / 2;
            this.Top = workArea.Top + (workArea.Height - this.Height) / 2;
        }

        private void NavigateTo(Page newPage)
        {
            if(MainFrame.Content is AutoCapturePage autoCapturePage )
            {
                _videoStream.StopStreamingVideo();
            }

            MainFrame.Navigate(newPage);
        }


        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) 
        {
            
            _videoStream.StopStreamingVideo();
            
        }

        private void AutoCapture(object sender, RoutedEventArgs e)
        {
            NavigateTo(new AutoCapturePage(_videoStream));
        }

        private void Matching(object sender, RoutedEventArgs e)
        {
            NavigateTo(new MatchingPage());
        }

        private void CameraMatching(object sender, RoutedEventArgs e)
        {
            NavigateTo(new MatchingPage());
        }

        private void Database(object sender, RoutedEventArgs e)
        {
            NavigateTo(new DatabasePage());
        }

        private void Enrollment(object sender, RoutedEventArgs e)
        {

        }

        private void Authentication(object sender, RoutedEventArgs e)
        {

        }

        private void Settings(object sender, RoutedEventArgs e)
        {

        }


    }
}
