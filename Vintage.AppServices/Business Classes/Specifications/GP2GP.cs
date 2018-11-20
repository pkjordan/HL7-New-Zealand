namespace Vintage.AppServices.BusinessClasses
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using GP2GP;
    using GP2GP.Utility;
    using NZeHR.CDA;

    public class Gp2Gp : IValidationTest
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

        public Gp2Gp()
        {
            this.specificationType = SpecificationType.GP2GP;
            this.specificationTypeVersion = "1.7.0";
            this.transportFormatType = FormatType.HL7v2;
            this.transportFormatVersion = "2.4";
            this.payloadFormatType = FormatType.CDA;
            this.payloadFormatVersion = "2.0";
            this.testFileExtension = ".hl7";
            this.testFileName = "GP2GP_v_1_7_0";
            this.documentTemplate = CdaTemplate.GP2GP_V1;
        }

        static public string GetDescription()
        {
            return "GP2GP" ; //: version 1.7.0";
        }

        public List<string> ValidateTestInstance(string testFileName, string encryptionKey)
        {
            List<string> validationResults = new List<string>();
            List<string> formatValidationResults = new List<string>();
            List<string> dataValidationResults = new List<string>();

            // create instance of GP2GP messaging class and use it to consume test data
            MessageCodec msg = new MessageCodec();
            ClinicalDocument cd = new ClinicalDocument();
            string templateID = string.Empty;

            try
            {

                // create CDA object from test data & add any message-related errors to list
                try
                {
                    if (Path.GetFileName(testFileName).StartsWith("ENC_"))
                    {
                        cd = msg.ConsumeIncomingMessage(testFileName, encryptionKey);
                    }
                    else
                    {
                        cd = msg.ConsumeIncomingMessage(testFileName);
                    }

                    formatValidationResults.AddRange(msg.Errors);
                }
                catch (Exception exFormat)
                {
                    formatValidationResults.Add("Exception : " + exFormat.Message);
                }

                // extract & validate CDA - add any errors to list
                try
                {

                    foreach (Template tmpl in cd.templates)
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
                        cd.ValidateSchema(Specification.GetSchemaLocation());
                        formatValidationResults.AddRange(cd.schemaErrors);

                        cd.Validate();
                        dataValidationResults.AddRange(cd.allErrors);
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
                cd = null;
                msg = null;
            }

            validationResults.Add("FORMAT : " + (formatValidationResults.Count < 1 ? "Pass" : "Fail"));
            validationResults.AddRange(formatValidationResults);
            validationResults.Add("DATA : " + (dataValidationResults.Count < 1 ? "Pass" : "Fail"));
            validationResults.AddRange(dataValidationResults);

            return validationResults;
        }

    }

}
