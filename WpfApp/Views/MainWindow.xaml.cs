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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp.HFApi;
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

        public CameraApi api { get; set; }

        public MainWindow()
        {
            api = new CameraApi();
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


        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
           await api.OpenCameraContext();
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
            if (MainFrame.Content is AutoCapturePage autoCapturePage ||
                MainFrame.Content is MatchingPage matcPage ||
                MainFrame.Content is VerificationPage verify ||
                MainFrame.Content is DatabasePage database ||
                MainFrame.Content is EnrollPage enroll ||
                MainFrame.Content is IdentificationPage iden
                )
            {
                api.StopOperation();
                //_videoStream.StopStreamingVideo();
            }

            MainFrame.Navigate(newPage);
        }


        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) 
        {
            
            _videoStream.StopStreamingVideo();
            await api.CloseCameraContext();
            
        }

        private void AutoCapture(object sender, RoutedEventArgs e)
        {
            NavigateTo(new AutoCapturePage(_videoStream,api));
        }

        private void Matching(object sender, RoutedEventArgs e)
        {
            NavigateTo(new MatchingPage(api));
        }

        private void Verification(object sender, RoutedEventArgs e)
        {
            NavigateTo(new VerificationPage(_videoStream,api));
        }

        private void Database(object sender, RoutedEventArgs e)
        {
            NavigateTo(new DatabasePage(api));
        }

        private void Enrollment(object sender, RoutedEventArgs e)
        {
            NavigateTo(new EnrollPage(_videoStream,api));
        }

        private void Authentication(object sender, RoutedEventArgs e)
        {
            NavigateTo(new IdentificationPage(_videoStream,api));
        }




    }
}
