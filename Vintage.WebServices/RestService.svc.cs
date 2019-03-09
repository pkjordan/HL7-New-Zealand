namespace Vintage.WebServices
{
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Rest;
    using Hl7.Fhir.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Xml.Linq;
    using Vintage.AppServices;
    using Vintage.AppServices.BusinessClasses;

    public class RestService : IRestService
    {

        string userID = string.Empty;
        string password = string.Empty;
        NameValueCollection qParam = new NameValueCollection();

        public Stream TerminzBatch()
        {
            string rv = string.Empty;

            string responseType = GetResponseType();
            string fhirVersion = GetFhirVersion();

            // get Bundle from the Request Stream

            string reqBody = string.Empty;
            UTF8Encoding enc = new UTF8Encoding();
            Stream reqStream = OperationContext.Current.RequestContext.RequestMessage.GetBody<Stream>();

            using (StreamReader reader = new StreamReader(reqStream, enc))
            {
                reqBody = reader.ReadToEnd();
            }

            string reqType = WebOperationContext.Current.IncomingRequest.ContentType.ToLower();

            Resource fhirResource;

            if (reqType.Contains("json"))
            {
                FhirJsonParser fjp = new FhirJsonParser();
                fhirResource = fjp.Parse<Resource>(reqBody);
            }
            else
            {
                FhirXmlParser fxp = new FhirXmlParser();
                fhirResource = fxp.Parse<Resource>(reqBody);
            }

            if (fhirResource.ResourceType == ResourceType.Bundle)
            {
                Bundle fb = (Bundle)fhirResource;
                if (fb.Type == Bundle.BundleType.Batch)
                {
                    Bundle responseBundle = new Bundle();

                    responseBundle.Id = Guid.NewGuid().ToString();
                    responseBundle.Type = Bundle.BundleType.BatchResponse;

                   foreach (Bundle.EntryComponent comp in fb.Entry)
                    {
                        Bundle.RequestComponent rc = comp.Request;

                        if (rc.Method == Bundle.HTTPVerb.GET)
                        {
                            if (rc.Url.IndexOf("$validate-code") > 0)
                            {
                                // extract and parse query string to get parameters
                                int qsPos = rc.Url.IndexOf('?');
                                string querystring = (qsPos < rc.Url.Length - 1) ? rc.Url.Substring(qsPos + 1) : String.Empty;
                                qParam = System.Web.HttpUtility.ParseQueryString(querystring);                                
                                // extract resource (CodeSystem or ValueSet) ID
                                string resourceID = string.Empty;
                                string resourceName = resourceID.IndexOf("/ValueSet/") > 0 ? "ValueSet" : "CodeSystem";
                                try
                                {
                                    resourceID = rc.Url.Remove(qsPos);
                                    int resNamePos = resourceID.IndexOf("/" + resourceName + "/");
                                    resourceID = resourceID.Substring(resNamePos + 9).Replace("/", "").Replace("$validate-code", ""); 
                                }
                                catch { }
                                ServiceInterface appSi = new ServiceInterface();
                                responseBundle.AddResourceEntry(appSi.GetFhirResource(resourceName, resourceID, "$validate-code", qParam, fhirVersion),string.Empty);
                            }
                            else
                            {
                                responseBundle.AddResourceEntry(OperationOutcome.ForMessage("Unrecognised Operation in Request..." + rc.Url, OperationOutcome.IssueType.Unknown),string.Empty);
                            }
                        }
                        else if (rc.Method == Bundle.HTTPVerb.POST)
                        {
                            if (rc.Url.IndexOf("$translate") > 0 && comp.Resource.ResourceType == ResourceType.Parameters)
                            {
                                // extract ConceptMap ID
                                string cmID = string.Empty;
                                try 
                                {
                                    cmID = rc.Url.Replace("ConceptMap/", "").Replace("$translate", "").Replace("/", "");
                                }
                                catch { }
                                // get parameters
                                Parameters fParam = (Parameters)comp.Resource;
                                SetQueryParameters(fParam);
                                ServiceInterface appSi = new ServiceInterface();
                                responseBundle.AddResourceEntry(appSi.GetFhirResource("ConceptMap", cmID, "$translate", qParam,fhirVersion),string.Empty);                               
                            }
                            else
                            {
                                responseBundle.AddResourceEntry(OperationOutcome.ForMessage("Unrecognised Operation in Request..." + rc.Url, OperationOutcome.IssueType.Unknown), string.Empty);
                            }
                        }
                        else
                        {
                            responseBundle.AddResourceEntry(OperationOutcome.ForMessage("Method Not Supported in Batch Mode '" + rc.Method.ToString() + "'", OperationOutcome.IssueType.NotSupported),string.Empty);
                        }
                    }

                   fhirResource = responseBundle;
                }
                else
                {
                    fhirResource = OperationOutcome.ForMessage("No module could be found to handle the bundle type '" + fb.TypeName + "'", OperationOutcome.IssueType.NotFound);
                }
            }
            else
            {
                fhirResource = OperationOutcome.ForMessage("No module could be found to handle the request '" + fhirResource.ResourceType.ToString() + "'", OperationOutcome.IssueType.NotFound);
            }       

            if (fhirResource.ResourceType == ResourceType.OperationOutcome)
            {
                AddNarrativeToOperationOutcome(fhirResource);
                OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                response.StatusCode = HttpStatusCode.BadRequest;
                // if wish for more granular response codes, need Issue Type
                //OperationOutcome oo = (OperationOutcome)fhirResource;
                //OperationOutcome.IssueType opOutcome = (OperationOutcome.IssueType)oo.Issue[0].Code;
            }

            // convert to stream to remove leading XML elements and json quotes
            rv = SerializeResponse(fhirResource, responseType, SummaryType.False);
            return new System.IO.MemoryStream(UTF8Encoding.Default.GetBytes(rv));

        }

        public Stream TerminzPostOpOnly(string resource, string operation)
        {
            return TerminzPost(resource, string.Empty, operation);
        }

        public Stream TerminzPost(string resource, string id, string operation)
        {

            // get Parameters from the Request Stream

            string reqBody = string.Empty;
            UTF8Encoding enc = new UTF8Encoding();
            Stream reqStream = OperationContext.Current.RequestContext.RequestMessage.GetBody<Stream>();

            using (StreamReader reader = new StreamReader(reqStream, enc) )
            {
                reqBody = reader.ReadToEnd();
            }

            string reqType = WebOperationContext.Current.IncomingRequest.ContentType.ToLower();

            Parameters fParam = new Parameters();

            if (reqType.Contains("json"))
            {
                FhirJsonParser fjp = new FhirJsonParser();
                fParam = fjp.Parse<Parameters>(reqBody);
            }
            else
            {
                FhirXmlParser fxp = new FhirXmlParser();
                fParam = fxp.Parse<Parameters>(reqBody);
            }

            SetQueryParameters(fParam);

            return TerminzGet(resource, id, operation);
        }

        public Stream TerminzDefault()
        {
            return TerminzGetResOnly("metadata");
        }

        public Stream TerminzOptions()
        {
            return TerminzGetResOnly("metadata");
        }

        public Stream TerminzGetResOnly(string resource)
        {
            // prevents re-directs if Conformance Statement requested
            return TerminzGet(resource, string.Empty, string.Empty);
        }

        public Stream TerminzGetIdOnly(string resource, string id)
        {
            return TerminzGet(resource, id, string.Empty);
        }

        public Stream TerminzGet(string resource, string id, string operation)
        {
            string rv = string.Empty;

            string summaryQueryStringValue = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["_summary"];

            string responseType = GetResponseType();
            string fhirVersion = GetFhirVersion();

            // If {id} is empty, {$operation} will be in that URL segment - all terminology operations start with $
            string reqId = id;
            string reqOperation = operation;

            if (!string.IsNullOrEmpty(id) && id.StartsWith("$"))
            {
                reqId = string.Empty;
                reqOperation = id;
            }

            // handle pure operation calls - e.g. $versions
            if (resource.StartsWith("$"))
            {
                reqOperation = resource;
                resource = string.Empty;
            }

            if (this.qParam.Count == 0)  // i.e. hasn't been populated from POST
            {
                qParam = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;
            }

            // remove summary type and format from query parameters as they are only processed at this layer
            qParam.Remove("_format");
            qParam.Remove("_summary");

            ServiceInterface appSi = new ServiceInterface();
            Resource fhirResource = appSi.GetFhirResource(resource, reqId, reqOperation, qParam, fhirVersion);

            SummaryType st = SummaryType.False;

            // honour summary type requests
            if (!string.IsNullOrEmpty(summaryQueryStringValue))
            {
                if (summaryQueryStringValue.Equals("count", System.StringComparison.OrdinalIgnoreCase))
                {
                    st = SummaryType.Count;
                }
                if (summaryQueryStringValue.Equals("data", System.StringComparison.OrdinalIgnoreCase))
                {
                    st = SummaryType.Data;
                }
                if (summaryQueryStringValue.Equals("text", System.StringComparison.OrdinalIgnoreCase))
                {
                    st = SummaryType.Text;
                }
                if (summaryQueryStringValue.Equals("true", System.StringComparison.OrdinalIgnoreCase))
                {
                    st = SummaryType.True;
                }
            }  

            if (fhirResource.ResourceType == ResourceType.OperationOutcome)
            {
                AddNarrativeToOperationOutcome(fhirResource);
                OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                response.StatusCode = HttpStatusCode.BadRequest;
            }

            // convert to stream to remove leading XML elements and json quotes
            rv = SerializeResponse(fhirResource, responseType, st);
            return new System.IO.MemoryStream(UTF8Encoding.UTF8.GetBytes(rv));
        }

        public List<string> SpecificationList()
        {
            ServiceInterface appSi = new ServiceInterface();
            return appSi.GetSpecificationList();
        }

        public string SpecificationTestInstance(string specification)
        {
            ServiceInterface appSi = new ServiceInterface();
            return appSi.GetSpecificationTestInstance(specification);
        }

        public List<string> ValidationTest(Stream requestStream)
        {
            ValidationRequest requestData = new ValidationRequest();
            UTF8Encoding enc = new UTF8Encoding();
            string xmlString = string.Empty;
            List<string> validationResults = new List<string>();
            try
            {
                using (StreamReader reader = new StreamReader(requestStream, enc))
                {
                    xmlString = reader.ReadToEnd();
                }

                XDocument xDoc = XDocument.Parse(xmlString);
                requestData.vendorCode = xDoc.Element("ValidationRequest").Element("vendorCode").Value;
                requestData.applicationName = xDoc.Element("ValidationRequest").Element("applicationName").Value;
                requestData.applicationVersion = xDoc.Element("ValidationRequest").Element("applicationVersion").Value;
                requestData.specification = xDoc.Element("ValidationRequest").Element("specification").Value;
                requestData.testInstance = xDoc.Element("ValidationRequest").Element("testInstance").Value;
                validationResults.Add(xmlString);

                ServiceInterface appSi = new ServiceInterface();
                validationResults = appSi.ValidateSpecificationTestInstance(requestData.vendorCode, requestData.applicationName, requestData.applicationVersion, requestData.specification, requestData.testInstance);
            }
            catch (Exception ex)
            {
                validationResults.Add("Web Service Error: " + ex.Message);
            }

            return validationResults;
        }

        public List<GeneralPractice> PracticeDirectory(Stream requestStream)
        {
            PracticeDirectoryRequest requestData = new PracticeDirectoryRequest();
            UTF8Encoding enc = new UTF8Encoding();
            string xmlString = string.Empty;
            List<GeneralPractice> directoryList = new List<GeneralPractice>();

            try
            {
                using (StreamReader reader = new StreamReader(requestStream, enc))
                {
                    xmlString = reader.ReadToEnd();
                }

                XDocument xDoc = XDocument.Parse(xmlString);
                requestData.vendorCode = xDoc.Element("PracticeDirectoryRequest").Element("vendorCode").Value;
                requestData.practiceName = xDoc.Element("PracticeDirectoryRequest").Element("practiceName").Value;
                requestData.practiceAddress = xDoc.Element("PracticeDirectoryRequest").Element("practiceAddress").Value;
                requestData.phoName = xDoc.Element("PracticeDirectoryRequest").Element("phoName").Value;
                requestData.dhbName = xDoc.Element("PracticeDirectoryRequest").Element("dhbName").Value;
                requestData.edi = xDoc.Element("PracticeDirectoryRequest").Element("edi").Value;
                //directoryList.Add(xmlString);

                ServiceInterface appSi = new ServiceInterface();
                directoryList = appSi.GetPracticeListing(requestData.vendorCode, requestData.practiceName, requestData.practiceAddress, requestData.phoName, requestData.dhbName, requestData.edi);
            }
            catch (Exception ex)
            {
                throw new Exception("Web Service Error: " + ex.Message);
            }

            return directoryList;
        }

        public List<HimDirectory> HimDirectory(string HpiFacilityId)
        {
            List<HimDirectory> directoryList = new List<HimDirectory>();

            try
            {
                if (AuthenticateUser(WebOperationContext.Current.IncomingRequest))
                {
                    if (this.userID != HpiFacilityId)
                    {
                        throw new Exception("Invalid Request Parameter");
                    }
                    ServiceInterface appSi = new ServiceInterface();
                    directoryList = appSi.GetHimDirectory(HpiFacilityId);
                }
                else
                {
                    OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                    response.StatusCode = HttpStatusCode.Unauthorized;
                }                
            }
            catch (Exception ex)
            {
                throw new Exception("Web Service Error: " + ex.Message);
            }

            return directoryList;
        }

        public List<HiMessageFile> HiMessages(string HpiFacilityId)
        {
            List<HiMessageFile> hiMessageList = new List<HiMessageFile>();

            try
            {
                if (AuthenticateUser(WebOperationContext.Current.IncomingRequest))
                {
                    if (this.userID != HpiFacilityId)
                    {
                        throw new Exception("Invalid Request Parameter");
                    }
                    ServiceInterface appSi = new ServiceInterface();
                    hiMessageList = appSi.GetHiMessages(HpiFacilityId);
                }
                else
                {
                    OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                    response.StatusCode = HttpStatusCode.Unauthorized;
                }                     
            }
            catch (Exception ex)
            {
                throw new Exception("Web Service Error: " + ex.Message);
            }

            return hiMessageList;
        }

        public List<HimAppFile> HimAppUpdate(string HimVersion)
        {
            List<HimAppFile> himAppFiles = new List<HimAppFile>();

            try
            {
                if (AuthenticateUser(WebOperationContext.Current.IncomingRequest))
                {
                    ServiceInterface appSi = new ServiceInterface();
                    himAppFiles = appSi.GetHimAppUpdate(HimVersion, this.userID);
                }
                else
                {
                    OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                    response.StatusCode = HttpStatusCode.Unauthorized;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Web Service Error: " + ex.Message);
            }

            return himAppFiles;
        }

        public string FacilityOffLine(string HpiFacilityId)
        {
            string rv = null;

            if (AuthenticateUser(WebOperationContext.Current.IncomingRequest))
            {
                if (this.userID != HpiFacilityId)
                {
                    throw new Exception("Invalid Request Parameter");
                }
                ServiceInterface appSi = new ServiceInterface();
                rv = appSi.PutFacilityOffLine(HpiFacilityId);
            }
            else
            {
                OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                response.StatusCode = HttpStatusCode.Unauthorized;
            }     

            return rv;
        }

        public string HiMessageCollected(string Gp2gpTransferId)
        {
            string rv = null;

            if (AuthenticateUser(WebOperationContext.Current.IncomingRequest))
            {
                ServiceInterface appSi = new ServiceInterface();
                rv = appSi.PutMessageCollectedStatus(Gp2gpTransferId);
            }
            else
            {
                OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                response.StatusCode = HttpStatusCode.Unauthorized;
            }     
            
            return rv;
        }

        public bool HiMessage(Stream requestStream)
        {
            bool messageSaved = false;

            try
            {

                if (AuthenticateUser(WebOperationContext.Current.IncomingRequest))
                {
                    SaveHiMessageRequest requestData = new SaveHiMessageRequest();
                    UTF8Encoding enc = new UTF8Encoding();
                    string xmlString = string.Empty;

                    using (StreamReader reader = new StreamReader(requestStream, enc))
                    {
                        xmlString = reader.ReadToEnd();
                    }

                    XDocument xDoc = XDocument.Parse(xmlString);
                    requestData.fileName = xDoc.Element("SaveHiMessageRequest").Element("fileName").Value;
                    requestData.messageHeader = xDoc.Element("SaveHiMessageRequest").Element("messageHeader").Value;
                    requestData.replyToMessageID = xDoc.Element("SaveHiMessageRequest").Element("replyToMessageID").Value;
                    requestData.encryptedMessage = xDoc.Element("SaveHiMessageRequest").Element("encryptedMessage").Value;

                    ServiceInterface appSi = new ServiceInterface();
                    messageSaved = appSi.PostHpiMessage(requestData.fileName, requestData.messageHeader, requestData.replyToMessageID, requestData.encryptedMessage);
                }
                else
                {
                    OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                    response.StatusCode = HttpStatusCode.Unauthorized;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Web Service Error: " + ex.Message);
            }
             
            return messageSaved;
        }
        
        public bool HimLog(Stream requestStream)
        {
            bool logSaved = false;

            try
            {

                if (AuthenticateUser(WebOperationContext.Current.IncomingRequest))
                {
                    SaveHimLogRequest requestData = new SaveHimLogRequest();
                    UTF8Encoding enc = new UTF8Encoding();
                    string xmlString = string.Empty;

                    using (StreamReader reader = new StreamReader(requestStream, enc))
                    {
                        xmlString = reader.ReadToEnd();
                    }

                    XDocument xDoc = XDocument.Parse(xmlString);
                    requestData.hpiFacilityId = xDoc.Element("SaveHimLogRequest").Element("hpiFacilityId").Value;
                    requestData.himLogData = xDoc.Element("SaveHimLogRequest").Element("himLogData").Value;

                    ServiceInterface appSi = new ServiceInterface();
                    logSaved = appSi.PostHimLogFile(requestData.hpiFacilityId, requestData.himLogData);
                }
                else
                {
                    OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                    response.StatusCode = HttpStatusCode.Unauthorized;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Web Service Error: " + ex.Message);
            }

            return logSaved;
        }

        private bool AuthenticateUser(IncomingWebRequestContext request)
        {
            bool authenticated = false;

            int hkeyNo = 0;

            WebHeaderCollection headers = request.Headers;

            foreach (string hkey in headers.AllKeys)
            {
                if (hkey.ToUpper().StartsWith("FACILITYID"))
                {
                    this.userID = headers.GetValues(hkeyNo)[0];
                }
                else if (hkey.ToUpper().EndsWith("USERKEY"))
                {
                    this.password = headers.GetValues(hkeyNo)[0];
                }
                hkeyNo++;
            }

            if (!string.IsNullOrEmpty(this.password))
            {
                ServiceInterface appSi = new ServiceInterface();
                authenticated = appSi.AuthenticateUser(userID, password);
            }

            return authenticated;
        }

        private string GetResponseType()
        {
            string responseType = "xml";

            string formatQueryStringValue = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["_format"];

            // check query parameters for response type (BUT, does this Format Parameter override the Accept Header?)
            if (!string.IsNullOrEmpty(formatQueryStringValue))
            {
                if (formatQueryStringValue.ToLower().Contains("xml"))
                {
                    responseType = "xml";
                }
                else if (formatQueryStringValue.ToLower().Contains("json"))
                {
                    responseType = "json";
                }
                else
                {
                    throw new WebFaultException<string>(string.Format("Unsupported format '{0}'", formatQueryStringValue), HttpStatusCode.BadRequest);
                }
            }
            else
            {
                // check request Accept Header if no query parameter supplied for response type
                try
                {
                    string acceptHeaderValue = WebOperationContext.Current.IncomingRequest.Accept;
                    if (acceptHeaderValue.ToLower().Contains("json"))
                    {
                        responseType = "json";                       
                    }
                }
                catch { }
            }

            return responseType;
        }

        private string GetFhirVersion()
        {
            string fhirVersion = "r4";
            try
            {
                string ah = WebOperationContext.Current.IncomingRequest.Accept;
                if (ah.ToLower().Contains("fhir-version=r3"))
                {
                    fhirVersion = "r3";
                }
                else if (ah.ToLower().Contains("fhir-version=r2"))
                {
                    fhirVersion = "r2";
                }
            }
            catch { }
            
            return fhirVersion;
        }

        private Resource AddNarrativeToOperationOutcome(Resource fhirResource)
        {
            try
            {
                OperationOutcome opOutcome = (OperationOutcome)fhirResource;
                XNamespace ns = "http://www.w3.org/1999/xhtml";
                var narrative = new XElement(ns + "div", new XElement(ns + "p", opOutcome.Issue[0].DiagnosticsElement));
                opOutcome.Text = new Narrative
                {
                    Status = Narrative.NarrativeStatus.Generated,
                    Div = narrative.ToString()
                };
                fhirResource = opOutcome;
            }
            catch { }

            return fhirResource;
        }

        private void SetQueryParameters(Parameters param)
        {
            this.qParam.Clear();

            string conceptList = string.Empty;
            string conceptSystem = string.Empty;

            foreach (Parameters.ParameterComponent p in param.Parameter)
            {
                if (p.Name == "coding")
                {
                    try
                    {
                        Coding cd = (Coding)p.Value;
                        this.qParam.Add("code", cd.Code);
                        this.qParam.Add("system", cd.System);
                        this.qParam.Add("display", cd.Display);
                    }
                    catch { }
                }
                else if (p.Name == "codeableConcept")
                {
                    try
                    {
                        CodeableConcept cc = (CodeableConcept)p.Value;
                        this.qParam.Add("code", cc.Coding[0].Code);
                        this.qParam.Add("system", cc.Coding[0].System);
                        this.qParam.Add("display", cc.Coding[0].Display);
                    }
                    catch { }
                }
                else if (p.Name == "concept")
                {
                    try
                    {
                        Coding cd = (Coding)p.Value;
                        this.qParam.Add("code", cd.Code);
                        this.qParam.Add("system", cd.System);
                        this.qParam.Add("display", cd.Display);
                    }
                    catch { }
                    try
                    {
                        CodeableConcept cc = (CodeableConcept)p.Value;
                        foreach (Coding cd in cc.Coding)
                        {
                            // <TODO> assume only 1 coding at this stage - but revisit for $closure
                            conceptList = cd.Code;
                            conceptSystem = cd.System;
                        }
                    }
                    catch { }
                }
                else if (p.Name == "codeA" || p.Name == "codeB" || p.Name == "codingA" || p.Name == "codingB")
                {
                    try
                    {
                        Coding cd = (Coding)p.Value;
                        this.qParam.Add(p.Name, cd.Code);
                    }
                    catch { }
                    try
                    {
                        Code cd = (Code)p.Value;
                        this.qParam.Add(p.Name, cd.Value);
                    }
                    catch { }
                }
                else if (p.Name == "property")
                {
                    try
                    {
                        if (p.Part.Count == 2)  // $compose attribute name/value pairs
                        {
                            Coding cd = (Coding)p.Value;
                            this.qParam.Add(p.Part[0].Value.ToString(), p.Part[1].Value.ToString());
                        }
                        else
                        {
                            this.qParam.Add(p.Name, p.Value.ToString());
                        }
                    }
                    catch { }
                }
                else
                {
                    this.qParam.Add(p.Name, p.Value.ToString());
                }
            }

            if (!string.IsNullOrEmpty(conceptList))
            {
                this.qParam.Add("code", conceptList);
                this.qParam.Add("system", conceptSystem);
            }

        }

        private string SerializeResponse(Resource fhirResource, string responseType, SummaryType st)
        {
            string rv = string.Empty;

            OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;

            if (responseType == "json")
            {
                FhirJsonSerializer fjs = new FhirJsonSerializer();
                rv = fjs.SerializeToString(fhirResource, st);
                //rv = FhirSerializer.SerializeResourceToJson(fhirResource,st);
                //context.ContentType = "application/json"; 
                context.ContentType = "application/fhir+json;charset=UTF-8"; // when IANA registered
                context.Format = WebMessageFormat.Json;
            }
            else
            {
                FhirXmlSerializer fxs = new FhirXmlSerializer();
                rv = fxs.SerializeToString(fhirResource, st);
                //rv = FhirSerializer.SerializeResourceToXml(fhirResource,st);
                //context.ContentType = "application/xml";
                context.ContentType = "application/fhir+xml;charset=UTF-8"; // when IANA registered
                context.Format = WebMessageFormat.Xml;
            }

            return rv;
        }

    }

    // CORS - Message Inspector for CORS

    public class CustomHeaderMessageInspector : IDispatchMessageInspector
    {
        Dictionary<string, string> requiredHeaders;
        public CustomHeaderMessageInspector(Dictionary<string, string> headers)
        {
            requiredHeaders = headers ?? new Dictionary<string, string>();
        }

        public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
        {
            return null;
        }

        public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            var httpHeader = reply.Properties["httpResponse"] as HttpResponseMessageProperty;
            foreach (var item in requiredHeaders)
            {
                httpHeader.Headers.Add(item.Key, item.Value);
            }
        }
    }

    // CORS - Create Endpoint Behavior and use Message Inspector to add headers

    public class EnableCrossOriginResourceSharingBehavior : BehaviorExtensionElement, IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {

        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
            var requiredHeaders = new Dictionary<string, string>();

            requiredHeaders.Add("Access-Control-Allow-Origin", "*");
            requiredHeaders.Add("Access-Control-Request-Method", "POST,GET,PUT,DELETE,OPTIONS");
            requiredHeaders.Add("Access-Control-Allow-Headers", "X-Requested-With,Content-Type");

            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new CustomHeaderMessageInspector(requiredHeaders));
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        public override Type BehaviorType
        {
            get { return typeof(EnableCrossOriginResourceSharingBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new EnableCrossOriginResourceSharingBehavior();
        }
    }


}
