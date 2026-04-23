using Accord.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using UareU.Helpers;
using static HFApiWrapper.HFApiWrapper;
using static HFApiWrapper.HFTypesManagedEquivalents;
using static HFApiWrapper.HFTypesManagedEquivalents.HFAlgorithmType;
using static HFApiWrapper.HFTypesManagedEquivalents.HFErrors.HFErrorCodes;
using static HFApiWrapper.HFTypesManagedEquivalents.HFResultFlags;
using static HFApiWrapper.HFTypesManagedEquivalents.HFStatusCodes;
using static HFApiWrapper.HFTypesManagedEquivalents.HFStatusCodes.HFStatus;


namespace WpfApp.HFApi
{
    public class CameraApi
    {
        
        public CameraApi() { }

        public void WaitForOperation(UInt32 hfOperation, System.IntPtr hfResult)
        {
            Int32 hfStatus = (Int32)HFSTATUS_BUSY;
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

            Helper.ASSURE(hfStatus == (Int32)HFSTATUS_READY);

        }

        public Task<UInt32> OpenCameraContext()
        {
            return Task.Run(() => 
            {
                UInt32 hfContext = HFCONTEXT_NONE;
                UInt32 hfOperation = HFOPERATION_NONE;
                System.IntPtr hfResult = default;

                HFStringArray cameraNames = new HFStringArray();
                HFStringArray cameraIDs = new HFStringArray();

                Helper.VERIFY(HFInit());
                Helper.VERIFY(HFEnumerateCameras(ref cameraNames,ref cameraIDs));
                if (cameraIDs.strings.Count <= 0)
                {
                    HFTerminate();
                    return hfContext;
                }

                Helper.VERIFY(HFOpenContext(cameraIDs.strings[0], (Int32)HFALGORITHM_TYPE_ON_DEVICE,ref hfContext,ref hfOperation));
                WaitForOperation(hfOperation, hfResult);
                Helper.VERIFY(HFCloseOperation(hfOperation));

                return hfContext;
            });
        }

        public Task CloseCameraContext(UInt32 hfContext,UInt32 hfOperation,IntPtr hfResult)
        {
            return Task.Run(() => 
            {
                Helper.VERIFY(HFCloseContext(hfContext,ref hfOperation));
                WaitForOperation(hfOperation,hfResult);
                HFFree(hfResult);
                HFTerminate();
            });
        }

        public Task<double> MatchTwoImage(BitmapImage img1, BitmapImage img2,IntPtr hfResult,UInt32 hfOperation,UInt32 hfContext)
        {
            return Task.Run(async () => 
            {
                HFData template1 = await ConvertImageToTemplate(Helper.BitmapImageToByteArray(img1), 0,hfResult,hfOperation,hfContext);
                HFData template2 = await ConvertImageToTemplate(Helper.BitmapImageToByteArray(img2), 0, hfResult, hfOperation, hfContext);
                double matchScore = 0;
                if (template1 == null && template2 == null)
                {
                    return 0d;
                }

                Helper.VERIFY(HFAsyncMatchWithTemplate(hfContext, 0.5, template1, template2, ref hfOperation));
                WaitForOperation(hfOperation,hfResult);
                Helper.VERIFY(HFGetFinalResult(hfOperation, (UInt64)HFRESULTVALID_ALL, ref hfResult));
                Helper.VERIFY(HFParseResultDouble(hfResult, (UInt64)HFRESULT_DOUBLE_MATCH_SCORE, ref matchScore));

                return matchScore;

            });
            
        }

        public Task<HFData> ConvertImageToTemplate(byte[] ImageData, short Type,IntPtr hfResult,UInt32 hfOperation, UInt32 hfContext)
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
                WaitForOperation(hfOperation,hfResult);
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

        public Task CaptureImage()
        {
            return Task.Run(() => 
            {

            });
        }
    }
}
