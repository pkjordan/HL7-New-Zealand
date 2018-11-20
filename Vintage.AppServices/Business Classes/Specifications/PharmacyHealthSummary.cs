namespace Vintage.AppServices.BusinessClasses
{
    using System;
    using System.Collections.Generic;
    using NZeHR.CDA;
    using NZeHR.Documents;

    public class PharmacyHealthSummary : IValidationTest
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

        public PharmacyHealthSummary()
        {
            this.specificationType = SpecificationType.PharmacyHealthSummary;
            this.specificationTypeVersion = "3.3.0";
            this.transportFormatType = FormatType.CDA;
            this.transportFormatVersion = "2.0";
            this.payloadFormatType = FormatType.CDA;
            this.payloadFormatVersion = "2.0";
            this.testFileExtension = ".xml";
            this.testFileName = "pharmacy_health_summary_v_3_3_0";
            this.documentTemplate = CdaTemplate.NZEPS_SHARED_HEALTH_SUMMARY_V1;
        }

        static public string GetDescription()
        {
            return "PharmacyHealthSummary";
        }

        public List<string> ValidateTestInstance(string testFileName, string encryptionKey)
        {

            List<string> validationResults = new List<string>();
            PharmacyHealthSummaryDocument testInstance = new PharmacyHealthSummaryDocument();

            try
            {
                testInstance = new PharmacyHealthSummaryDocument(testFileName);
                validationResults = Specification.ValidateNZeHR(testInstance, this.documentTemplate);
            }
            catch (Exception ex)
            {
                validationResults.Add("FORMAT : Fail");
                validationResults.Add("Exception : " + ex.Message);
                validationResults.Add("DATA : Fail");
                validationResults.Add("Unable to validate due to formatting exception(s)");
            }
            finally
            {
                testInstance = null;
            }

            return validationResults;
        }

    }

}