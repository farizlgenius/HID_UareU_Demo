using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UareU;
using UareU.Helpers;
using static HFApiWrapper.HFApiWrapper;
using static HFApiWrapper.HFTypesManagedEquivalents;
using static HFApiWrapper.HFTypesManagedEquivalents.HFAlgorithmType;
using static HFApiWrapper.HFTypesManagedEquivalents.HFErrors.HFErrorCodes;
using static HFApiWrapper.HFTypesManagedEquivalents.HFResultFlags;
using static HFApiWrapper.HFTypesManagedEquivalents.HFStatusCodes.HFStatus;

namespace WpfApp.Features
{
    public class Database
    {

        HFStringArray cameraNames = new HFStringArray();
        HFStringArray cameraIDs = new HFStringArray();
        UInt32 hfOperation = HFOPERATION_NONE;
        UInt32 hfContext = HFCONTEXT_NONE;
        Int32 hfStatus = (Int32)HFSTATUS_BUSY;
        Int32 hfError;
        Int32 error = (Int32)HFERROR_OK;
        System.IntPtr hfResult;
        private VideoStream _videoStream;
        TextBlock _detail;
        System.Windows.Controls.Image liveImage;
        System.Windows.Controls.Image captureImage;
        public HFImage image = new HFImage();
        HFStringArray Ids = new HFStringArray();
        ObservableCollection<FaceRecord> Records = new ObservableCollection<FaceRecord>();
        HFDatabaseRecord Record = new HFDatabaseRecord();

        public Database() { }

        public Task<ObservableCollection<FaceRecord>> GetRecord()
        {
            return Task.Run(() =>
            {
                var records = new ObservableCollection<FaceRecord>();

                //System.Windows.Controls.Canvas target;
                Console.WriteLine("Start HF API.....");
                // Now video is streaming to liveStreamImage on our GUI, lets collect the facial data from HFApi and overlay it on the video

                // Termiante first because we don't know camera status 
                HFTerminate();
                // Init HF SDK
                Helper.VERIFY(HFInit());


                Helper.VERIFY(HFEnumerateCameras(ref cameraNames, ref cameraIDs));

                if (cameraIDs.strings.Count == 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("No UareU Camera Found");
                    });
                    return records;
                }


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

                // 🔥 GET RECORDS
                Helper.VERIFY(HFListRecords(hfContext, "", ref Ids));

                Console.WriteLine(Ids.strings.Count);

                foreach (var id in Ids.strings)
                {
                    Helper.VERIFY(HFGetRecord(hfContext, id, ref Record));

                    records.Add(new FaceRecord
                    {
                        RecordId = Record.header.recordID,
                        FaceImage = Helper.ByteArrayToBitmapImage(Record.templ.data)
                    });
                }

                return records;
            }); 
        }

 
    }
}
