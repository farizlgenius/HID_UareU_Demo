using Accord;
using Accord.Imaging.Filters;
using Accord.Video;
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Remoting.Contexts;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static HFApiWrapper.HFApiWrapper;
using static HFApiWrapper.HFTypesManagedEquivalents;
using static HFApiWrapper.HFTypesManagedEquivalents.HFAlgorithmType;
using static HFApiWrapper.HFTypesManagedEquivalents.HFErrors;
using static HFApiWrapper.HFTypesManagedEquivalents.HFErrors.HFErrorCodes;
using static HFApiWrapper.HFTypesManagedEquivalents.HFImageEncoding;
using static HFApiWrapper.HFTypesManagedEquivalents.HFParam;
using static HFApiWrapper.HFTypesManagedEquivalents.HFPositioningFeedback;
using static HFApiWrapper.HFTypesManagedEquivalents.HFResultFlags;
using static HFApiWrapper.HFTypesManagedEquivalents.HFStatusCodes;
using static HFApiWrapper.HFTypesManagedEquivalents.HFStatusCodes.HFStatus;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using UareU.Views;
using System.Threading.Tasks;
using UareU.Helpers;

namespace UareU
{
    public class HFApi
    {
        private HFData _captureTemplate;
        private HFData _uploadTemplate;
        public static string _path;
        private int _feature;
        public bool _HFApiStopLooping = false;
        public static System.Windows.Controls.Canvas _canvasDataOverlay;
        public static System.Windows.Controls.TextBlock _qualityLabel;
        public static System.Windows.Controls.TextBlock _spoofingLabel;
        public static System.Windows.Controls.TextBlock _positionLabel;
        public static System.Threading.Thread _hfApiThread;
        public static System.Windows.Controls.TextBlock _instruction;
        public static System.Windows.Controls.Image _liveStreamImage;
        public static VideoStream _videoStream;
        public static byte[] _uploadImage;
        public static HFImage _img = new HFImage();
        HFImageEncoding enc = HFIMAGE_ENCODING_PNG;
        UInt32 hfOperation = HFOPERATION_NONE;
        UInt32 hfContext = HFCONTEXT_NONE;
        Int32 hfStatus = (Int32)HFSTATUS_BUSY;
        Int32 hfError;
        System.IntPtr hfResult;
        Int32 error = (Int32)HFERROR_OK;
        HFData templ = new HFData();
        HFData templ2 = new HFData();
        HFImage image = new HFImage();
        HFStringArray cameraNames = new HFStringArray();
        HFStringArray cameraIDs = new HFStringArray();
        BitmapImage feature2Img1;
        BitmapImage feature2Img2;
        double score = 0;


        public HFApi(int feature) 
        {
            _feature = feature;
        }







        #region Main Function

        public  Task<double> MatchingImage(BitmapImage img1, BitmapImage img2)
        {
            return Task.Run(() =>
            {
                _HFApiStopLooping = false;
                Console.WriteLine("Start HF API.....");

                HFTerminate();
                Helper.VERIFY(HFInit());
                Helper.VERIFY(HFEnumerateCameras(ref cameraNames, ref cameraIDs));

                if (cameraIDs.strings.Count == 0)
                {
                    MessageBox.Show("No UareU Camera Found");
                    return 0d;
                }

                Helper.VERIFY(HFOpenContext(cameraIDs.strings[0],
                       (Int32)HFALGORITHM_TYPE_ON_DEVICE,
                       ref hfContext,
                       ref hfOperation));

                hfStatus = (Int32)HFSTATUS_BUSY;
                while (hfStatus == (Int32)HFSTATUS_BUSY)
                {
                    Helper.VERIFY(HFGetIntermediateResult(hfOperation,
                           (UInt32)HFRESULTVALID_BASIC,
                           HFSEQUENCE_NUMBER_NONE,
                           ref hfResult));

                    Helper.VERIFY(HFParseResultInt((IntPtr)hfResult,
                           (UInt32)HFRESULT_INT_OPERATION_STATUS,
                           ref hfStatus));

                    HFFree((IntPtr)hfResult);
                }

                Helper.ASSURE(hfStatus == (Int32)HFSTATUS_READY);
                Helper.VERIFY(HFCloseOperation(hfOperation));

                HFData template1 = ConvertImageToTemplate(Helper.BitmapImageToByteArray(img1), 0);
                HFData template2 = ConvertImageToTemplate(Helper.BitmapImageToByteArray(img2), 0);

                if (template1 != null && template2 != null)
                    return MatchingTemplate(template1, template2);

                return 0d;
            });

        }


       

        public int WaitForOperation(uint operation)
        {
            hfStatus = (Int32)HFSTATUS_BUSY;
            while (hfStatus == (Int32)HFSTATUS_BUSY)
            {
                Helper.VERIFY(HFGetIntermediateResult(operation, (UInt32)HFRESULTVALID_BASIC, HFSEQUENCE_NUMBER_NONE, ref hfResult));
                Helper.VERIFY(HFParseResultInt((IntPtr)hfResult, (UInt32)HFRESULT_INT_OPERATION_STATUS, ref hfStatus));
            }
            Console.WriteLine(">>>>>>>>>>>>>>>>>" + hfStatus);
            return (int)HFERROR_OK;
        }

        public void AnalyzeStreamFeatureTwo()
        {
            _HFApiStopLooping = false;
            //System.Windows.Controls.Canvas target;
            Console.WriteLine("Start HF API.....");
            // Now video is streaming to liveStreamImage on our GUI, lets collect the facial data from HFApi and overlay it on the video

            // Termiante first because we don't know camera status 
            HFTerminate();
            // Init HF SDK
            Helper.VERIFY(HFInit());


            Helper.VERIFY(HFEnumerateCameras(ref cameraNames, ref cameraIDs));

            // Open Camera Context
            if (cameraIDs.strings.Count != 0)
            {
                Helper.VERIFY(HFOpenContext(cameraIDs.strings[0], (Int32)HFALGORITHM_TYPE_ON_DEVICE, ref hfContext, ref hfOperation));
                Console.WriteLine("Open camera context...");

                hfStatus = (Int32)HFSTATUS_BUSY;
                while (hfStatus == (Int32)HFSTATUS_BUSY)
                {
                    Helper.VERIFY(HFGetIntermediateResult(hfOperation, (UInt32)HFRESULTVALID_BASIC, HFSEQUENCE_NUMBER_NONE, ref hfResult));
                    Helper.VERIFY(HFParseResultInt((IntPtr)hfResult, (UInt32)HFRESULT_INT_OPERATION_STATUS, ref hfStatus));
                    HFFree((IntPtr)hfResult);
                }
                Helper.ASSURE(hfStatus == (Int32)HFSTATUS_READY);
                Helper.VERIFY(HFCloseOperation(hfOperation));

                // Start Image Process to get location information about user face and capture image after location is find
                //StartImageProcessFeatureTwo();


                App.Current?.Dispatcher.Invoke(() =>
                {
                    _canvasDataOverlay.Children.Clear();
                    _positionLabel.Text = "";
                    _spoofingLabel.Text = "";
                    _qualityLabel.Text = "";
                    _videoStream.StopStreamingVideo();
                });
            }
            else
            {
                MessageBox.Show("No UareU Camera Found");
            }


            _hfApiThread.Abort();


        }

        //public void AnalyzeStreamFeatureOne()
        //{
        //    _HFApiStopLooping = false;
        //    //System.Windows.Controls.Canvas target;
        //    Console.WriteLine("Start HF API.....");
        //    // Now video is streaming to liveStreamImage on our GUI, lets collect the facial data from HFApi and overlay it on the video

        //    // Termiante first because we don't know camera status 
        //    HFTerminate();
        //    // Init HF SDK
        //    VERIFY(HFInit());


        //    VERIFY(HFEnumerateCameras(ref cameraNames, ref cameraIDs));

        //    // Open Camera Context
        //    if (cameraIDs.strings.Count != 0)
        //    {
        //        VERIFY(HFOpenContext(cameraIDs.strings[0], (Int32)HFALGORITHM_TYPE_ON_DEVICE, ref hfContext, ref hfOperation));
        //        Console.WriteLine("Open camera context...");

        //        hfStatus = (Int32)HFSTATUS_BUSY;
        //        while (hfStatus == (Int32)HFSTATUS_BUSY)
        //        {
        //            VERIFY(HFGetIntermediateResult(hfOperation, (UInt32)HFRESULTVALID_BASIC, HFSEQUENCE_NUMBER_NONE, ref hfResult));
        //            VERIFY(HFParseResultInt((IntPtr)hfResult, (UInt32)HFRESULT_INT_OPERATION_STATUS, ref hfStatus));
        //            HFFree((IntPtr)hfResult);
        //        }
        //        ASSURE(hfStatus == (Int32)HFSTATUS_READY);
        //        VERIFY(HFCloseOperation(hfOperation));

        //        // Start Image Process to get location information about user face and capture image after location is find
        //        StartImageProcessFeatureOne();


        //        App.Current?.Dispatcher.Invoke(() =>
        //        {
        //            _canvasDataOverlay.Children.Clear();
        //            _positionLabel.Text = "";
        //            _spoofingLabel.Text = "";
        //            _qualityLabel.Text = "";
        //            _videoStream.StopStreamingVideo();
        //        });
        //    }
        //    else
        //    {
        //        MessageBox.Show("No UareU Camera Found");
        //    }


        //    _hfApiThread.Abort();


        //}


        #endregion

        #region Matching Image

        private double MatchingTemplate(HFData Template1,HFData Template2)
        {
            double matchScore = 0;
            // Matching Process
            Helper.VERIFY(HFAsyncMatchWithTemplate(hfContext, 0.5, Template1, Template2, ref hfOperation));
            hfStatus = (Int32)HFSTATUS_BUSY;
            while (hfStatus == (Int32)HFSTATUS_BUSY)
            {
                Helper.VERIFY(HFGetIntermediateResult(hfOperation, (UInt64)HFRESULTVALID_ALL, HFSEQUENCE_NUMBER_NONE, ref hfResult));
                Helper.VERIFY(HFParseResultInt(hfResult, (UInt32)HFRESULT_INT_OPERATION_STATUS, ref hfStatus));
                HFFree((IntPtr)hfResult);

            }
            Helper.VERIFY(HFGetFinalResult(hfOperation, (UInt64)HFRESULTVALID_ALL, ref hfResult));
            Helper.VERIFY(HFParseResultDouble(hfResult, (UInt64)HFRESULT_DOUBLE_MATCH_SCORE, ref matchScore));

            return matchScore;

        }

        #endregion

        #region Process Image Convert Image to Templte

        public HFData ConvertImageToTemplate(byte[] ImageData,short Type)
        {
            HFData template = new HFData();
            HFImage Image = new HFImage();
            Image.data.data = ImageData;
            Image.data.size = (uint)ImageData.Length;
            switch (Type)
            {
                case 0:
                    Image.imageEncoding = HFImageEncoding.HFIMAGE_ENCODING_PNG;
                    break;
                case 1:
                    Image.imageEncoding = HFImageEncoding.HFIMAGE_ENCODING_JPEG;
                    break;
                case 3:
                    Image.imageEncoding = HFImageEncoding.HFIMAGE_ENCODING_MAX;
                    break;
                default:
                    break;
            }

            // Process Image
            hfStatus = (Int32)HFSTATUS_BUSY;
            Helper.VERIFY(HFAsyncProcessImage(hfContext, Image, 0.1, 0.1, (Int64)HFRESULT_DATA_TEMPLATE, ref hfOperation));
            while (hfStatus == (Int32)HFSTATUS_BUSY)
            {
                Helper.VERIFY(HFGetIntermediateResult(hfOperation, (UInt64)HFRESULTVALID_ALL, HFSEQUENCE_NUMBER_NONE, ref hfResult));
                Helper.VERIFY(HFParseResultInt(hfResult, (UInt32)HFRESULT_INT_OPERATION_STATUS, ref hfStatus));
                HFFree((IntPtr)hfResult);

            }
            Console.WriteLine(hfResult);
            Helper.VERIFY(HFGetFinalResult(hfOperation, (UInt64)HFRESULTVALID_ALL, ref hfResult));
            error = Helper.VERIFY(HFParseResultData(hfResult, (UInt64)HFRESULT_DATA_TEMPLATE, ref template));
            if (error == 3) 
            {
                MessageBox.Show("There're no person in image");
                return null;
            }
            
            return template;

        }

        #endregion



        //private void StartImageProcessFeatureTwo()
        //{
        //    Console.WriteLine("Start Image Processing...");
        //    // Parameter
        //    HFPoint[] faceCropPosition = new HFPoint[2];
        //    faceCropPosition[0] = new HFPoint();
        //    faceCropPosition[1] = new HFPoint();
        //    Int32 hfErrorQuality = 0;
        //    Int32 hfErrorSpoof = 0;
        //    Int32 hfErrorPosition = 0;
        //    double quality = 0;
        //    double spoof = 0;
        //    Int32 positioningBits = 0;
        //    Int32 hfErrorBottomRight = 0;
        //    Int32 hfErrorUpperLeft = 0;

        //    // Loop for Get Face Data 
        //    Helper.VERIFY(HFSetParamInt(hfContext, (Int32)HFPARAM_CONTEXT_INT_CAPTURE_IMAGE_ENCODING, (Int32)HFIMAGE_ENCODING_PNG));
        //    Helper.VERIFY(HFAsyncStartCaptureAndProcessImage(hfContext, -1, 0, 0, (UInt64)HFRESULTVALID_ALL_NO_IMAGE, (UInt64)HFRESULTVALID_ALL, ref hfOperation));
        //    hfStatus = (Int32)HFSTATUS_BUSY;
        //    while ((_HFApiStopLooping == false) && (hfStatus == (Int32)HFSTATUS_BUSY))
        //    {

        //        hfResult = (System.IntPtr)0;

        //        // Get the intermediate result 
        //        Helper.VERIFY(HFGetIntermediateResult(hfOperation, (UInt64)HFRESULTVALID_ALL_NO_IMAGE, HFSEQUENCE_NUMBER_NONE, ref hfResult));
        //        DrawFaceFeatures((ulong)hfResult);

        //        hfErrorQuality = HFParseResultDouble(hfResult, (UInt64)HFRESULT_DOUBLE_QUALITY, ref quality);
        //        hfErrorSpoof = HFParseResultDouble(hfResult, (UInt64)HFRESULT_DOUBLE_SPOOF_PROBABILITY, ref spoof);
        //        hfErrorPosition = HFParseResultInt(hfResult, (Int64)HFRESULT_INT_POSITIONING_FEEDBACK, ref positioningBits);


        //        Helper.VERIFY(HFParseResultInt(hfResult, (Int32)HFRESULT_INT_OPERATION_STATUS, ref hfStatus));

        //        App.Current?.Dispatcher.Invoke(() =>
        //        {
        //            _qualityLabel.Text = (hfErrorQuality == (Int32)HFERROR_OK) ? $"{Math.Round(quality * 100)} %" : ErrorToString(hfErrorQuality);
        //            _positionLabel.Text = (hfErrorPosition == (Int32)HFERROR_OK) ? PositionBitsToString(positioningBits) : ErrorToString(hfErrorPosition);
        //            _spoofingLabel.Text = (hfErrorSpoof == (Int32)HFERROR_OK) ? $"{Math.Round(spoof * 100)} %" : ErrorToString(hfErrorSpoof);
        //        });
        //        Console.WriteLine("Quality : " + quality);
        //        Console.WriteLine("Spoofing : " + spoof);
        //        Console.WriteLine("Position : " + positioningBits);

        //        if (positioningBits == (Int32)HFPOSITION_OK &&
        //                (hfErrorSpoof == (Int32)HFERROR_OK) && (spoof > 0.5) &&
        //                ((hfErrorQuality == (Int32)HFERROR_OK) && (quality > 0.70)))
        //        {
                    
        //            byte[] _captureImageByteArray = _videoStream.CaptureImage();
        //            HFImage _image = new HFImage();
        //            _image.data.data = _captureImageByteArray;
        //            _image.data.size = (uint)_captureImageByteArray.Length;
                    
        //            // Start
        //            hfStatus = (Int32)HFSTATUS_BUSY;
        //            Helper.VERIFY(HFAsyncProcessImage(hfContext, _image, 0.1, 0.1, (Int64)HFRESULTVALID_ALL, ref hfOperation));
        //            while (hfStatus == (Int32)HFSTATUS_BUSY)
        //            {
        //                Helper.VERIFY(HFGetIntermediateResult(hfOperation, (UInt64)HFRESULTVALID_ALL, HFSEQUENCE_NUMBER_NONE, ref hfResult));
        //                Helper.VERIFY(HFParseResultInt(hfResult, (UInt32)HFRESULT_INT_OPERATION_STATUS, ref hfStatus));
        //            }
        //            Console.WriteLine(hfResult);
        //            Helper.VERIFY(HFGetFinalResult(hfOperation, (UInt64)HFRESULTVALID_ALL, ref hfResult));
        //            hfErrorBottomRight = Helper.VERIFY(HFParseResultPoint(hfResult, (UInt64)HFRESULT_POINT_BOUNDING_BOX_BOTTOM_RIGHT, ref faceCropPosition[1]));
        //            hfErrorUpperLeft = Helper.VERIFY(HFParseResultPoint(hfResult, (UInt64)HFRESULT_POINT_BOUNDING_BOX_UPPER_LEFT, ref faceCropPosition[0]));
        //            // End


        //            _videoStream.CropImage(faceCropPosition, _path, _captureImageByteArray);
                    
        //            if (_captureImageByteArray != null)
        //            {
        //                _uploadTemplate = ConvertImageToTemplate(_uploadImage, 0);
        //                _captureTemplate = ConvertImageToTemplate(_captureImageByteArray, 1);
        //                score = MatchingTemplate(_uploadTemplate, _captureTemplate);

        //                feature2Img1 = Helper.ByteToBitMapImage(_uploadImage);
        //                feature2Img2 = Helper.ByteToBitMapImage(_captureImageByteArray);

        //                App.Current?.Dispatcher.Invoke(() =>
        //                {
        //                    _liveStreamImage.Source = feature2Img2;
        //                 });


        //            }

        //            _HFApiStopLooping = true;

        //        }


        //        HFFree((IntPtr)hfResult);

        //    }
        //    HFFree((IntPtr)hfResult);

        //}
      
    

     

        public BitmapImage ConvertToBitmapImage(HFImage hfImage)
        {
            if (hfImage?.data?.data == null || hfImage.data.data.Length == 0)
                return null;

            BitmapImage bitmap = new BitmapImage();

            using (MemoryStream ms = new MemoryStream(hfImage.data.data, 0, (int)hfImage.data.size))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();
            }

            return bitmap;
        }

    }
}
