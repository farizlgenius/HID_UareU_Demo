using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UareU;
using WpfApp.HFApi;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace UareU.Views
{
    /// <summary>
    /// Interaction logic for MatchingPage.xaml
    /// </summary>
    public partial class MatchingPage : Page
    {
        private BitmapImage img1;
        private BitmapImage img2;
        private CameraApi api;
        public MatchingPage(CameraApi api)
        {
            InitializeComponent();
            this.api = api;
            

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



        private void UploadImg1(object sender, RoutedEventArgs e)
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

                        Image1Preview.Source = bitmap;
                        img1 = bitmap;

                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Unsupport file type");
                    }

                }
            }
        }

        private void UploadImg2(object sender, RoutedEventArgs e)
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

                        Image2Preview.Source = bitmap;
                        img2 = bitmap;

                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Unsupport file type");
                    }

                }
            }
        }


        private async void Matching(object sender, RoutedEventArgs e)
        {
            if(img1 != null && img2 != null)
            {
                Loader.Show();
                var result = await api.MatchTwoImage(img1, img2);
                Loader.Hide();
                App.Current.Dispatcher.Invoke(() =>
                {
                    //ScoreText.Text = $"{Math.Round(result,2) * 100}%";
                    ScoreText.Text = $"{result * 100}%";
                });
            }
            else
            {
                System.Windows.MessageBox.Show("Please upload both images before matching.");
            }
           
        }
    }
}
