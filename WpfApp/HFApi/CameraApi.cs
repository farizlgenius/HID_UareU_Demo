using Accord.Imaging.Filters;
using OpenCvSharp.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using UareU;
using UareU.Helpers;
using WpfApp.Models;
using static HFApiWrapper.HFApiWrapper;
using static HFApiWrapper.HFTypesManagedEquivalents;
using static HFApiWrapper.HFTypesManagedEquivalents.HFAlgorithmType;
using static HFApiWrapper.HFTypesManagedEquivalents.HFErrors.HFErrorCodes;
using static HFApiWrapper.HFTypesManagedEquivalents.HFImageEncoding;
using static HFApiWrapper.HFTypesManagedEquivalents.HFParam;
using static HFApiWrapper.HFTypesManagedEquivalents.HFResultFlags;
using static HFApiWrapper.HFTypesManagedEquivalents.HFStatusCodes;
using static HFApiWrapper.HFTypesManagedEquivalents.HFStatusCodes.HFStatus;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;


namespace WpfApp.HFApi
{
    public class CameraApi
    {

        public UInt32 hfContext  = HFCONTEXT_NONE;
        public UInt32 hfOperation = HFOPERATION_NONE;
        public IntPtr hfResult = default;
        public bool StopOperationFlag { get; set; } = false;
        Int32 hfStatus = (Int32)HFSTATUS_BUSY;

        public CameraApi() 
        {
            
        }

        public void WaitForOperation()
        {
            hfStatus = (Int32)HFSTATUS_BUSY;
            while(hfStatus == (Int32)HFSTATUS_BUSY)
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

            //Helper.ASSURE(hfStatus == (Int32)HFSTATUS_READY);

        }

        public void StopOperation()
        {
            StopOperationFlag = true;
   
        }

        public Task Authentication(Canvas canvasOverlay, System.Windows.Controls.Image liveImage)
        {
            return Task.Run(() => 
            {
                int matchSuccess = 0;
                double matchScore = 0;
                // Loop for Get Face Data 
                //Helper.VERIFY(HFSetParamInt(hfContext, (Int32)HFPARAM_CONTEXT_INT_CAPTURE_IMAGE_ENCODING, (Int32)HFIMAGE_ENCODING_PNG));
                //Helper.VERIFY(HFSetParamInt(hfContext, (Int32)HFPARAM_CONTEXT_INT_FACE_IMAGE_CROP_ENLARGE_BBOX_RATIO,(Int32)HFCROP_MODE_ENLARGED_BOUNDING_BOX));
                //Helper.VERIFY(HFAsyncStartCaptureAndProcessImage(hfContext, -1, 0.5, 0.1, (UInt64)HFRESULTVALID_ALL_NO_IMAGE, (UInt64)HFRESULTVALID_ALL, ref hfOperation));
                Helper.VERIFY(HFAsyncIdentifyWithCaptured(hfOperation,string.Empty,0.5));
                //Helper.VERIFY(HFAsyncIdentifyWithTemplate());
                if (WaitingForOperationWithGuiBox(hfResult, canvasOverlay, liveImage))
                    return;
                     
                Helper.VERIFY(HFGetFinalResult(hfOperation, (UInt64)HFRESULTVALID_ALL_NO_IMAGE, ref hfResult));
                Helper.VERIFY(HFParseResultInt(hfResult,(UInt64)HFRESULT_INT_MATCH_SUCCESS,ref matchSuccess));
                Helper.VERIFY(HFParseResultDouble(hfResult, (UInt64)HFRESULT_DOUBLE_MATCH_SCORE, ref matchScore));

                Console.WriteLine(matchSuccess);
                Console.WriteLine(matchScore);

                HFFree((IntPtr)hfResult);

            });
        }

        public Task<VerifyImage> VerifyWithImage(Canvas canvasOverlay, System.Windows.Controls.Image liveImage,BitmapImage img)
        {
            return Task.Run(async () =>
            {
                int matchSuccess = 0;
                double matchScore = 0;

                HFData uploadTemp = await ConvertImageToTemplate(Helper.BitmapImageToByteArray(img), 0);


                //// Loop for Get Face Data 
                Helper.VERIFY(HFSetParamInt(hfContext, (Int32)HFPARAM_CONTEXT_INT_CAPTURE_IMAGE_ENCODING, (Int32)HFIMAGE_ENCODING_PNG));
                //Helper.VERIFY(HFSetParamInt(hfContext, (Int32)HFPARAM_CONTEXT_INT_FACE_IMAGE_CROP_ENLARGE_BBOX_RATIO,(Int32)HFCROP_MODE_ENLARGED_BOUNDING_BOX));
                Helper.VERIFY(HFAsyncStartCaptureAndProcessImage(hfContext, -1, 0.5, 0.1, (UInt64)HFRESULTVALID_ALL_NO_IMAGE, (UInt64)HFRESULTVALID_ALL, ref hfOperation));
                if (WaitingForOperationWithGuiBox(hfResult, canvasOverlay, liveImage))
                    return null;

                Int32 hfStatus = (Int32)HFSTATUS_BUSY;
                Int32 hfError = (Int32)HFERROR_OK;
                HFImage image = new HFImage();
                HFData templ = new HFData();

                Helper.VERIFY(HFGetFinalResult(hfOperation, (UInt64)HFRESULTVALID_ALL, ref hfResult));
                Helper.VERIFY(HFParseResultInt(hfResult, (UInt64)HFRESULT_INT_OPERATION_STATUS, ref hfStatus));
                Helper.VERIFY(HFParseResultInt(hfResult, (UInt64)HFRESULT_INT_CONTEXT_ERROR, ref hfError));
                if (hfError == (Int32)HFSTATUS_ERROR)
                {
                    MessageBox.Show("Capture Error with : " + HFGetErrorString(hfError));
                }
                Helper.VERIFY(HFParseResultImage(hfResult, (UInt64)HFRESULT_IMAGE_FACE_IMAGE, ref image));
                Helper.VERIFY(HFParseResultData(hfResult, (UInt64)HFRESULT_DATA_TEMPLATE, ref templ));
                

                HFFree((IntPtr)hfResult);

                Helper.VERIFY(HFAsyncMatchWithTemplate(hfContext, 0.5,templ, uploadTemp, ref hfOperation));
                Helper.VERIFY(HFGetFinalResult(hfOperation, (UInt64)HFRESULTVALID_ALL_NO_IMAGE, ref hfResult));
                Helper.VERIFY(HFParseResultInt(hfResult, (UInt64)HFRESULT_INT_MATCH_SUCCESS, ref matchSuccess));
                Helper.VERIFY(HFParseResultDouble(hfResult, (UInt64)HFRESULT_DOUBLE_MATCH_SCORE, ref matchScore));

                Console.WriteLine(matchSuccess);
                Console.WriteLine(matchScore);
                if (image.data.data != null && image.data.data.Length > 0)
                {
                    return new VerifyImage
                    {
                        MatchScore = matchScore,
                        MatchSuccess = matchSuccess,
                        Image = image.data.data
                    };
                }

                HFFree((IntPtr)hfResult);
                return new VerifyImage();
            });
        }

        public Task<byte[]> CaptureImage(Canvas canvasOverlay, System.Windows.Controls.Image liveImage, System.Windows.Controls.Image captureImage, TextBlock detail,double Quality,double Spoofing)
        {
            return Task.Run(() => 
            {
                // Loop for Get Face Data 
                Helper.VERIFY(HFSetParamInt(hfContext, (Int32)HFPARAM_CONTEXT_INT_CAPTURE_IMAGE_ENCODING, (Int32)HFIMAGE_ENCODING_PNG));
                //Helper.VERIFY(HFSetParamInt(hfContext, (Int32)HFPARAM_CONTEXT_INT_FACE_IMAGE_CROP_ENLARGE_BBOX_RATIO,(Int32)HFCROP_MODE_ENLARGED_BOUNDING_BOX));
                Helper.VERIFY(HFAsyncStartCaptureAndProcessImage(hfContext, -1, Quality, Spoofing, (UInt64)HFRESULTVALID_ALL_NO_IMAGE, (UInt64)HFRESULTVALID_ALL, ref hfOperation));
                if (WaitingForOperationWithGuiBox(hfResult, canvasOverlay, liveImage))
                    return null;

                Int32 hfStatus = (Int32)HFSTATUS_BUSY;
                Int32 hfError = (Int32)HFERROR_OK;
                HFImage image = new HFImage();

                Helper.VERIFY(HFGetFinalResult(hfOperation, (UInt64)HFRESULTVALID_ALL, ref hfResult));
                Helper.VERIFY(HFParseResultInt(hfResult, (UInt64)HFRESULT_INT_OPERATION_STATUS, ref hfStatus));
                Helper.VERIFY(HFParseResultInt(hfResult, (UInt64)HFRESULT_INT_CONTEXT_ERROR, ref hfError));
                if (hfError == (Int32)HFSTATUS_ERROR)
                {
                    MessageBox.Show("Capture Error with : " + HFGetErrorString(hfError));
                    return null;
                }
                //Helper.VERIFY(HFParseResultData(hfResult, HFRESULT_DATA_TEMPLATE, &templ));
                Helper.VERIFY(HFParseResultImage(hfResult, (UInt64)HFRESULT_IMAGE_FACE_IMAGE, ref image));
                if (image.data.data != null && image.data.data.Length > 0)
                {
                    return image.data.data;
                }


                HFFree((IntPtr)hfResult);

                return null;

            });
        }

        public Task ListGallery()
        {
            HFStringArray gal = new HFStringArray();
            return Task.Run(() =>
            {
                Helper.VERIFY(HFListGalleries(hfContext, ref gal));
                Console.WriteLine(gal.strings.Count);


            });
        }

        public Task<HFMatchRecord> IdentifyImage(byte[] image)
        {
            return Task.Run(async () =>
            {
                HFData template1 = await ConvertImageToTemplate(image, 0);
                HFMatchGallery gallery = new HFMatchGallery();
                int matchSuccess = 0;
                double minMatchScore = 0;

                HFStringArray gal = new HFStringArray();
                Helper.VERIFY(HFListGalleries(hfContext, ref gal));
                if (gal.strings.Count == 0)
                    return null;

                Helper.VERIFY(HFGetParamDouble(hfContext, (UInt32)HFPARAM_CONTEXT_DOUBLE_REC_MIN_MATCH_SCORE_L3, ref minMatchScore));
                Helper.VERIFY(HFAsyncIdentifyWithTemplate(hfContext, gal.strings.ElementAt(0), minMatchScore, template1, ref hfOperation));
                WaitForOperation();
                Helper.VERIFY(HFGetFinalResult(hfOperation, (UInt64)HFRESULTVALID_ALL, ref hfResult));
                Helper.VERIFY(HFParseResultInt(hfResult, (UInt64)HFRESULT_INT_MATCH_SUCCESS, ref matchSuccess));
                if (matchSuccess == 1)
                {
                    Helper.VERIFY(HFParseResultMatchGallery(hfResult, (UInt32)HFRESULT_MATCHGALLERY_MATCH_GALLERY, ref gallery));
                    if (gallery.recordsCount == 0)
                        return null;

                   return gallery.records.OrderByDescending(x => x.matchScore).FirstOrDefault();
                }

                return null;
            });
        }


        public Task<bool> CheckExistsFace(byte[] image)
        {

            return Task.Run(async () =>
            {
                HFData template1 = await ConvertImageToTemplate(image, 0);
                int matchSuccess = 0;
                double minMatchScore = 0;

                HFStringArray gal = new HFStringArray();
                Helper.VERIFY(HFListGalleries(hfContext, ref gal));
                if (gal.strings.Count == 0)
                    return false;

                Helper.VERIFY(HFGetParamDouble(hfContext, (UInt32)HFPARAM_CONTEXT_DOUBLE_REC_MIN_MATCH_SCORE_L3, ref minMatchScore));
                Helper.VERIFY(HFAsyncIdentifyWithTemplate(hfContext, gal.strings.ElementAt(0), minMatchScore, template1, ref hfOperation));
                WaitForOperation();
                Helper.VERIFY(HFGetFinalResult(hfOperation, (UInt64)HFRESULTVALID_ALL, ref hfResult));
                Helper.VERIFY(HFParseResultInt(hfResult, (UInt64)HFRESULT_INT_MATCH_SUCCESS, ref matchSuccess));
                if (matchSuccess == 1)
                    return true;


                return false;
            });
        }


        public Task AddRecord(BitmapImage img)
        {
            return Task.Run(async () => {
                HFData template1 = await ConvertImageToTemplate(Helper.BitmapImageToByteArray(img), 0);
                HFStringArray gal = new HFStringArray();
                string galId = "";
                string id = Guid.NewGuid().ToString();
                Helper.VERIFY(HFListGalleries(hfContext, ref gal));
                if(gal.strings.Count > 0)
                {
                    galId = gal.strings.ElementAt(0);
                }
                else
                {
                    galId = id;
                }

               
                // Add record
                HFDatabaseRecordHeader header = new HFDatabaseRecordHeader
                {
                    recordID = id,
                    customData = new HFData()
                }
                ;
                HFDatabaseRecord record = new HFDatabaseRecord
                {
                    header = header,
                    templ = template1,
                };

                Helper.VERIFY(HFAddRecordWithTemplate(hfContext, record, true));
                Helper.SaveBitmapImageToFile(img, $"{header.recordID}.png");

                // Add record to Gallery
                Helper.VERIFY(HFAddRecordToGallery(hfContext, id, galId));

            });
        }

       

        public Task<byte[]> CaptureImageFromLiveStream(Canvas canvasOverlay, System.Windows.Controls.Image liveImage, System.Windows.Controls.Image captureImage, TextBlock detail)
        {
            return Task.Run(() =>
            {
                // Loop for Get Face Data 
                Helper.VERIFY(HFSetParamInt(hfContext, (Int32)HFPARAM_CONTEXT_INT_CAPTURE_IMAGE_ENCODING, (Int32)HFIMAGE_ENCODING_PNG));
                //Helper.VERIFY(HFSetParamInt(hfContext, (Int32)HFPARAM_CONTEXT_INT_FACE_IMAGE_CROP_ENLARGE_BBOX_RATIO,(Int32)HFCROP_MODE_ENLARGED_BOUNDING_BOX));
                Helper.VERIFY(HFAsyncStartCaptureAndProcessImage(hfContext, -1, 0.5, 0.1, (UInt64)HFRESULTVALID_ALL_NO_IMAGE, (UInt64)HFRESULTVALID_ALL, ref hfOperation));
                if (WaitingForOperationWithGuiBox(hfResult, canvasOverlay, liveImage))
                    return null;


                Int32 hfStatus = (Int32)HFSTATUS_BUSY;
                Int32 hfError = (Int32)HFERROR_OK;
                HFImage image = new HFImage();
                HFData templ = new HFData();

                Helper.VERIFY(HFGetFinalResult(hfOperation, (UInt64)HFRESULTVALID_ALL, ref hfResult));
                Helper.VERIFY(HFParseResultInt(hfResult, (UInt64)HFRESULT_INT_OPERATION_STATUS, ref hfStatus));
                Helper.VERIFY(HFParseResultInt(hfResult, (UInt64)HFRESULT_INT_CONTEXT_ERROR, ref hfError));
                if (hfError == (Int32)HFSTATUS_ERROR)
                {
                    MessageBox.Show("Capture Error with : " + HFGetErrorString(hfError));
                }
                Helper.VERIFY(HFParseResultData(hfResult, (UInt64)HFRESULT_DATA_TEMPLATE,ref templ));
                Helper.VERIFY(HFParseResultImage(hfResult, (UInt64)HFRESULT_IMAGE_FACE_IMAGE, ref image));
                

                if (image.data.data != null && image.data.data.Length > 0)
                {
                    
                    return image.data.data;
                }


                HFFree((IntPtr)hfResult);

                return null;

            });
        }

        public bool WaitingForOperationWithGuiBox(System.IntPtr hfResult, Canvas canvasOverlay, System.Windows.Controls.Image liveImage)
        {
            double quality = 0;
            double spoof = 0;
            Int32 positioningBits = 0;
            Int32 hfStatus = (Int32)HFSTATUS_BUSY;
            while (hfStatus == (Int32)HFSTATUS_BUSY)
            {

                hfResult = (System.IntPtr)0;

                // Get the intermediate result 
                Helper.VERIFY(HFGetIntermediateResult(hfOperation, (UInt64)HFRESULTVALID_ALL_NO_IMAGE, HFSEQUENCE_NUMBER_NONE, ref hfResult));

                HFParseResultDouble(hfResult, (UInt64)HFRESULT_DOUBLE_QUALITY, ref quality);
                HFParseResultDouble(hfResult, (UInt64)HFRESULT_DOUBLE_SPOOF_SCORE, ref spoof);
                HFParseResultInt(hfResult, (Int64)HFRESULT_INT_POSITIONING_FEEDBACK, ref positioningBits);

                Helper.DrawFaceFeatures((ulong)hfResult, canvasOverlay, liveImage, Helper.PositionBitsToString(positioningBits), $"{Math.Round(quality * 100)} %", $"{spoof}");

                Helper.VERIFY(HFParseResultInt(hfResult, (Int32)HFRESULT_INT_OPERATION_STATUS, ref hfStatus));


                HFFree((IntPtr)hfResult);

                if (StopOperationFlag)
                {
                    Helper.VERIFY(HFStopOperation(hfOperation));
                    StopOperationFlag = false;
                    return true;
                }

            }
            return false;

            //Helper.ASSURE(hfStatus == (Int32)HFSTATUS_READY);
        }

        public Task OpenCameraContext()
        {
            return Task.Run(() => 
            {
                Console.WriteLine("Open Context Start");

                HFStringArray cameraNames = new HFStringArray();
                HFStringArray cameraIDs = new HFStringArray();
               

                Helper.VERIFY(HFInit());
                Helper.VERIFY(HFEnumerateCameras(ref cameraNames,ref cameraIDs));
                if (cameraIDs.strings.Count <= 0)
                {
                    HFTerminate();
                }

                Helper.VERIFY(HFOpenContext(cameraIDs.strings[0], (Int32)HFALGORITHM_TYPE_ON_DEVICE,ref hfContext,ref hfOperation));
                WaitForOperation();
                Helper.VERIFY(HFCloseOperation(hfOperation));
                Console.WriteLine("Open Context Finish");
                return hfContext;
            });
        }

        public Task CloseCameraContext()
        {
            return Task.Run(() => 
            {
                Helper.VERIFY(HFCloseContext(hfContext,ref hfOperation));
                WaitForOperation();
                HFFree(hfResult);
                HFTerminate();
            });
        }

        public Task<HFStringArray> GetRecordList()
        {
            return Task.Run(() =>
            {
                HFStringArray Ids = new HFStringArray();
                // 🔥 GET RECORDS
                Helper.VERIFY(HFListRecords(hfContext, "", ref Ids));
                return Ids;
            });
        }

        public Task<double> MatchTwoImage(BitmapImage img1, BitmapImage img2)
        {
            return Task.Run(async () => 
            {
                HFData template1 = await ConvertImageToTemplate(Helper.BitmapImageToByteArray(img1), 0);
                HFData template2 = await ConvertImageToTemplate(Helper.BitmapImageToByteArray(img2), 0);
                double matchScore = 0;
                if (template1 == null && template2 == null)
                {
                    return 0d;
                }

                Helper.VERIFY(HFAsyncMatchWithTemplate(hfContext, 0.5, template1, template2, ref hfOperation));
                WaitForOperation();
                Helper.VERIFY(HFGetFinalResult(hfOperation, (UInt64)HFRESULTVALID_ALL, ref hfResult));
                Helper.VERIFY(HFParseResultDouble(hfResult, (UInt64)HFRESULT_DOUBLE_MATCH_SCORE, ref matchScore));

                return matchScore;

            });
            
        }

        public Task<HFData> ConvertImageToTemplate(byte[] ImageData, short Type)
        {
            return Task.Run(() =>
            {
                HFData template = new HFData();
                HFImage Image = new HFImage();
                Image.data.data = ImageData;
                Image.data.size = (uint)ImageData.Length;
                Int32 error = (Int32)HFERROR_OK;
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
                Helper.VERIFY(HFAsyncProcessImage(hfContext, Image, 0.1, 0.1, (Int64)HFRESULT_DATA_TEMPLATE, ref hfOperation));
                WaitForOperation();
                Console.WriteLine(hfResult);
                Helper.VERIFY(HFGetFinalResult(hfOperation, (UInt64)HFRESULTVALID_ALL, ref hfResult));
                error = Helper.VERIFY(HFParseResultData(hfResult, (UInt64)HFRESULT_DATA_TEMPLATE, ref template));
                if (error == 3)
                {
                    MessageBox.Show("There're no person in image");
                    return null;
                }
                return template;
            } );
        }

        public Task DeleteRecord(string recordId)
        {
            return Task.Run(() => 
            {
                Helper.VERIFY(HFDeleteRecord(hfContext,recordId));
                
            });
        }





    }
}
