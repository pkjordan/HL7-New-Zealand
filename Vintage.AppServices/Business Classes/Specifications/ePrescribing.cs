namespace Vintage.AppServices.BusinessClasses
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using ePrescribing;
    using GP2GP.Utility;
    using NZeHR.CDA;

    public class ePrescription : IValidationTest
    {
        public SpecificationType specificationType { get; set; }
        public string specificationTypeVersion { get; set; }
        public FormatType transportFormatType { get; set; }
        public string transportFormatVersion { get; set; }
        public FormatType payloadFormatType { get; set; }
        public string payloadFormatVersion { get; set; }
        public string testFileExtension { get; set; }
        public string testFileName { get; set; }
        public string documentTemplate { get; set; }

        public ePrescription()
        {
            this.specificationType = SpecificationType.ePrescribing;
            this.specificationTypeVersion = "3.9.0";
            this.transportFormatType = FormatType.XML_NZEPS;
            this.transportFormatVersion = "2.3";
            this.payloadFormatType = FormatType.CDA;
            this.payloadFormatVersion = "2.0";
            this.testFileExtension = ".xml";
            this.testFileName = "ePrescribing_v_3_9_0";
            this.documentTemplate = CdaTemplate.NZEPS_EPRESCRIPTION_V1;
        }

        static public string GetDescription()
        {
            return "ePrescribing";
        }

        public List<string> ValidateTestInstance(string testFileName, string encryptionKey)
        {
            List<string> validationResults = new List<string>();
            List<string> formatValidationResults = new List<string>();
            List<string> dataValidationResults = new List<string>();
            string templateID = string.Empty;

            // NZePS Schema Validation (not part of Toolkit)
            formatValidationResults.AddRange(ValidateNZePS(testFileName));

            // create instance of ePrescribing messaging class and use it to consume test data
            SimplMessageCodec msg = new SimplMessageCodec();
            PrescriptionDocument pd = new PrescriptionDocument();

            try
            {

                string testData = string.Empty;

                // read the file into a string as Toolkit does not create message files for NZePS
                using (StreamReader sr = File.OpenText(testFileName))
                {
                    testData = sr.ReadToEnd();
                }

                msg.encryptionKey = encryptionKey;
                msg.AddMessage(testData);

                // process transport message & add any message-related errors to list
                try
                {
                    msg.ConsumeIncomingMessage();
                    formatValidationResults.AddRange(msg.errors);
                }
                catch (Exception exFormat)
                {
                    formatValidationResults.Add("Exception : " + exFormat.Message);
                }

                // extract & validate CDA - add any errors to list
                try
                {
                    pd = msg.prescriptionDocument;

                    foreach (Template tmpl in pd.templates)
                    {
                        templateID = tmpl.oid;
                        if (templateID == this.documentTemplate)
                        {
                            break;
                        }
                    }

                    if (templateID != this.documentTemplate)
                    {
                        formatValidationResults.Add("Invalid Document Template ID " + templateID + " : expected " + this.documentTemplate);
                    }
                    else
                    {
                        pd.ValidateSchema(Specification.GetSchemaLocation());
                        formatValidationResults.AddRange(pd.schemaErrors);

                        pd.Validate();
                        dataValidationResults.AddRange(pd.allErrors);
                    }

                }
                catch (Exception exData)
                {
                    dataValidationResults.Add("Exception : " + exData.Message);
                }

            }
            catch (Exception ex)
            {
                formatValidationResults.Add("Exception : " + ex.Message);
                dataValidationResults.Add("Unable to validate due to formatting exception(s)");
            }
            finally
            {
                msg = null;
                pd = null;
            }

            validationResults.Add("FORMAT : " + (formatValidationResults.Count < 1 ? "Pass" : "Fail"));
            validationResults.AddRange(formatValidationResults);
            validationResults.Add("DATA : " + (dataValidationResults.Count < 1 ? "Pass" : "Fail"));
            validationResults.AddRange(dataValidationResults);

            return validationResults;
        }

       
        // NZePS Schema validation (not performed by CDA Toolkit)
        private static List<string> ValidateNZePS(string messageFile)
        {
            List<string> validationErrors = new List<string>();
            string nzepsSchemaFile = "NZePS.xsd";
            
            // locate schema file in application runtime folder + \schema (default)
            string schemaFolder = Directory.GetCurrentDirectory() + @"\Schema";
            if (!File.Exists(schemaFolder + @"\" + nzepsSchemaFile))
            {
                // if not there - try just the runtime folder
                schemaFolder = Directory.GetCurrentDirectory();
            }

            if (File.Exists(schemaFolder + @"\" + nzepsSchemaFile))
            {
                XDocument xdoc = XDocument.Load(messageFile);
                XmlSchemaSet set = new XmlSchemaSet();
                set.Add(null, schemaFolder + @"/" + nzepsSchemaFile);
                StringBuilder sb = new StringBuilder();
                xdoc.Validate(set, (sender, args) => { validationErrors.Add(args.Exception.Message); });
            }

            return validationErrors;
        }

    }

}