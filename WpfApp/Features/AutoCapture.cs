using Accord.Imaging.Filters;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UareU;
using UareU.Helpers;
using static HFApiWrapper.HFApiWrapper;
using static HFApiWrapper.HFTypesManagedEquivalents;
using static HFApiWrapper.HFTypesManagedEquivalents.HFAlgorithmType;
using static HFApiWrapper.HFTypesManagedEquivalents.HFCropMode;
using static HFApiWrapper.HFTypesManagedEquivalents.HFErrors;
using static HFApiWrapper.HFTypesManagedEquivalents.HFErrors.HFErrorCodes;
using static HFApiWrapper.HFTypesManagedEquivalents.HFImageEncoding;
using static HFApiWrapper.HFTypesManagedEquivalents.HFParam;
using static HFApiWrapper.HFTypesManagedEquivalents.HFPositioningFeedback;
using static HFApiWrapper.HFTypesManagedEquivalents.HFResultFlags;
using static HFApiWrapper.HFTypesManagedEquivalents.HFStatusCodes.HFStatus;
using WpfApp.HFApi;

namespace WpfApp.Features
{
    
    public class AutoCapture
    {
        CameraApi api;
        HFStringArray cameraNames = new HFStringArray();
        HFStringArray cameraIDs = new HFStringArray();
        UInt32 hfOperation = HFOPERATION_NONE;
        UInt32 hfContext = HFCONTEXT_NONE;
        Int32 hfStatus = (Int32)HFSTATUS_BUSY;
        Int32 hfError;
        Int32 error = (Int32)HFERROR_OK;
        System.IntPtr hfResult;
        public static System.Windows.Controls.Canvas canvasOverlay;
        private VideoStream _videoStream;
        TextBlock _detail;
        System.Windows.Controls.Image liveImage;
        System.Windows.Controls.Image captureImage;
        public HFImage image = new HFImage();
        public string Path { get; set; }

        public AutoCapture(VideoStream stream,TextBlock detail,Canvas overlay, System.Windows.Controls.Image liveImage, System.Windows.Controls.Image captureImage) 
        {
            _videoStream = stream;
            canvasOverlay = overlay;
            this.liveImage = liveImage;
            this.captureImage = captureImage;
            _detail = detail;
            api = new CameraApi();
        }

        public Task Capture()
        {
            return Task.Run(async () =>
            {
                UInt32 hfOperation = HFOPERATION_NONE;
                UInt32 hfContext = HFCONTEXT_NONE;
                System.IntPtr hfResult = default;

                // Open Camera Context
                hfContext = await api.OpenCameraContext();

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
                    StartImageProcess();


                    App.Current?.Dispatcher.Invoke(() =>
                    {
                        canvasOverlay.Children.Clear();
                        _detail.Text = "";
                        _videoStream.StopStreamingVideo();
                        //_liveStreamImage.Source = null;
                    });
                }
                else
                {
                    MessageBox.Show("No UareU Camera Found");
                }


            });



        }

        private void StartImageProcess()
        {
            Console.WriteLine("Start Image Processing...");
            // Parameter
            Int32 hfErrorQuality = 0;
            Int32 hfErrorSpoof = 0;
            Int32 hfErrorPosition = 0;
            double quality = 0;
            double spoof = 0;
            Int32 positioningBits = 0;

            // Loop for Get Face Data 
            Helper.VERIFY(HFSetParamInt(hfContext, (Int32)HFPARAM_CONTEXT_INT_CAPTURE_IMAGE_ENCODING, (Int32)HFIMAGE_ENCODING_PNG));
            //Helper.VERIFY(HFSetParamInt(hfContext, (Int32)HFPARAM_CONTEXT_INT_FACE_IMAGE_CROP_ENLARGE_BBOX_RATIO,(Int32)HFCROP_MODE_ENLARGED_BOUNDING_BOX));
            Helper.VERIFY(HFAsyncStartCaptureAndProcessImage(hfContext, -1, 0.5, 0.1, (UInt64)HFRESULTVALID_ALL_NO_IMAGE, (UInt64)HFRESULTVALID_ALL, ref hfOperation));
            hfStatus = (Int32)HFSTATUS_BUSY;
            while (hfStatus == (Int32)HFSTATUS_BUSY)
            {

                hfResult = (System.IntPtr)0;

                // Get the intermediate result 
                Helper.VERIFY(HFGetIntermediateResult(hfOperation, (UInt64)HFRESULTVALID_ALL_NO_IMAGE, HFSEQUENCE_NUMBER_NONE, ref hfResult));
               
                

                hfErrorQuality = HFParseResultDouble(hfResult, (UInt64)HFRESULT_DOUBLE_QUALITY, ref quality);
                hfErrorSpoof = HFParseResultDouble(hfResult, (UInt64)HFRESULT_DOUBLE_SPOOF_SCORE, ref spoof);
                hfErrorPosition = HFParseResultInt(hfResult, (Int64)HFRESULT_INT_POSITIONING_FEEDBACK, ref positioningBits);

                Helper.DrawFaceFeatures((ulong)hfResult, canvasOverlay, liveImage, Helper.PositionBitsToString(positioningBits), $"{Math.Round(quality * 100)} %");

                Helper.VERIFY(HFParseResultInt(hfResult, (Int32)HFRESULT_INT_OPERATION_STATUS, ref hfStatus));

                App.Current?.Dispatcher.Invoke(() =>
                {
                    //_quality.Text = (hfErrorQuality == (Int32)HFERROR_OK) ? $"{Math.Round(quality * 100)} %" : ErrorToString(hfErrorQuality);
                    //_position.Text = (hfErrorPosition == (Int32)HFERROR_OK) ? Helper.PositionBitsToString(positioningBits) : ErrorToString(hfErrorPosition);
                    //_spoof.Text = (hfErrorSpoof == (Int32)HFERROR_OK) ? $"{spoof * 100} %" : ErrorToString(hfErrorSpoof);
                    _detail.Text = "Processing...";
                });
                Console.WriteLine("Quality : " + quality);
                Console.WriteLine("Spoofing : " + spoof);
                Console.WriteLine("Position : " + positioningBits);

                //_HFApiStopLooping = true;


                HFFree((IntPtr)hfResult);


            }

            App.Current.Dispatcher.Invoke(() =>
            {
                _detail.Text = "Capturing...";

            });

            Helper.VERIFY(HFGetFinalResult(hfOperation, (UInt64)HFRESULTVALID_ALL, ref hfResult));
            Helper.VERIFY(HFParseResultInt(hfResult, (UInt64)HFRESULT_INT_OPERATION_STATUS, ref hfStatus));
            Helper.VERIFY(HFParseResultInt(hfResult, (UInt64)HFRESULT_INT_CONTEXT_ERROR, ref hfError));
            if (hfError == (Int32)HFSTATUS_ERROR)
            {
                MessageBox.Show("Capture Error with : " + HFGetErrorString(hfError));
            }
            //Helper.VERIFY(HFParseResultData(hfResult, HFRESULT_DATA_TEMPLATE, &templ));
            Helper.VERIFY(HFParseResultImage(hfResult, (UInt64)HFRESULT_IMAGE_FACE_IMAGE, ref image));
            if (image.data.data != null && image.data.data.Length > 0)
            {
                try
                {

                    // Show on page
                    App.Current.Dispatcher.Invoke(() => {
                        captureImage.Source = Helper.ByteToBitMapImage(image.data.data);
                        _detail.Text = "Finished";

                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    MessageBox.Show($"Failed to save image: {ex.Message}");
                }
            }


            HFFree((IntPtr)hfResult);

        }
    }
}
