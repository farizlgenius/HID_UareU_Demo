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
using WpfApp.Features;
using static HFApiWrapper.HFTypesManagedEquivalents;

namespace WpfApp.Views
{
    /// <summary>
    /// Interaction logic for DatabasePage.xaml
    /// </summary>
    public partial class DatabasePage : Page
    {
        ObservableCollection<FaceRecord> records = new ObservableCollection<FaceRecord>();
        public Database db = new Database();
        public DatabasePage()
        {
            InitializeComponent();

            _ = LoadData();

        }

        private async Task LoadData()
        {
            FaceGrid.ItemsSource = await db.GetRecord();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {

        }

       
    }
}
