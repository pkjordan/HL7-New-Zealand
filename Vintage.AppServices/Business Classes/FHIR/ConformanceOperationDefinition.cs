namespace Vintage.AppServices.BusinessClasses.FHIR
{
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Serialization;
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    public static class ConformanceOperationDefinition
    {

        internal const string UNFOUND_OPDEF_IDENTIFIER = "OPERATION DEFINITION NOT FOUND";
        internal const string MISSING_OPDEF_IDENTIFIER = "MISSING OPERATION DEFINITION IDENTIFIER";

        public static Resource GetRequest(string id, NameValueCollection queryParam)
        {
            Resource fhirResource = null;

            try
            {
                string identifier = GetIdentifier(id, queryParam);

                if (string.IsNullOrEmpty(identifier))
                {
                    fhirResource = GetAllOperationDefinitions(queryParam);
                }
                else
                {
                    fhirResource = GetOperationDefinition(identifier, queryParam);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == MISSING_OPDEF_IDENTIFIER)
                {
                    return OperationOutcome.ForMessage("No Operation Definition id or identifer in Request", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == UNFOUND_OPDEF_IDENTIFIER)
                {
                    return OperationOutcome.ForMessage("Operation Definition not found", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else
                {
                    return OperationOutcome.ForMessage(ex.Message, OperationOutcome.IssueType.Unknown, OperationOutcome.IssueSeverity.Error);
                }
            }

            return fhirResource;
        }

        private static Resource GetAllOperationDefinitions(NameValueCollection queryParam)
        {
            Bundle nsBundle = new Bundle
            {
                Id = Guid.NewGuid().ToString(),
                Type = Bundle.BundleType.Searchset
            };

            nsBundle.Link.Add(new Bundle.LinkComponent { Url = ServerCapability.TERMINZ_CANONICAL + "/OperationDefinition", Relation = "self" });
            nsBundle.AddResourceEntry(GetOperationDefinition("CodeSystem-lookup", queryParam), ServerCapability.TERMINZ_CANONICAL + "/OperationDefinition/CodeSystem-lookup");
            nsBundle.AddResourceEntry(GetOperationDefinition("CodeSystem-subsumes", queryParam), ServerCapability.TERMINZ_CANONICAL + "/OperationDefinition/CodeSystem-subsumes");
            nsBundle.AddResourceEntry(GetOperationDefinition("CodeSystem-validate-code", queryParam), ServerCapability.TERMINZ_CANONICAL + "/OperationDefinition/CodeSystem-validate-code");
            nsBundle.AddResourceEntry(GetOperationDefinition("ConceptMap-translate", queryParam), ServerCapability.TERMINZ_CANONICAL + "/OperationDefinition/ConceptMap-translate");
            nsBundle.AddResourceEntry(GetOperationDefinition("ValueSet-expand", queryParam), ServerCapability.TERMINZ_CANONICAL + "/OperationDefinition/ValueSet-expand");
            nsBundle.AddResourceEntry(GetOperationDefinition("ValueSet-validate-code", queryParam), ServerCapability.TERMINZ_CANONICAL + "/OperationDefinition/ValueSet-validate-code");

            nsBundle.Total = nsBundle.Entry.Count();

            if (nsBundle.Total == 0)
            {
                throw new Exception(UNFOUND_OPDEF_IDENTIFIER);
            }

            return nsBundle;
        }

        private static Resource GetOperationDefinition(string identifier, NameValueCollection queryParam)
        {

            if (string.IsNullOrEmpty(identifier))
            {
                throw new Exception(MISSING_OPDEF_IDENTIFIER);
            }

            OperationDefinition opDef = new OperationDefinition();

            string resourceFilePath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) + @"\Test Files\";

            FhirXmlParser fxp = new FhirXmlParser();

            if (identifier.ToUpper() == "CODESYSTEM-LOOKUP" || identifier == ServerCapability.TERMINZ_CANONICAL + "/OperationDefinition/CodeSystem-lookup")
            {
                opDef = fxp.Parse<OperationDefinition>(File.ReadAllText(resourceFilePath + "OperationDefinition-CodeSystem-lookup.xml"));
            }
            else if (identifier.ToUpper() == "CODESYSTEM-SUBSUMES" || identifier == ServerCapability.TERMINZ_CANONICAL + "/OperationDefinition/CodeSystem-subsumes")
            {
                opDef = fxp.Parse<OperationDefinition>(File.ReadAllText(resourceFilePath + "OperationDefinition-CodeSystem-subsumes.xml"));
            }
            else if (identifier.ToUpper() == "CODESYSTEM-VALIDATE-CODE" || identifier == ServerCapability.TERMINZ_CANONICAL + "/OperationDefinition/CodeSystem-validate-code")
            {
                opDef = fxp.Parse<OperationDefinition>(File.ReadAllText(resourceFilePath + "OperationDefinition-CodeSystem-validate-code.xml"));
            }
            else if (identifier.ToUpper() == "CONCEPTMAP-TRANSLATE" || identifier == ServerCapability.TERMINZ_CANONICAL + "/OperationDefinition/ConceptMap-translate")
            {
                opDef = fxp.Parse<OperationDefinition>(File.ReadAllText(resourceFilePath + "OperationDefinition-ConceptMap-translate.xml"));
            }
            else if (identifier.ToUpper() == "VALUESET-EXPAND" || identifier == ServerCapability.TERMINZ_CANONICAL + "/OperationDefinition/ValueSet-expand")
            {
                opDef = fxp.Parse<OperationDefinition>(File.ReadAllText(resourceFilePath + "OperationDefinition-ValueSet-expand.xml"));
            }
            else if (identifier.ToUpper() == "VALUESET-VALIDATE-CODE" || identifier == ServerCapability.TERMINZ_CANONICAL + "/OperationDefinition/ValueSet-validate-code")
            {
                opDef = fxp.Parse<OperationDefinition>(File.ReadAllText(resourceFilePath + "OperationDefinition-ValueSet-validate-code.xml"));
            }
            else
            {
                throw new Exception(UNFOUND_OPDEF_IDENTIFIER);
            }

            AddNarrative(opDef);

            return opDef;
        }

        internal static OperationDefinition AddNarrative(OperationDefinition opDef)
        {
            // create display text for Organisation Resource
            string textString = string.Empty;

            try
            {
                XNamespace ns = "http://www.w3.org/1999/xhtml";

                var summary = new XElement(ns + "div",
                    new XElement(ns + "h2", opDef.Name),
                    new XElement(ns + "table",
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Id"),
                        new XElement(ns + "td", opDef.Id)
                        ),
                        new XElement(ns + "tr",
                         new XElement(ns + "td", "Description"),
                        new XElement(ns + "td", opDef.Description.ToString())
                        ),
                         new XElement(ns + "tr",
                         new XElement(ns + "td", "Kind"),
                        new XElement(ns + "td", opDef.Kind.ToString())
                        ),
                         new XElement(ns + "tr",
                         new XElement(ns + "td", "Resource"),
                        new XElement(ns + "td", opDef.Resource)
                        ),
                        new XElement(ns + "table",
                            new XElement(ns + "tr",
                            new XElement(ns + "th", "Use"),
                            new XElement(ns + "th", "Name"),
                            new XElement(ns + "th", "Cardinality"),
                            new XElement(ns + "th", "Type")
                            ),
                            from param in opDef.Parameter
                            select new XElement(ns + "tr",
                                new XElement(ns + "td", param.Use.ToString()),
                                new XElement(ns + "td", param.Name),
                                new XElement(ns + "td", param.Min.ToString() + ".." + param.Max.ToString()),
                                new XElement(ns + "td", param.Type.ToString())
                            )
                        )
                     )
                  );

                textString = summary.ToString();
            }
            catch
            { }

            opDef.Text = new Narrative
            {
                Status = Narrative.NarrativeStatus.Generated,
                Div = textString
            };

            return opDef;
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