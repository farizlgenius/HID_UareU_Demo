using OpenCvSharp;

namespace MaskDetection.Core
{
    public class MaskResult
    {
        public bool HasMask { get; set; }
        public float Confidence { get; set; }
        public Rect Box { get; set; }

    }
}
