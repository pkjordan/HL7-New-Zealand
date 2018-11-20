namespace Vintage.AppServices.BusinessClasses
{
    using System.Configuration;
    using System.IO;

    public class HiMessageFile
    {

           // Note ALL City & Postcode, vendor, responding & file size groups updated by Stored Procedures called from SSIS Package 
           // that loads Healthlink Transfers each month

            public string senderEDI { get; set; }
            public string receiverEDI { get; set; }
            public string messageId { get; set; }
            public short? messageYear { get; set; }
            public byte? messageMonth { get; set; }
            public byte? messageDay { get; set; }
            public string applicationType { get; set; }
            public long? fileSize { get; set; }
            public string sendingApplication { get; set; }
            public char? responseMessageIndicator { get; set; }
            public string replyToMessageId { get; set; }
            public string messageFileName { get; set; }
            public int? dbKey { get; set; }
            public string fileText { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public HiMessageFile()
        {
        }

        /// <summary>
        /// Constructor that populates properties from Message Header
        /// </summary>
        /// MSH|^~\&|MyVipIntraMed 3.45c|patftest|GP2GP|gpnztest|20140818141150|PKI|ZGP^R01^ZGP_R01|20140818141148188|P|2.4
        public HiMessageFile(string header)
        {
            this.sendingApplication = GetComponentFromSegment(header, 2, 0);
            this.senderEDI = GetComponentFromSegment(header, 3, 0);
            this.applicationType = GetComponentFromSegment(header, 4, 0);
            this.receiverEDI = GetComponentFromSegment(header, 5, 0);
            string messageDate = GetComponentFromSegment(header, 6, 0);
            this.messageYear = short.Parse(messageDate.Substring(0,4));
            this.messageMonth = byte.Parse(messageDate.Substring(4, 2));
            this.messageDay = byte.Parse(messageDate.Substring(6, 2));
            this.responseMessageIndicator = (GetComponentFromSegment(header, 8, 0).StartsWith("ACK")) ? 'T' : 'F';
            this.messageId = GetComponentFromSegment(header, 9, 0);
        }

        /// <summary>
        /// Save Message File to disk Folder set in Configuration File
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="encryptedMessage"></param>
        /// <returns>full file name - including folder</returns>
        public string SaveMessageToDisk(string fileName, string encryptedMessage)
        {
            // get Folder Name
            string bSlash = @"\";
            string messageFolder = ConfigurationManager.AppSettings["HiMessageFolder"].ToString();
            string folderName = (messageFolder.EndsWith(bSlash) ? messageFolder : messageFolder + bSlash);

            // save File
            File.WriteAllText(folderName + fileName, encryptedMessage);

            return folderName + fileName;
        }

        public string GetMessageFromDisk(string fileName)
        {
            string fileText = string.Empty;

            // read the file into a string
            using (StreamReader sr = File.OpenText(fileName))
            {
                fileText = sr.ReadToEnd();
            }

            return fileText;
        }

        public bool DeleteMessageFromDisk(string fileName)
        {
            bool rv = true;

            try
            {
                File.Delete(fileName);
            }
            catch
            {
                rv = false;
            }

            return rv;
        }


        public long GetFileSize(string messageFileName)
        {
            FileInfo fi = new FileInfo(messageFileName);
            return fi.Length;
        }

        private static string GetComponentFromSegment(string segment, int FieldNo, int compNo)
        {
            string returnValue = string.Empty;
            char fieldSeparator = '|';
            char compSeparator = '^';
            string compSep = "^";

            try
            {
                string[] fields = segment.Split(new char[] { fieldSeparator });
                returnValue = fields[FieldNo];

                if (returnValue.Contains(compSep))
                {
                    string[] comps = returnValue.Split(compSeparator);
                    returnValue = comps[compNo];
                }
                else if (compNo > 0)
                {
                    returnValue = string.Empty;
                }
            }
            catch { }

            return returnValue;
        }
    }

}
