namespace Vintage.AppServices.BusinessClasses
{
    using System;
    using System.Collections.Generic;
    using eDischarge;
    using GP2GP.Utility;
    using NZeHR.CDA;

    public class eDischargeSummary : IValidationTest
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

        public eDischargeSummary()
        {
            this.specificationType = SpecificationType.eDischargeSummary;
            this.specificationTypeVersion = "2.3.1";
            this.transportFormatType = FormatType.HL7v2;
            this.transportFormatVersion = "2.2";
            this.payloadFormatType = FormatType.CDA;
            this.payloadFormatVersion = "2.4";
            this.testFileExtension = ".hl7";
            this.testFileName = "eDischargeSummary_v_2_3_1";
            this.documentTemplate = CdaTemplate.DISCHARGE_SUMMARY_PILOT;
        }

        static public string GetDescription()
        {
            return "eDischarge"; //Summary Pilot: version 2.3.1";
        }

        public List<string> ValidateTestInstance(string testFileName, string encryptionKey)
        {
            List<string> validationResults = new List<string>();
            List<string> formatValidationResults = new List<string>();
            List<string> dataValidationResults = new List<string>();
            string templateID = string.Empty;

            // create instance of eDischarge messaging class and use it to consume test data
            DischargeMessageCodec msg = new DischargeMessageCodec();
            DischargeDocument dd = new DischargeDocument();

            try
            {

                // create CDA object from test data & add any message-related errors to list
                try
                {
                    dd = msg.ConsumeIncomingMessage(testFileName);
                    formatValidationResults.AddRange(msg.Errors);
                }
                catch (Exception exFormat)
                {
                    formatValidationResults.Add("Exception : " + exFormat.Message);
                }

                // validate CDA - add any errors to list
                try
                {
                    foreach (Template tmpl in dd.templates)
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
                        dd.ValidateSchema(Specification.GetSchemaLocation());
                        formatValidationResults.AddRange(dd.schemaErrors);

                        dd.Validate();
                        dataValidationResults.AddRange(dd.allErrors);
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
                dd = null;
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