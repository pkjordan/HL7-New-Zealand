namespace Vintage.WebServices
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using Vintage.AppServices.BusinessClasses;

    [ServiceContract(Namespace = "http://its.patientsfirst.org.nz")]
    public interface IRestService
    {
      
        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "Terminz/{resource}/{id=null}/{operation=null}")]
        Stream TerminzGet(string resource, string id, string operation);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "Terminz/{resource}/{id=null}")]
        Stream TerminzGetIdOnly(string resource, string id);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "Terminz/{resource}")]
        Stream TerminzGetResOnly(string resource);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "Terminz/")]
        Stream TerminzDefault();

        [OperationContract]
        [WebInvoke(Method = "OPTIONS",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "Terminz/")]
        Stream TerminzOptions();

        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "Terminz/{resource}/{id=null}/{operation=null}")]
        Stream TerminzPost(string resource, string id, string operation);

        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "Terminz/{resource}/{operation=null}")]
        Stream TerminzPostOpOnly(string resource, string operation);

        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "Terminz/")]
        Stream TerminzBatch();

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "SpecificationList")]
        List<string> SpecificationList();

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "SpecificationTestInstance/{specification}")]
        string SpecificationTestInstance(string specification);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "HimDirectory/{HpiFacilityId}")]
        List<HimDirectory> HimDirectory(string HpiFacilityId);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "HiMessages/{HpiFacilityId}")]
        List<HiMessageFile> HiMessages(string HpiFacilityId);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "HimAppUpdate/{HimVersion}")]
        List<HimAppFile> HimAppUpdate(string HimVersion);

        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Xml,
            RequestFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "ValidationTest")]
        List<string> ValidationTest(Stream valRequest);

        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Xml,
            RequestFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "PracticeDirectory")]
        List<GeneralPractice> PracticeDirectory(Stream dirRequest);

        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Xml,
            RequestFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "HiMessage")]
        bool HiMessage(Stream postHiMessageRequest);

        [OperationContract]
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Xml,
            RequestFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "HimLog")]
        bool HimLog(Stream postHimLogRequest);

        [OperationContract]
        [WebInvoke(Method = "PUT",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "FacilityOffLine/{HpiFacilityId}")]
        string FacilityOffLine(string HpiFacilityId);

        [OperationContract]
        [WebInvoke(Method = "PUT",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "HiMessageCollected/{Gp2GpTransferId}")]
        string HiMessageCollected(string Gp2gpTransferId);

    }

    [DataContract]
    [XmlSerializerFormat]
    public class ValidationRequest
    {
        [DataMember]
        public string vendorCode { get; set; }

        [DataMember]
        public string applicationName { get; set; }

        [DataMember]
        public string applicationVersion { get; set; }

        [DataMember]
        public string specification { get; set; }

        [DataMember]
        public string testInstance { get; set; }
    }

    [DataContract]
    [XmlSerializerFormat]
    public class PracticeDirectoryRequest
    {
        [DataMember]
        public string vendorCode { get; set; }

        [DataMember]
        public string practiceName { get; set; }

        [DataMember]
        public string practiceAddress { get; set; }

        [DataMember]
        public string phoName { get; set; }

        [DataMember]
        public string dhbName { get; set; }

        [DataMember]
        public string edi { get; set; }

    }

    [DataContract]
    [XmlSerializerFormat]
    public class SaveHiMessageRequest
    {
        [DataMember]
        public string fileName { get; set; }

        [DataMember]
        public string messageHeader { get; set; }

        [DataMember]
        public string replyToMessageID { get; set; }

        [DataMember]
        public string encryptedMessage { get; set; }

    }

    [DataContract]
    [XmlSerializerFormat]
    public class SaveHimLogRequest
    {
        [DataMember]
        public string hpiFacilityId { get; set; }

        [DataMember]
        public string himLogData { get; set; }
    }

}