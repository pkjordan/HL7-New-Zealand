namespace Vintage.AppServices.BusinessClasses.FHIR
{
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Serialization;
    using Hl7.Fhir.Utility;
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using Vintage.AppServices.BusinessClasses.FHIR.CodeSystems;

    public static class TerminologyNamingSystem
    {
    
        internal const string UNFOUND_NS_IDENTIFIER = "IDENTIFIER NOT FOUND";
        internal const string MISSING_NS_IDENTIFIER = "MISSING NAMING SYSTEM IDENTIFIER";
        internal const string MISSING_NS_TYPE = "MISSING NAMING SYSTEM TYPE";
        internal const string DIGITAL_HEALTH_NZ = "https://standards.digital.health.nz";
        public static Resource GetRequest(string id, string operation, NameValueCollection queryParam)
        {
            Resource fhirResource = null;

            try
            {
                string identifier = GetIdentifier(id, queryParam);

                // check requested operation
                if (string.IsNullOrEmpty(operation))
                {
                    if (string.IsNullOrEmpty(identifier))
                    {
                        fhirResource = GetAllNamingSystems(queryParam);
                    }
                    else
                    {
                        fhirResource = GetRequest(identifier, queryParam);
                    }
                }
                else if (operation == "$preferred-id")
                {
                   fhirResource = PreferredIdOperation(id,queryParam);
                }
                else
                {
                    return OperationOutcome.ForMessage("Unrecognised Operation in Request..." + operation, OperationOutcome.IssueType.Unknown, OperationOutcome.IssueSeverity.Error);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == MISSING_NS_IDENTIFIER)
                {
                    return OperationOutcome.ForMessage("No Naming System id or identifer in Request", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == MISSING_NS_TYPE)
                {
                    return OperationOutcome.ForMessage("No Naming System Type in Request", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == UNFOUND_NS_IDENTIFIER)
                {
                    return OperationOutcome.ForMessage("Naming System Preferred Identifier not found", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else
                {
                    return OperationOutcome.ForMessage(ex.Message, OperationOutcome.IssueType.Unknown, OperationOutcome.IssueSeverity.Error);
                }
            }

            return fhirResource;
        }

        private static Resource GetAllNamingSystems(NameValueCollection queryParam)
        {
            string kind = Utilities.GetQueryValue("kind", queryParam);

            Bundle nsBundle = new Bundle
            {
                Id = Guid.NewGuid().ToString(),
                Type = Bundle.BundleType.Searchset
            };

            nsBundle.Link.Add(new Bundle.LinkComponent { Url = ServerCapability.TERMINZ_CANONICAL + "/NamingSystem", Relation = "self" });

            if (string.IsNullOrEmpty(kind) || kind == "identifier")
            {
                nsBundle.AddResourceEntry(GetRequest("NHI", queryParam), ServerCapability.TERMINZ_CANONICAL + "/NamingSystem/NHI");
                nsBundle.AddResourceEntry(GetRequest("HPI-CPN", queryParam), ServerCapability.TERMINZ_CANONICAL + "/NamingSystem/HPI-CPN");
                nsBundle.AddResourceEntry(GetRequest("HPI-FAC", queryParam), ServerCapability.TERMINZ_CANONICAL + "/NamingSystem/HPI-FAC");
                nsBundle.AddResourceEntry(GetRequest("HPI-ORG", queryParam), ServerCapability.TERMINZ_CANONICAL + "/NamingSystem/HPI-ORG");
            }

            if (string.IsNullOrEmpty(kind) || kind == "codesystem")
            {
                nsBundle.AddResourceEntry(GetRequest("NZMT", queryParam), "/NamingSystem/NZMT");
                nsBundle.AddResourceEntry(GetRequest("NZ_ETHNICITY_LEVEL_1", queryParam), ServerCapability.TERMINZ_CANONICAL + "/NamingSystem/NZ_ETHNICITY_LEVEL_1");
                nsBundle.AddResourceEntry(GetRequest("NZ_ETHNICITY_LEVEL_2", queryParam), ServerCapability.TERMINZ_CANONICAL + "/NamingSystem/NZ_ETHNICITY_LEVEL_2");
                nsBundle.AddResourceEntry(GetRequest("NZ_ETHNICITY_LEVEL_3", queryParam), ServerCapability.TERMINZ_CANONICAL + "/NamingSystem/NZ_ETHNICITY_LEVEL_3");
                nsBundle.AddResourceEntry(GetRequest("NZ_ETHNICITY_LEVEL_4", queryParam), ServerCapability.TERMINZ_CANONICAL + "/NamingSystem/NZ_ETHNICITY_LEVEL_4");
            }

            nsBundle.Total = nsBundle.Entry.Count();

            if (nsBundle.Total == 0)
            {
                return OperationOutcome.ForMessage("No Naming Systems match search parameter values.", OperationOutcome.IssueType.NotFound, OperationOutcome.IssueSeverity.Information);
            }

            return nsBundle;
        }

        private static Resource GetRequest(string identifier, NameValueCollection queryParam)
        {

            if (string.IsNullOrEmpty(identifier))
            {
                return OperationOutcome.ForMessage("No NamingSystem id or identifer in Request", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
            }

            NamingSystem namingSystem = new NamingSystem();
            string resourceFilePath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) + @"\Test Files\";

            FhirXmlParser fxp = new FhirXmlParser();

            if (identifier.ToUpper() == "NHI" || identifier == DIGITAL_HEALTH_NZ + "/id/nhi" || identifier == "2.16.840.1.113883.2.18.2")
            {
                namingSystem = fxp.Parse<NamingSystem>(File.ReadAllText(resourceFilePath + "NamingSystem_NHI.xml"));
            }
            else if (identifier.ToUpper() == "HPI-CPN" || identifier == DIGITAL_HEALTH_NZ + "/id/hpi-person" || identifier == "2.16.840.1.113883.2.18.3.1")
            {
                namingSystem = fxp.Parse<NamingSystem>(File.ReadAllText(resourceFilePath + "NamingSystem_HPI_CPN.xml"));
            }
            else if (identifier.ToUpper() == "HPI-FAC" || identifier == DIGITAL_HEALTH_NZ + "/id/hpi-facility" || identifier == "2.16.840.1.113883.2.18.3.2")
            {
                namingSystem = fxp.Parse<NamingSystem>(File.ReadAllText(resourceFilePath + "NamingSystem_HPI_FAC.xml"));
            }
            else if (identifier.ToUpper() == "HPI-ORG" || identifier == DIGITAL_HEALTH_NZ + "/id/hpi-organisation" || identifier == "2.16.840.1.113883.2.18.3.3")
            {
                namingSystem = fxp.Parse<NamingSystem>(File.ReadAllText(resourceFilePath + "NamingSystem_HPI_ORG.xml"));
            }
            else if (identifier.ToUpper() == "NZMT" || identifier == NzMt.URI || identifier == "2.16.840.1.113883.2.18.21")
            {
                namingSystem = fxp.Parse<NamingSystem>(File.ReadAllText(resourceFilePath + "NamingSystem_NZMT.xml"));
            }
            else if (identifier.ToUpper() == "NZ_ETHNICITY_LEVEL_1" || identifier == NzEthnicityL1.URI || identifier == "2.16.840.1.113883.2.18.11")
            {
                namingSystem = fxp.Parse<NamingSystem>(File.ReadAllText(resourceFilePath + "NamingSystem_NZ_ETHNICITY_LEVEL_1.xml"));
            }
            else if (identifier.ToUpper() == "NZ_ETHNICITY_LEVEL_2" || identifier == NzEthnicityL2.URI || identifier == "2.16.840.1.113883.2.18.11.1")
            {
                namingSystem = fxp.Parse<NamingSystem>(File.ReadAllText(resourceFilePath + "NamingSystem_NZ_ETHNICITY_LEVEL_2.xml"));
            }
            else if (identifier.ToUpper() == "NZ_ETHNICITY_LEVEL_3" || identifier == NzEthnicityL3.URI || identifier == "2.16.840.1.113883.2.18.11.2")
            {
                namingSystem = fxp.Parse<NamingSystem>(File.ReadAllText(resourceFilePath + "NamingSystem_NZ_ETHNICITY_LEVEL_3.xml"));
            }
            else if (identifier.ToUpper() == "NZ_ETHNICITY_LEVEL_4" || identifier == NzEthnicityL4.URI || identifier == "2.16.840.1.113883.2.18.11.5")
            {
                namingSystem = fxp.Parse<NamingSystem>(File.ReadAllText(resourceFilePath + "NamingSystem_NZ_ETHNICITY_LEVEL_4.xml"));
            }
            else
            {
                return OperationOutcome.ForMessage("Naming System Not Recognised.", OperationOutcome.IssueType.Unknown, OperationOutcome.IssueSeverity.Error);
            }
            
            AddNarrative(namingSystem);

            return namingSystem;
        }

        internal static NamingSystem AddNarrative(NamingSystem nameSys)
        {
            // create display text for Organisation Resource
            string textString = string.Empty;

            try
            {
                XNamespace ns = "http://www.w3.org/1999/xhtml";

                var summary = new XElement(ns + "div",
                    new XElement(ns + "h2", nameSys.Name),
                    new XElement(ns + "table",
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Id"),
                        new XElement(ns + "td", nameSys.Id)
                        ),
                        new XElement(ns + "tr",
                         new XElement(ns + "td", "Description"),
                        new XElement(ns + "td", nameSys.Description.ToString())
                        ),
                         new XElement(ns + "tr",
                         new XElement(ns + "td", "Kind"),
                        new XElement(ns + "td", nameSys.Kind.ToString())
                        ),
                        new XElement(ns + "table",
                            new XElement(ns + "tr",
                            new XElement(ns + "th", "Type"),
                            new XElement(ns + "th", "Value")
                            ),
                            from ui in nameSys.UniqueId
                            select new XElement(ns + "tr",
                                new XElement(ns + "td", ui.Type.ToString()),
                                new XElement(ns + "td", ui.Value)
                            )
                        )
                     )
                  );

                textString = summary.ToString();
            }
            catch
            { }

            nameSys.Text = new Narrative
            {
                Status = Narrative.NarrativeStatus.Generated,
                Div = textString
            };

            return nameSys;
        }

        private static Resource PreferredIdOperation(string id, NameValueCollection queryParam)
        {
            string nsType = Utilities.GetQueryValue("type", queryParam);

            if (string.IsNullOrEmpty(id))
            {
                throw new Exception(MISSING_NS_IDENTIFIER);
            }
            else if (string.IsNullOrEmpty(nsType))
            {
                throw new Exception(MISSING_NS_TYPE);
            }

            Parameters param = new Parameters();
            string prefID = string.Empty;

            // get resource
            NamingSystem namingSystem = (NamingSystem) GetRequest(id, queryParam);

            foreach(NamingSystem.UniqueIdComponent idComp in namingSystem.UniqueId)
            {
                if(idComp.Preferred ==  true && idComp.Type.GetLiteral() == nsType)
                {
                    prefID = idComp.Value;
                }
            }

            if (string.IsNullOrEmpty(prefID))
            {
                throw new Exception(UNFOUND_NS_IDENTIFIER);
            }
            else
            {
                param.Add("result", new FhirString(prefID));
            }

            return param;
        }

        private static string GetIdentifier(string id, NameValueCollection queryParam)
        {
            string identifier = id;

            // get parameter values that might uniquely identify the Resource

            if (string.IsNullOrEmpty(id))
            {
                identifier = Utilities.GetQueryValue("_id", queryParam);
            }

            if (string.IsNullOrEmpty(identifier))
            {
                identifier = Utilities.GetQueryValue("identifier", queryParam);
            }

            if (string.IsNullOrEmpty(identifier))
            {
                identifier = Utilities.GetQueryValue("value", queryParam);
            }

            if (string.IsNullOrEmpty(identifier))
            {
                identifier = Utilities.GetQueryValue("url", queryParam);
            }

            return identifier;
        }
    }
}