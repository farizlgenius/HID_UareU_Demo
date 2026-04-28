using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static HFApiWrapper.HFTypesManagedEquivalents;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace WpfApp.Views
{
    /// <summary>
    /// Interaction logic for DatabasePage.xaml
    /// </summary>
    public partial class DatabasePage : Page
    {
        ObservableCollection<FaceRecord> records = new ObservableCollection<FaceRecord>();
        private CameraApi api;
        public string SelectedRecord { get; set; }
        public DatabasePage(CameraApi api)
        {
            this.api = api;
            api.StopOperationFlag = false;
            InitializeComponent();

            

        }

        // 🔥 Runs AFTER Frame.Navigate()
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            api.StopOperationFlag = false;
            _ = LoadData();
        }

        // 🔴 Runs when navigating away
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            api.StopOperation();
        }

        private async Task LoadData()
        {
            Loader.Show();
            FaceGrid.ItemsSource = await GetRecord();
            Loader.Hide();
        }


        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var row = btn?.CommandParameter as FaceRecord; // your row class

            if (row == null)
                return;

            DeleteRecord(row.RecordId);

            _ = LoadData();
        }

        private void FaceGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FaceGrid.SelectedItem == null)
                return;

            // Get the selected row object
            var selectedRow = FaceGrid.SelectedItem as FaceRecord; // <-- your model class name

            if (selectedRow == null)
                return;

            SelectedRecord = selectedRow.RecordId;


        }

        public Task DeleteRecord(string id)
        {
            return Task.Run(async () =>
            {

                await api.DeleteRecord(id);
            });
        }

        public Task<ObservableCollection<FaceRecord>> GetRecord()
        {
            return Task.Run(async () =>
            {
                HFStringArray Ids = new HFStringArray();
                var records = new ObservableCollection<FaceRecord>();


                Ids = await api.GetRecordList();


                // 🔵 Switch to UI thread here
                return await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    var Records = new ObservableCollection<FaceRecord>();

                    foreach (var r in Ids.strings)
                    {
                        string path = System.IO.Path.Combine(
                       AppDomain.CurrentDomain.BaseDirectory,
                       "Databases",
                       $"{r}.png"
                            );

                        try
                        {
                            BitmapImage img = new BitmapImage(new Uri(path));
                            Records.Add(new FaceRecord
                            {
                                RecordId = r,
                                FaceImage = img // now safe
                            });
                        }
                        catch 
                        {
                            Records.Add(new FaceRecord
                            {
                                RecordId = r,
                                FaceImage = default // now safe
                            });
                        }

                        

                       
                    }

                    return Records;
                });
            });
        }


    }
}
