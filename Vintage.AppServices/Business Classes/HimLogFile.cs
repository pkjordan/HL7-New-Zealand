namespace Vintage.AppServices.BusinessClasses
{
    using System;
    using System.Configuration;
    using System.IO;

    public static class HimLogFile
    {

        /// <summary>
        /// Save (non-empty) Facility Client Log Files to Folder set in Configuration File
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="messageText"></param>
        /// <returns>full file name - including folder</returns>
        public static void SaveFileToDisk(string hpiFacilityID, string himLogData)
        {
            if (himLogData.Length > 0)  // 4.3.0.8 added feature to prevent saving of empty log files
            {
                // create file name
                string fileName = hpiFacilityID + "_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".log";

                // get Folder Name
                string bSlash = @"\";
                string logFileFolder = ConfigurationManager.AppSettings["HimLogFolder"].ToString();
                string folderName = (logFileFolder.EndsWith(bSlash) ? logFileFolder : logFileFolder + bSlash);

                // save File
                byte[] fileBytes = Convert.FromBase64String(himLogData);
                File.WriteAllBytes(folderName + fileName, fileBytes);
            }

        }
    }

}
