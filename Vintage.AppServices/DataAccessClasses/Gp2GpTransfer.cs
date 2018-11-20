namespace Vintage.AppServices.DataAccessClasses
{
    using System.Collections.Generic;
    using System.Linq;
    using Vintage.AppServices.BusinessClasses;

    public static class Gp2GpTransfer
    {
        public static void AddGp2GpTransfer( HiMessageFile mf)
        {
            mf.applicationType = "GP2GP"; // some PMS omit this in Ack messages

            using (PatientsFirstDataContext dc = new PatientsFirstDataContext())
            {
                dc.Gp2GpTransfers_Insert(
                    mf.senderEDI,
                    mf.receiverEDI, 
                    mf.messageId, 
                    mf.messageYear, 
                    mf.messageMonth, 
                    mf.messageDay, 
                    mf.applicationType, 
                    mf.fileSize, 
                    mf.sendingApplication, 
                    mf.responseMessageIndicator, 
                    mf.replyToMessageId, 
                    mf.messageFileName);
            }
        }

        public static List<HiMessageFile> GetUncollectedMessages(string hpiFacilityID)
        {
            List<HiMessageFile> hpiMessageList = new List<HiMessageFile>();

            List<GetUncollectedGp2GpTransfersResult> transfers = new List<GetUncollectedGp2GpTransfersResult>();

            using (PatientsFirstDataContext dc = new PatientsFirstDataContext())
            {
                transfers = dc.GetUncollectedGp2GpTransfers(hpiFacilityID).ToList();
            }

            foreach (GetUncollectedGp2GpTransfersResult tx in transfers)
            {
                HiMessageFile mf = new HiMessageFile();
                mf.dbKey = tx.Gp2GpTransferId;
                mf.messageFileName = tx.MessageFileName;
                mf.messageId = tx.MessageId;

                hpiMessageList.Add(mf);
            }

            return hpiMessageList;
        }

        public static void UpdateMessageCollectedStatus(int gp2gpTransferID)
        {
            using (PatientsFirstDataContext dc = new PatientsFirstDataContext())
            {
                dc.Gp2GpTransfers_UpdateCollected(gp2gpTransferID);
            }
        }

        public static string GetMessageFileName(int gp2gpTransferID)
        {
            string fileName = string.Empty;

            using (PatientsFirstDataContext dc = new PatientsFirstDataContext())
            {

               fileName = dc.GetGp2GpMessageFileName(gp2gpTransferID);
            }

            return fileName;
        }

    }
}
