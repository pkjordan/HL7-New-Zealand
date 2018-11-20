namespace Vintage.AppServices.BusinessClasses
{
    using System;
    using System.Collections.Generic;
    using NZeHR.CDA;
    using NZeHR.Documents;

    public class InterRaiContact : IValidationTest
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

        public InterRaiContact()
        {
            this.specificationType = SpecificationType.InterRaiContact;
            this.specificationTypeVersion = "3.3.0";
            this.transportFormatType = FormatType.CDA;
            this.transportFormatVersion = "2.0";
            this.payloadFormatType = FormatType.CDA;
            this.payloadFormatVersion = "2.0";
            this.testFileExtension = ".xml";
            this.testFileName = "interRAI_ca_v_3_3_0";
            this.documentTemplate = CdaTemplate.INTERRAI_CONTACT_V1;
        }

        static public string GetDescription()
        {
            return "InterRaiContact";
        }

        public List<string> ValidateTestInstance(string testFileName, string encryptionKey)
        {
            List<string> validationResults = new List<string>();
            InterRaiContactDocument testInstance = new InterRaiContactDocument();

            try
            {
                testInstance = new InterRaiContactDocument(testFileName);
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