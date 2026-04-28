using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp.Models
{
    public sealed class VerifyImage
    {
        public int MatchSuccess { get; set; }
        public double MatchScore { get; set; }
        public byte[] Image { get; set; }
    }
}
