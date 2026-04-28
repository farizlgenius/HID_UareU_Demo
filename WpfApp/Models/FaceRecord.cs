using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WpfApp.Models
{
    public sealed class FaceRecord
    {
        public string RecordId { get; set; }
        public BitmapImage FaceImage { get; set; }
    }
}
