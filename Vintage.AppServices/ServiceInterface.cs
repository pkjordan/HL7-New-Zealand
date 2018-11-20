namespace Vintage.AppServices
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using Vintage.AppServices.BusinessClasses;
    using Vintage.AppServices.BusinessWorkflows;
    using Hl7.Fhir.Model;


    /// <summary>
    /// Exposes public interfaces for client applications (Vintage.WebServices)
    /// </summary>
    public class ServiceInterface
    {

        public bool AuthenticateUser(string userID, string password)
        {
            return WorkflowHandler.AuthenticateUser(userID, password);
        }

        public Resource GetFhirResource(string resource, string id, string operation , NameValueCollection queryParams, string fhirVersion)
        {
            return WorkflowHandler.GetFhirResource(resource, id, operation, queryParams, fhirVersion);
        }

        public List<string> GetSpecificationList()
        {
            return WorkflowHandler.GetSpecificationList();
        }

        public string GetSpecificationTestInstance(string specification)
        {
            return WorkflowHandler.GetSpecificationTestInstance(specification);
        }

        public List<string> ValidateSpecificationTestInstance(string vendorCode, string applicationName, string applicationVersion, string specification, string testInstance)
        {
            return WorkflowHandler.ValidateSpecificationTestInstance(vendorCode, applicationName, applicationVersion, specification, testInstance);
        }

        public List<GeneralPractice> GetPracticeListing(string vendorCode, string practiceName, string practiceAddress, string phoName, string dhbName, string ediAddress)
        {
            return WorkflowHandler.GetPracticeListing(vendorCode, practiceName, practiceAddress, phoName, dhbName, ediAddress);
        }

        public List<HimDirectory> GetHimDirectory(string HpiFacilityID)
        {
            return WorkflowHandler.GetHimDirectory(HpiFacilityID);
        }

        public string PutFacilityOffLine(string HpiFacilityID)
        {
            return WorkflowHandler.PutFacilityOffLine(HpiFacilityID);
        }

        public bool PostHpiMessage(string fileName, string messageHeader, string replyToMessageID, string encryptedMessage)
        {
            return WorkflowHandler.PostHpiMessage(fileName, messageHeader, replyToMessageID, encryptedMessage);
        }

        public List<HiMessageFile> GetHiMessages(string HpiFacilityId)
        {
            return WorkflowHandler.GetHiMessages(HpiFacilityId);
        }

        public List<HimAppFile> GetHimAppUpdate(string HimVersion, string HpiFacilityID)
        {
            return WorkflowHandler.GetHimAppUpdate(HimVersion, HpiFacilityID);
        }

        public string PutMessageCollectedStatus(string Gp2gpTransferId)
        {
            return WorkflowHandler.PutMessageCollectedStatus(Gp2gpTransferId);
        }


        public bool PostHimLogFile(string hpiFacilityID, string himLogData)
        {
            return WorkflowHandler.PostHimLogFile(hpiFacilityID, himLogData);
        }
    }
}
