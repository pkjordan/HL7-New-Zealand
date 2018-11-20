namespace Vintage.AppServices.BusinessClasses
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;

    public class HimAppFile
    {
        public string fileName { get; set; }
        public string fileVersion { get; set; }
        public string fileText { get; set; }
        
        /// <summary>
        /// Default Constructor
        /// </summary>
        public HimAppFile()
        {
            this.fileName = string.Empty;
            this.fileVersion = string.Empty;
            this.fileText = string.Empty;
        }

        public bool GetFileTextFromDisk()
        {
            bool fileLoaded = false;

            string messageFolder = ConfigurationManager.AppSettings["HimAppFolder"].ToString();

            if (File.Exists(messageFolder + this.fileName))
            {
                // read the  file into a byte array and then convert to a base64 string for XML transport
                byte[] fileBytes = File.ReadAllBytes(messageFolder + this.fileName);
                this.fileText = Convert.ToBase64String(fileBytes);
                fileLoaded = true;
            }

            return fileLoaded;

        }

        public void GetFileVersion()
        {
            string messageFolder = ConfigurationManager.AppSettings["HimAppFolder"].ToString();

            this.fileVersion = FileVersionInfo.GetVersionInfo(messageFolder + this.fileName).FileVersion;
        }

    }
}