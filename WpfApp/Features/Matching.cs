using Accord.Imaging.Filters;
using System;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using UareU.Helpers;
using WpfApp.HFApi;
using static HFApiWrapper.HFApiWrapper;
using static HFApiWrapper.HFTypesManagedEquivalents;
using static HFApiWrapper.HFTypesManagedEquivalents.HFAlgorithmType;
using static HFApiWrapper.HFTypesManagedEquivalents.HFErrors.HFErrorCodes;
using static HFApiWrapper.HFTypesManagedEquivalents.HFResultFlags;
using static HFApiWrapper.HFTypesManagedEquivalents.HFStatusCodes.HFStatus;

namespace WpfApp.Features
{
    public class Matching
    {
        CameraApi api;

        public Matching() 
        {
            api = new CameraApi();
        }

        public Task<double> MatchingImage(BitmapImage img1, BitmapImage img2)
        {
            return Task.Run(async () => 
            {
                UInt32 hfOperation = HFOPERATION_NONE;
                UInt32 hfContext = HFCONTEXT_NONE;
                System.IntPtr hfResult = default;
                double score = 0;
                // Open Camera Context
                hfContext = await api.OpenCameraContext();

                // Match 2 Image
                score = await api.MatchTwoImage(img1,img2,hfResult,hfOperation,hfContext);

                // Close Context
                await api.CloseCameraContext(hfContext,hfOperation,hfResult);
                return score;
            });
        }

       
    }
}
