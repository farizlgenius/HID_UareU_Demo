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
using WpfApp.Features;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace UareU.Views
{
    /// <summary>
    /// Interaction logic for AutoCapturePage.xaml
    /// </summary>
    public partial class AutoCapturePage : Page
    {
        AutoCapture capture;
        public AutoCapturePage(VideoStream videoStream)
        {
            InitializeComponent();
            liveStreamImage.SizeChanged += (s, e) =>
            {
                canvasOverlay.Width = liveStreamImage.ActualWidth;
                canvasOverlay.Height = liveStreamImage.ActualHeight;
            };
            videoStream.StartStreamingVideo(liveStreamImage);
            
            capture = new AutoCapture(videoStream, detail, canvasOverlay,liveStreamImage,captureImage);
            capture.Capture();

        }

        private void Select_Path(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    capture.Path = dialog.SelectedPath;
                    Console.WriteLine(dialog.SelectedPath);
                    Environment.SpecialFolder root = dialog.RootFolder;

                    string file = System.IO.Path.Combine(dialog.SelectedPath, $"{DateTime.Now:yyyy-MM-dd-HHmmss}.png");

                    if (capture.image.data.data != null)
                    {

                        if (File.Exists(file))
                            File.Delete(file);


                        // Save to byte array
                        using (MemoryStream stream = new MemoryStream(capture.image.data.data))
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


    }
}
