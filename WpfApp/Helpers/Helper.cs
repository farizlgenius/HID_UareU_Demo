using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static HFApiWrapper.HFApiWrapper;
using static HFApiWrapper.HFTypesManagedEquivalents;
using static HFApiWrapper.HFTypesManagedEquivalents.HFErrors;
using static HFApiWrapper.HFTypesManagedEquivalents.HFErrors.HFErrorCodes;
using static HFApiWrapper.HFTypesManagedEquivalents.HFPositioningFeedback;
using static HFApiWrapper.HFTypesManagedEquivalents.HFResultFlags;

namespace UareU.Helpers
{
    public sealed class Helper
    {
        public static int VERIFY(Int32 error)
        {
            if (error != (Int32)HFErrorCodes.HFERROR_OK)
            {
                Console.WriteLine(error);
                if (error != 3)
                {
                    MessageBox.Show("HFApi error, quitting.  Put break point in VERIFY() and check the stack: " + ErrorToString(error));
                    HFTerminate();
                    Environment.Exit(error);
                }
                HFTerminate();
                return error;
            }
            return 0;

        }

        public static void ASSURE(bool cond)
        {
            if (!cond)
            {
                MessageBox.Show("HFApi error, quitting.  Put break point in ASSURE() and check the stack.");
                HFTerminate();
                Environment.Exit(1);
            }
        }
        public static BitmapImage ByteToBitMapImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }


        public static Bitmap ByteToBitMap(byte[] imageData)
        {
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                return new Bitmap(ms);
            }
        }

        public static string PositionBitsToString(Int32 positioningBits)
        {
            if (positioningBits == (Int32)HFPOSITION_OK)
            {
                return "Position OK";
            }
            else if ((positioningBits & (Int32)HFPOSITION_TURN_RIGHT) == (Int32)HFPOSITION_TURN_RIGHT)
            {
                return "Turn Right";
            }
            else if ((positioningBits & (Int32)HFPOSITION_TURN_LEFT) == (Int32)HFPOSITION_TURN_LEFT)
            {
                return "Turn Left";
            }
            else if ((positioningBits & (Int32)HFPOSITION_LIFT_HEAD) == (Int32)HFPOSITION_LIFT_HEAD)
            {
                return "Lift Head";
            }
            else if ((positioningBits & (Int32)HFPOSITION_LOWER_HEAD) == (Int32)HFPOSITION_LOWER_HEAD)
            {
                return "Lower Head";
            }
            else if ((positioningBits & (Int32)HFPOSITION_TILT_RIGHT) == (Int32)HFPOSITION_TILT_RIGHT)
            {
                return "Tilt Head Right";
            }
            else if ((positioningBits & (Int32)HFPOSITION_TILT_LEFT) == (Int32)HFPOSITION_TILT_LEFT)
            {
                return "Tilt Head Left";
            }
            else if ((positioningBits & (Int32)HFPOSITION_GET_CLOSER) == (Int32)HFPOSITION_GET_CLOSER)
            {
                return "Move Closer";
            }
            else if ((positioningBits & (Int32)HFPOSITION_MOVE_AWAY) == (Int32)HFPOSITION_MOVE_AWAY)
            {
                return "Back Up";
            }
            else if ((positioningBits & (Int32)HFPOSITION_MOVE_LEFT) == (Int32)HFPOSITION_MOVE_LEFT)
            {
                return "Move Left";
            }
            else if ((positioningBits & (Int32)HFPOSITION_MOVE_RIGHT) == (Int32)HFPOSITION_MOVE_RIGHT)
            {
                return "Move Right";
            }
            else if ((positioningBits & (Int32)HFPOSITION_MOVE_UP) == (Int32)HFPOSITION_MOVE_UP)
            {
                return "Move Up";
            }
            else if ((positioningBits & (Int32)HFPOSITION_MOVE_DOWN) == (Int32)HFPOSITION_MOVE_DOWN)
            {
                return "Move Down";
            }else if ((positioningBits & (Int32)HFPOSITION_ENSURE_SINGLE_FACE_IN_CROPPED_IMAGE) == (Int32)HFPOSITION_ENSURE_SINGLE_FACE_IN_CROPPED_IMAGE)
            {
                return "Found more than one face";
            }

             return "";

        }

        public static byte[] BitmapImageToByteArray(BitmapImage bitmapImage)
        {
            byte[] data;
            var encoder = new PngBitmapEncoder(); // or JpegBitmapEncoder, etc.
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                data = stream.ToArray();
            }

            return data;
        }

        public static byte[] BitmapToByte(BitmapImage img)
        {
            byte[] data;
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();

            }
            return data;

        }

        public static void DrawFaceFeatures(UInt64 hfResult,System.Windows.Controls.Canvas _canvasDataOverlay,System.Windows.Controls.Image imageControl,string position,string quality)
        {
            // Points of interest
            HFPoint faceBottomRight = new HFPoint();
            HFPoint faceTopLeft = new HFPoint();
            HFImage image = new HFImage();

            Int32 hfError = 0;

            // Face bounding box
            hfError |= HFParseResultPoint((IntPtr)hfResult, (UInt64)HFRESULT_POINT_BOUNDING_BOX_UPPER_LEFT, ref faceTopLeft);
            hfError |= HFParseResultPoint((IntPtr)hfResult, (UInt64)HFRESULT_POINT_BOUNDING_BOX_BOTTOM_RIGHT, ref faceBottomRight);
            

            App.Current?.Dispatcher.Invoke(() =>
            {
                if (hfError == (UInt32)HFERROR_OK)
                {
                    DrawDataPointsOnGui(faceTopLeft, faceBottomRight, _canvasDataOverlay, imageControl, position,quality);

                }
                else
                {
                    // No face data , clear GUI
                    _canvasDataOverlay.Children.Clear();
                }
            });

        }

        public static BitmapImage ByteArrayToBitmapImage(byte[] imageBytes)
        {
            BitmapImage bitmap = new BitmapImage();

            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // load into memory (important)
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze(); // optional but recommended for cross-thread use
            }

            return bitmap;
        }

        public static void DrawDataPointsOnGui(
    HFPoint faceTopLeft,
    HFPoint faceBottomRight,
    Canvas canvas,
    System.Windows.Controls.Image imageControl,
            string position,string quality
            )
        {
            canvas.Children.Clear();

            // SAFE CAST (old C# compatible)
            BitmapSource bitmap = imageControl.Source as BitmapSource;
            if (bitmap == null)
                return;

            // scale from image pixels → screen pixels
            double scaleX = imageControl.ActualWidth / bitmap.PixelWidth;
            double scaleY = imageControl.ActualHeight / bitmap.PixelHeight;

            double x1 = faceTopLeft.x * scaleX;
            double y1 = faceTopLeft.y * scaleY;
            double x2 = faceBottomRight.x * scaleX;
            double y2 = faceBottomRight.y * scaleY;

            System.Windows.Media.SolidColorBrush brush = System.Windows.Media.Brushes.LimeGreen;
            int thickness = 4;

            canvas.Children.Add(new Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y1, Stroke = brush, StrokeThickness = thickness });
            canvas.Children.Add(new Line { X1 = x1, Y1 = y2, X2 = x2, Y2 = y2, Stroke = brush, StrokeThickness = thickness });
            canvas.Children.Add(new Line { X1 = x1, Y1 = y1, X2 = x1, Y2 = y2, Stroke = brush, StrokeThickness = thickness });
            canvas.Children.Add(new Line { X1 = x2, Y1 = y1, X2 = x2, Y2 = y2, Stroke = brush, StrokeThickness = thickness });

            // ⭐ TEXT LABEL ABOVE BOX
            TextBlock text = new TextBlock();
            text.Text = position;
            text.Foreground = System.Windows.Media.Brushes.LimeGreen;
            text.FontSize = 18;
            text.FontWeight = FontWeights.Bold;
            //text.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(160, 0, 0, 0));
            text.Padding = new Thickness(6, 2, 6, 2);

            // place slightly above top line
            Canvas.SetLeft(text, x1);
            Canvas.SetTop(text, y1 - 30);

            // TEXT for Quality
            TextBlock text2 = new TextBlock();
            text2.Text = $"Q: {quality}";
            text2.Foreground = System.Windows.Media.Brushes.LimeGreen;
            text2.FontSize = 18;
            text2.FontWeight = FontWeights.Bold;
            //text.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(160, 0, 0, 0));
            text2.Padding = new Thickness(6, 2, 6, 2);
            // place slightly above top line
            Canvas.SetLeft(text2, x1);
            Canvas.SetTop(text2, y1 - 50);


            canvas.Children.Add(text);
            canvas.Children.Add(text2);
        }






    }
}
