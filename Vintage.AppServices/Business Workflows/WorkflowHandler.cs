namespace Vintage.AppServices.BusinessWorkflows
{
    using Hl7.Fhir.Model;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;
    using Vintage.AppServices.BusinessClasses;
    using Vintage.AppServices.BusinessClasses.FHIR;
    using Vintage.AppServices.DataAccessClasses;
    using Vintage.AppServices.Utilities;

    /// <summary>
    /// Directs Service Interface calls to the relevant Business Classes
    /// </summary>
    /// <remarks>Also contains all exception handling and logging.</remarks>
    internal static class WorkflowHandler
    {

        internal static bool AuthenticateUser(string userID, string password)
        {
            bool authenticated = false;

            try
            {
                if (ValidateRequestParameter(userID,8,false) && ValidateRequestParameter(password,50,true))
                {
                    authenticated = PracticeUser.AuthenticateUser(userID, password);
                }
                if (!authenticated)
                {
                    Log.Write("Authentication Failure for UserID = " + userID + " & Password = " + password, LogLevel.Normal);
                }
            }
            catch (Exception ex)
            {
                Log.Write("ERROR: " + ex.ToString(), LogLevel.ExceptionOnly);
                throw;
            }

            return authenticated;
        }

        internal static Resource GetFhirResource(string resource, string id, string operation, NameValueCollection queryParams, string fhirVersion)
        {
            Resource fhirResource = null;

            try
            {
                if (fhirVersion == "r2")
                {
                    fhirResource = OperationOutcome.ForMessage("FHIR r2/DSTU2 not supported by this server", OperationOutcome.IssueType.BusinessRule);
                }
                else if (fhirVersion == "r3")
                {
                    fhirResource = OperationOutcome.ForMessage("FHIR r3/STU3 not supported by this server", OperationOutcome.IssueType.BusinessRule);
                }
                else if (string.IsNullOrEmpty(resource) && operation == "$versions")
                {
                    fhirResource = ServerCapability.GetVersions();
                }
                else if (resource == "metadata")
                {
                    fhirResource = ServerCapability.GetStatement(queryParams);
                }
                else if (resource == "TerminologyCapabilities")
                {
                    fhirResource = TerminologyCapability.GetStatement(true);
                }
                else if (resource == "OperationDefinition")
                {
                    fhirResource = ConformanceOperationDefinition.GetRequest(id, queryParams);
                }
                else if (resource == "CodeSystem")
                {
                    fhirResource = TerminologyCodeSystem.PerformOperation(id, operation, queryParams);
                }
                else if (resource == "ConceptMap" || operation == "$closure")
                {
                    fhirResource = TerminologyConceptMap.PerformOperation(id, operation, queryParams);
                }
                else if (resource == "ValueSet")
                {
                    fhirResource = TerminologyValueSet.PerformOperation(id, operation, queryParams);
                }
                else if (resource == "NamingSystem")
                {
                    fhirResource = TerminologyNamingSystem.GetRequest(id, operation, queryParams);
                }
                else if (resource == "Location")
                {
                    fhirResource = AdministrationLocation.GetRequest(id, queryParams);
                }
                else if (resource == "Organization")
                {
                    fhirResource = AdministrationOrganisation.GetRequest(id, queryParams);
                }
                else
                {
                    fhirResource = OperationOutcome.ForMessage("No module could be found to handle the request '" + resource + "'", OperationOutcome.IssueType.NotFound);
                }
            }
            catch (Exception ex)
            {
                Log.Write("ERROR: " + ex.ToString(), LogLevel.ExceptionOnly);
                throw;
            }

            return fhirResource;
        }

        internal static List<string> GetSpecificationList()
        {
            List<string> specifications = new List<string>();

            try
            {
                specifications = Specification.GetAvailableSpecificationList();
            }
            catch (Exception ex)
            {
                Log.Write("ERROR: " + ex.ToString(), LogLevel.ExceptionOnly);
                throw;
            }

            return specifications;
        }

        internal static string GetSpecificationTestInstance(string specification)
        {
            string testInstance = string.Empty;

            try
            {

                // determine which specification type has been requested
                SpecificationType specType = Specification.GetSpecificationType(specification);

                if (specType == SpecificationType.Unknown)
                {
                    throw new Exception("unrecognised specification type: " + specification);
                }

                // get the sample
                testInstance = Specification.GetSpecificationInstance(specType);
            }
            catch (Exception ex)
            {
                Log.Write("ERROR: " + ex.ToString(), LogLevel.ExceptionOnly);
                throw;
            }

            return testInstance;
        }

        internal static List<string> ValidateSpecificationTestInstance(string vendorCode, string applicationName, string applicationVersion, string specification, string testData)
        {
            List<string> validationResults = new List<string>();

            try
            {

                StringBuilder reportTransport = new StringBuilder();
                StringBuilder reportFormat = new StringBuilder();
                StringBuilder reportData = new StringBuilder();

                bool passedTransport = false;
                bool passedFormat = false;
                bool passedData = false;

                // check vendor remaining parameters
                if (string.IsNullOrEmpty(vendorCode) || string.IsNullOrEmpty(applicationName) || string.IsNullOrEmpty(applicationVersion))
                {
                    throw new Exception("Incomplete vendor and/or application details supplied");
                }

                // check that test instance supplied
                if (string.IsNullOrEmpty(testData))
                {
                    throw new Exception("No Test Data supplied");
                }

                // determine which specification type has been requested
                SpecificationType specType = Specification.GetSpecificationType(specification);

                if (specType == SpecificationType.Unknown)
                {
                    throw new Exception("Unrecognised Specification Type: " + specification);
                }

                // validate the passed Test Data
                validationResults.AddRange(Specification.Validate(specType, testData));

                // iterate through Validation Report List and populate Report Type properties

                string reportLineType = "T";

                foreach (string reportLine in validationResults)
                {
                    if (reportLine.StartsWith("TRANSPORT :"))
                    {
                        reportLineType = "T";
                        passedTransport = reportLine.StartsWith("TRANSPORT : Pass");
                    }
                    else if (reportLine.StartsWith("FORMAT :"))
                    {
                        reportLineType = "F";
                        passedFormat = reportLine.StartsWith("FORMAT : Pass");
                    }
                    else if (reportLine.StartsWith("DATA :"))
                    {
                        reportLineType = "D";
                        passedData = reportLine.StartsWith("DATA : Pass");
                    }

                    if (reportLineType == "T")
                    {
                        reportTransport.AppendLine(reportLine);
                    }
                    else if (reportLineType == "F")
                    {
                        reportFormat.AppendLine(reportLine);
                    }
                    else if (reportLineType == "D")
                    {
                        reportData.AppendLine(reportLine);
                    }

                }

                // Store Validation Result in Patients First DB
                ValidationResult.AddValidationResult(
                    applicationName,
                    applicationVersion,
                    specification,
                    passedTransport,
                    passedFormat,
                    passedData,
                    reportTransport.ToString(),
                    reportFormat.ToString(),
                    reportData.ToString(),
                    vendorCode
                    );

            }
            catch (Exception ex)
            {
                Log.Write("ERROR: " + ex.ToString(), LogLevel.ExceptionOnly);
                throw;
            }

            return validationResults;
        }

        internal static List<GeneralPractice> GetPracticeListing(string vendorCode, string practiceName, string practiceAddress, string phoName, string dhbName, string ediAddress)
        {
            List<GeneralPractice> directory = new List<GeneralPractice>();

            try
            {
                directory = PracticeSearch.GetPractices(vendorCode, practiceName, practiceAddress, phoName, dhbName, ediAddress);
            }
            catch (Exception ex)
            {
                Log.Write("ERROR: " + ex.ToString(), LogLevel.ExceptionOnly);
                throw;
            }

            return directory;
        }

        internal static List<HimDirectory> GetHimDirectory(string facilityID)
        {
            List<HimDirectory> directory = new List<HimDirectory>();

            try
            {

                if (ValidateRequestParameter(facilityID, 8, false))
                {

                    // update On-Line status of passed Facility to true
                    HimOnLine.UpdateOnLineStatus(facilityID, true);

                    // get list of all Facilities using HPI Messaging 
                    directory = HimSearch.GetFacilities();
                }
            }
            catch (Exception ex)
            {
                Log.Write("ERROR: " + ex.ToString(), LogLevel.ExceptionOnly);
                throw;
            }

            return directory;
        }

        internal static string PutFacilityOffLine(string facilityID)
        {
            List<HimDirectory> directory = new List<HimDirectory>();

            try
            {
                if (ValidateRequestParameter(facilityID, 8, false))
                {
                    // update On-Line status of passed Facility to false
                    HimOnLine.UpdateOnLineStatus(facilityID, false);
                }
            }
            catch (Exception ex)
            {
                Log.Write("ERROR: " + ex.ToString(), LogLevel.ExceptionOnly);
                throw;
            }

            return "OffLine";
        }

        internal static bool PostHpiMessage(string fileName, string messageHeader, string replyToMessageID, string encryptedMessage)
        {
            try
            {
                // instantiate message file object and populated properties from header
                HiMessageFile mf = new HiMessageFile(messageHeader);
                mf.replyToMessageId = replyToMessageID;

                // message will be empty if this is a notification (hashed PID will be in replyToMessageID)
                if (!string.IsNullOrEmpty(encryptedMessage))
                {
                    // save file to disk and populate message file name and size properties
                    mf.messageFileName = mf.SaveMessageToDisk(fileName, encryptedMessage);
                    mf.fileSize = mf.GetFileSize(mf.messageFileName);
                }

                // record in Patients First DB - Gp2GpTransfer table
                Gp2GpTransfer.AddGp2GpTransfer(mf);
            }
            catch (Exception ex)
            {
                Log.Write("ERROR: " + ex.ToString(), LogLevel.ExceptionOnly);
                throw;
            }

            return true;
        }

        // maybe call one at a time - because of potential size issues?? (where to throttle this - client param?)
        internal static List<HiMessageFile> GetHiMessages(string facilityID)
        {
            List<HiMessageFile> hiMessageList = new List<HiMessageFile>();

            try
            {
                if (ValidateRequestParameter(facilityID, 8, false))
                {
                    // Get a list of all uncollected message files for the passed facility
                    hiMessageList = Gp2GpTransfer.GetUncollectedMessages(facilityID);

                    // iterate through list adding the file to each object
                    foreach (HiMessageFile mf in hiMessageList)
                    {
                        // add contents (don't delete file at this stage)
                        mf.fileText = mf.GetMessageFromDisk(mf.messageFileName);
                        // use Client to send ok message on receipt and then update DB and delete file
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("ERROR: " + ex.ToString(), LogLevel.ExceptionOnly);
                throw;
            }

            return hiMessageList;
        }

        internal static List<HimAppFile> GetHimAppUpdate(string himVersion, string facilityID)
        {
            List<HimAppFile> himAppFiles = new List<HimAppFile>();

            try
            {
                if (ValidateRequestParameter(facilityID, 8, false) && ValidateRequestParameter(himVersion, 8, false))
                {
                    // <TODO> when clients all updated - get file version from HimServiceConsole.exe?
                    HimAppFile appFile1 = new HimAppFile();
                    appFile1.fileName = "HIM.exe";
                    appFile1.fileVersion = himVersion;

                    // get version of HIM.exe in updates folder (if none there, swallow exception)
                    try
                    {
                        appFile1.GetFileVersion();
                    }
                    catch { }

                    // record current version
                    HimOnLine.UpdateClientVersion(facilityID, himVersion);

                    // populate return List if the server contains a different version (not just a newer one, in case need to revert back)
                    if (appFile1.fileVersion != himVersion)
                    {

                        // backwards-compatibility - check if Console App exists, if so, just send this, otherwise HIM.EXE and HIM.pdb

                        HimAppFile appFile0 = new HimAppFile();
                        appFile0.fileName = "HimServiceConsole.exe";

                        if (appFile0.GetFileTextFromDisk())
                        {
                            himAppFiles.Add(appFile0);
                        }
                        else
                        {

                            if (appFile1.GetFileTextFromDisk())
                            {
                                himAppFiles.Add(appFile1);
                            }

                            HimAppFile appFile2 = new HimAppFile();
                            appFile2.fileName = "HIM.pdb";
                            if (appFile2.GetFileTextFromDisk())
                            {
                                himAppFiles.Add(appFile2);
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("ERROR: " + ex.ToString(), LogLevel.ExceptionOnly);
                throw;
            }

            return himAppFiles;
        }

        internal static string PutMessageCollectedStatus(string gp2gpTransferID)
        {
            try
            {
                int dbKey = int.Parse(gp2gpTransferID);

                // get message file name
                HiMessageFile mf = new HiMessageFile();
                mf.messageFileName = Gp2GpTransfer.GetMessageFileName(dbKey);

                if (string.IsNullOrEmpty(mf.messageFileName))
                {
                    Log.Write("ERROR: unable to identify Message File for ID = " + dbKey.ToString(), LogLevel.ExceptionOnly);
                }
                else
                {
                    // delete message file from disk (if this can't be done, it's still been received - will to be reviewed by SysOp)
                    if (!mf.DeleteMessageFromDisk(mf.messageFileName))
                    {
                        Log.Write("ERROR: unable to delete collected file: " + mf.messageFileName, LogLevel.ExceptionOnly);
                    }

                    // update Collected Flag in DB (even if it can't be deleted - as it has been collected)
                    Gp2GpTransfer.UpdateMessageCollectedStatus(dbKey);
                }
            }
            catch (Exception ex)
            {
                Log.Write("ERROR: " + ex.ToString(), LogLevel.ExceptionOnly);
                throw;
            }

            return "OK";
        }

        internal static bool PostHimLogFile(string facilityID, string himLogData)
        {
            try
            {
                if (ValidateRequestParameter(facilityID, 8, false))
                {
                    // save file to disk
                    HimLogFile.SaveFileToDisk(facilityID, himLogData);
                }
            }
            catch (Exception ex)
            {
                Log.Write("ERROR: " + ex.ToString(), LogLevel.ExceptionOnly);
                throw;
            }

            return true;
        }

        private static bool ValidateRequestParameter(string wsParam, int maxLength, bool alphaNumericOnly)
        {

            if (string.IsNullOrEmpty(wsParam))
            {
                throw new Exception("Missing Request Parameter Value");
            }

            if (wsParam.Length > maxLength || wsParam.Any(Char.IsWhiteSpace))
            {
                throw new Exception("Invalid Request Parameter Value supplied");
            }

            if (alphaNumericOnly && !wsParam.All(char.IsLetterOrDigit))
            {
                throw new Exception("Invalid Request Parameter Value supplied");
            }

            return true;
        }

    }
}
