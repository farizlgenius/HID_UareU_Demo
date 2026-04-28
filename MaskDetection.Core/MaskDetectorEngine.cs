using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaskDetection.Core
{
    public class MaskDetectorEngine : IDisposable
    {
        private readonly InferenceSession _session;
       

        public MaskDetectorEngine(string modelPath)
        {
            _session = new InferenceSession(modelPath);
        }

        public string Detect(Mat frame)
        {
            // crop center square
            int size = Math.Min(frame.Width, frame.Height);

            var roi = new Rect(
                (frame.Width - size) / 2,
                (frame.Height - size) / 2,
                size, size);

            var cropped = new Mat(frame, roi);
            var resized = cropped.Resize(new Size(360, 360));

            var input = new DenseTensor<float>(new[] { 1, 3, 360, 360 });

            // BGR → RGB
            Cv2.CvtColor(resized, resized, ColorConversionCodes.BGR2RGB);

            for (int y = 0; y < 360; y++)
                for (int x = 0; x < 360; x++)
                {
                    var pixel = resized.At<Vec3b>(y, x);

                    float r = pixel.Item0 / 255f;
                    float g = pixel.Item1 / 255f;
                    float b = pixel.Item2 / 255f;

                    input[0, 0, y, x] = r;
                    input[0, 1, y, x] = g;
                    input[0, 2, y, x] = b;
                }

            string inputName = _session.InputMetadata.Keys.First();

            var inputs = new List<NamedOnnxValue>
    {
        NamedOnnxValue.CreateFromTensor(inputName, input)
    };

            using var results = _session.Run(inputs);
            var output = results.First().AsEnumerable<float>().ToArray();

            // 🔥 FIX OUTPUT ORDER (important)
            float noMaskProb = output[0];
            float maskProb = output[1];

            if (maskProb > noMaskProb)
                return $"Mask 😷 {maskProb:P1}";
            else
                return $"No Mask 🚫 {noMaskProb:P1}";
        }

        public void Dispose()
        {
            _session.Dispose();
        }

    
    }


}
