namespace Vintage.AppServices.BusinessClasses.FHIR
{
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using Vintage.AppServices.BusinessClasses.FHIR.CodeSystems;
    using Vintage.AppServices.BusinessClasses.FHIR.ConceptMaps;
    using Vintage.AppServices.DataAccessClasses;

    public static class TerminologyConceptMap
    {

        internal const string INVALID_CLOSURE_NAME = "INVALID CLOSURE TABLE NAME";
        internal const string INVALID_CLOSURE_PARAMS = "INVALID CLOSURE REQUEST";
        internal const string MISSING_CLOSURE_NAME = "MISSING CLOSURE TABLE NAME";
        internal const string MISSING_CONCEPTMAP_IDENTIFIER = "MISSING CONCEPT MAP IDENTIFIER";
        internal const string MISSING_SOURCE_CODE_CODING = "MISSING SOURCE CODE";
        internal const string MISSING_SOURCE_SYSTEM = "MISSING SOURCE SYSTEM";
        internal const string MISSING_TARGET_SYSTEM = "MISSING TARGET SYSTEM";
        internal const string REINITIALISE_CLOSURE = "REINITIALISE CLOSURE";
        internal const string UNRECOGNISED_CONCEPTMAP = "UNRECOGNISED CONCEPT MAP";
        internal const string UNRECOGNISED_OPERATION = "UNRECOGNISED OPERATION";
        internal const string UNSUPPORTED_VERSION = "UNSUPPORTED VERSION";
        internal const string UNSUPPORTED_CODE_SYSTEM = "UNSUPPORTED CODE SYSTEM";
        internal const string CLOSURE_CODE_SYSTEM_ID = @"http://snomed.info/sct";
        internal const string CLOSURE_CODE_SYSTEM_VERSION = "20170131";

        public static Resource PerformOperation(string id, string operation, NameValueCollection queryParam)
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
                        fhirResource = GetConceptMaps(identifier, queryParam);
                    }
                    else
                    {
                        fhirResource = GetRequest(identifier, queryParam);
                    }
                }
                else if (operation == "$translate")
                {
                    fhirResource = TranslateOperation(id, queryParam);
                }
                else if (operation == "$closure")
                {
                    //return OperationOutcome.ForMessage("Closure operation not yet supported.", OperationOutcome.IssueType.NotSupported);
                    fhirResource = ClosureOperation(queryParam);
                }
                else
                {
                    return OperationOutcome.ForMessage("Unrecognised Operation in Request..." + operation, OperationOutcome.IssueType.Unknown, OperationOutcome.IssueSeverity.Error);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == INVALID_CLOSURE_NAME)
                {
                    return OperationOutcome.ForMessage("Invalid Closure Name", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == INVALID_CLOSURE_PARAMS)
                {
                    return OperationOutcome.ForMessage("Invalid Closure Parameters - cannot pass both concepts and a version", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == MISSING_CLOSURE_NAME)
                {
                    return OperationOutcome.ForMessage("No Closure Name in Request", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == REINITIALISE_CLOSURE)
                {
                    return OperationOutcome.ForMessage("Closure must be reinitialised", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == MISSING_CONCEPTMAP_IDENTIFIER)
                {
                    return OperationOutcome.ForMessage("No ConceptMap id or identifer in Request", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == MISSING_SOURCE_CODE_CODING)
                {
                    return OperationOutcome.ForMessage("No source code for mapping in Request", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == MISSING_SOURCE_SYSTEM)
                {
                    return OperationOutcome.ForMessage("No source system for code in mapping in Request", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == MISSING_TARGET_SYSTEM)
                {
                    return OperationOutcome.ForMessage("No target system for mapping in Request", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == UNRECOGNISED_CONCEPTMAP)
                {
                    return OperationOutcome.ForMessage("Concept Map Not Recognised.", OperationOutcome.IssueType.Unknown, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == UNSUPPORTED_VERSION)
                {
                    return OperationOutcome.ForMessage("Concept Map Version Not Supported.", OperationOutcome.IssueType.NotSupported, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == UNSUPPORTED_CODE_SYSTEM)
                {
                    return OperationOutcome.ForMessage("Code System Not Supported.", OperationOutcome.IssueType.NotSupported, OperationOutcome.IssueSeverity.Error);
                }
                else
                {
                    return OperationOutcome.ForMessage(ex.Message, OperationOutcome.IssueType.Unknown, OperationOutcome.IssueSeverity.Error);
                }
                throw;
            }

            return fhirResource;
        }

        private static Resource GetConceptMaps(string identifier, NameValueCollection queryParam)
        {
            Bundle cmBundle = new Bundle();

            cmBundle.Id = Guid.NewGuid().ToString();
            cmBundle.Type = Bundle.BundleType.Searchset;
            cmBundle.Link.Add(new Bundle.LinkComponent { Url = ServerCapability.TERMINZ_CANONICAL + "/ConceptMap", Relation = "self" });

            if (string.IsNullOrEmpty(identifier) && queryParam.Count < 1)
            {
                cmBundle.AddResourceEntry(GetRequest("NZREAD_SCT", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ConceptMap/NzRead_Sct");
                cmBundle.AddResourceEntry(GetRequest("SCT_NZREAD", queryParam), ServerCapability.TERMINZ_CANONICAL + "ConceptMap/Sct_NzRead");
                cmBundle.AddResourceEntry(GetRequest("NZMP_SCT", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ConceptMap/NzMp_Sct");
                cmBundle.AddResourceEntry(GetRequest("NZ_ETHNICITY", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ConceptMap/NzEthnicity");
                cmBundle.AddResourceEntry(GetRequest("DELPHIC_LOINC", queryParam), "http://sysmex.co.nz/LIS/Observation/Mapping");
                cmBundle.AddResourceEntry(GetRequest("101", queryParam), "http://hl7.org/fhir/ConceptMap/101");
                cmBundle.AddResourceEntry(GetRequest("102", queryParam), "http://hl7.org/fhir/ConceptMap/102");
            }
            else
            {
                ConceptMap cm = (ConceptMap)GetRequest(identifier, queryParam);
                cmBundle.AddResourceEntry(cm, cm.Url);
            }

            cmBundle.Total = cmBundle.Entry.Count();

            return cmBundle;
        }

        private static Resource GetRequest(string identifier, NameValueCollection queryParam)
        {

            //if (string.IsNullOrEmpty(identifier))
            //{
            //    throw new Exception(MISSING_CONCEPTMAP_IDENTIFIER);
            //}

            string url = Utilities.GetQueryValue("url", queryParam);

            ConceptMap conceptMap = new ConceptMap();

            string sourceSystem = Utilities.GetQueryValue("source", queryParam);
            string targetSystem = Utilities.GetQueryValue("target", queryParam);
            string versionVal = Utilities.GetQueryValue("version", queryParam);

            conceptMap = GetConceptMap(identifier, string.Empty, sourceSystem, targetSystem, versionVal);

            AddNarrative(conceptMap);

            return conceptMap;
        }

        private static Resource TranslateOperation(string id, NameValueCollection queryParam)
        {

            string systemVal = Utilities.GetQueryValue("system", queryParam);
            string sourceSystemVal = Utilities.GetQueryValue("source", queryParam);
            string sourceCodeVal = Utilities.GetQueryValue("code", queryParam);
            //string codingVal = Utilities.GetQueryValue("coding", queryParam);
            string versionVal = Utilities.GetQueryValue("conceptMapVersion", queryParam);
            string targetVal = Utilities.GetQueryValue("target", queryParam);  // ValueSet
            string targetSystemVal = Utilities.GetQueryValue("targetSystem", queryParam);  // Code System

            if (string.IsNullOrEmpty(sourceSystemVal))
            {
                sourceSystemVal = systemVal;
            }

            if (string.IsNullOrEmpty(targetSystemVal))
            {
                targetSystemVal = targetVal;
            }

            // if id supplied -  just require source code to be translated, otherwise both source and target code systems must also be supplied
            if (string.IsNullOrEmpty(sourceCodeVal))
            {
                throw new Exception(MISSING_SOURCE_CODE_CODING);
            }
            else if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(sourceSystemVal))
            {
                throw new Exception(MISSING_SOURCE_SYSTEM);
            }
            else if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(targetSystemVal))
            {
                throw new Exception(MISSING_TARGET_SYSTEM);
            }

            // get Mapping(s)
            ConceptMap conceptMap = GetConceptMap(id, sourceCodeVal, sourceSystemVal, targetSystemVal, versionVal);

            // iterate through Mappings storing any results

            List<Tuple<string, Base>> tuples = new List<Tuple<string, Base>>();
            string matchMessage = string.Empty;
            Dictionary<string, string> targetCodes = new Dictionary<string, string>();

            foreach (ConceptMap.GroupComponent cmGroup in conceptMap.Group)
            {
                foreach (ConceptMap.SourceElementComponent se in cmGroup.Element)
                {
                    if (se.Code.Trim() == sourceCodeVal.Trim())
                    {
                        foreach (ConceptMap.TargetElementComponent te in se.Target)
                        {
                            if (!string.IsNullOrEmpty(se.Display))
                            {
                                matchMessage += string.IsNullOrEmpty(matchMessage) ? "Source Code Description(s): " : " | ";
                                matchMessage += se.Display;
                            }
                            // prevent duplicate targets (e.g. Read to SCT maps with >1 description mapped per code)
                            if (!targetCodes.ContainsKey(te.Code.Trim()))
                            {
                                tuples.Add(new Tuple<string, Base>("equivalence", new FhirString(te.Equivalence.ToString())));
                                tuples.Add(new Tuple<string, Base>("concept", new Coding { Code = te.Code.Trim(), Display = te.Display, System = cmGroup.Target }));
                                targetCodes.Add(te.Code.Trim(), te.Display);
                            }
                            foreach (ConceptMap.OtherElementComponent prod in te.Product)
                            {
                                tuples.Add(new Tuple<string, Base>("product", new Coding { Code = prod.Value.Trim(), Display = prod.Display, System = prod.System, ElementId = prod.Property  }));
                            }
                        }
                    }
                }
            }

            Parameters param = new Parameters();
            param.Add("result", new FhirBoolean(tuples.Count > 0));

            if (tuples.Count < 1 )
            {
                throw new Exception("No mappings could be found for " + sourceCodeVal + " / " + systemVal);
                //param.Add("message", new FhirString("No mappings could be found for " + sourceCodeVal + " / " + systemVal));
            }
            else
            {
                if (!string.IsNullOrEmpty(matchMessage))
                {
                    param.Add("message", new FhirString(matchMessage));
                }
                param.Add("match", tuples);
            }

            return param;
        }

        private static ConceptMap GetConceptMap(string identifier, string sourceCode, string sourceSystem, string targetSystem, string mapVersion)
        {

            string resourceFilePath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) + @"\Test Files\";

            // <TODO> need a factory method here
            ConceptMap conceptMap = new ConceptMap();

            if (identifier == "101" || identifier == "http://hl7.org/fhir/ConceptMap/101" || (sourceSystem == "http://hl7.org/fhir/ValueSet/address-use" && targetSystem == "http://terminology.hl7.org/ValueSet/v3-AddressUse"))
            {
                AddressUseV3Fhir cm = new AddressUseV3Fhir(mapVersion);
                conceptMap = cm.conceptMap;
            }
            else if (identifier == "102" || identifier == "http://hl7.org/fhir/ConceptMap/102" || (sourceSystem == "http://hl7.org/fhir/v2/0487" && targetSystem.StartsWith(FhirSnomed.URI)))
            {
                FhirXmlParser fxp = new FhirXmlParser();
                conceptMap = fxp.Parse<ConceptMap>(File.ReadAllText(resourceFilePath + "SpecimenType_v2_SCT_Map.xml"));
            }
            else if (identifier == "NZREAD_SCT" || identifier == "http://itp.patientsfirst.org.nz/ConceptMap/NzRead_Sct" || (sourceSystem.StartsWith("http://health.govt.nz/read-codes") && targetSystem.StartsWith(FhirSnomed.URI)))
            {
                NZReadToSCT cm = new NZReadToSCT(mapVersion, sourceCode);
                conceptMap = cm.conceptMap;
            }
            else if (identifier == "SCT_NZREAD" || identifier == "http://itp.patientsfirst.org.nz/ConceptMap/Sct_NzRead" || (sourceSystem.StartsWith(FhirSnomed.URI) && targetSystem.StartsWith("http://health.govt.nz/read-codes")))
            {
                SctToNZRead cm = new SctToNZRead(mapVersion, sourceCode);
                conceptMap = cm.conceptMap;
            }
            else if (identifier == "NZMP_SCT" || identifier == "http://itp.patientsfirst.org.nz/ConceptMap/NzMp_Sct" || (sourceSystem.StartsWith(NzMt.URI) && targetSystem.StartsWith(FhirSnomed.URI)))
            {
                NzMpToSCT cm = new NzMpToSCT(mapVersion, sourceCode);
                conceptMap = cm.conceptMap;
            }
            else if (identifier == "NZ_ETHNICITY" || identifier == "http://itp.patientsfirst.org.nz/ConceptMap/NzEthnicity" || (sourceSystem.StartsWith("https://standards.digital.health.nz/codesystem/ethnic-group-level-") && targetSystem.StartsWith("https://standards.digital.health.nz/codesystem/ethnic-group-level-")))
            {
                sourceSystem = string.IsNullOrEmpty(sourceSystem) ? "https://standards.digital.health.nz/codesystem/ethnic-group-level-" : sourceSystem;
                targetSystem = string.IsNullOrEmpty(targetSystem) ? "https://standards.digital.health.nz/codesystem/ethnic-group-level-" : targetSystem;

                // as this facilitates a generic map to/from levels 2-4, prevent use of an invalid ethnicity level
                string invalidLevels = "0156789";
                string sourceLevel = sourceSystem.Substring(sourceSystem.Length - 1, 1);
                string targetLevel = targetSystem.Substring(targetSystem.Length - 1, 1);

                if (invalidLevels.Contains(sourceLevel) && invalidLevels.Contains(targetLevel))
                {
                    throw new Exception(UNSUPPORTED_CODE_SYSTEM);
                }

                NZEthnicityLevels cm = new NZEthnicityLevels(mapVersion, sourceCode, sourceSystem, targetSystem);
                conceptMap = cm.conceptMap;
            }
            else if (identifier == "DELPHIC_LOINC" || identifier == "http://sysmex.co.nz/LIS/Observation/Mapping" || identifier == "f9f7e8f3-4d81-4291-8534-538fecd6f554")
            {
                FhirXmlParser fxp = new FhirXmlParser();
                conceptMap = fxp.Parse<ConceptMap>(File.ReadAllText(resourceFilePath + "DELPHIC_LOINC_Map.xml"));
            }
            //else if (identifier == "NZMP_SCT" || (sourceSystem == "http://nzmt.org.nz" && targetSystem == "http://snomed.info/sct"))
            //{
            //    NZMpToSCT cm = new NZMpToSCT(mapVersion, sourceCode);
            //    conceptMap = cm.conceptMap;
            //}
            else
            {
                throw new Exception(UNRECOGNISED_CONCEPTMAP);
            }

            return conceptMap;
        }

        private static Resource ClosureOperation(NameValueCollection queryParam)
        {
            bool outcome = true;
            int spReturn = 0;
            bool resync = false;

            string nameVal = Utilities.GetQueryValue("name", queryParam);
            string versionVal = Utilities.GetQueryValue("version", queryParam);
            string codeVals = Utilities.GetQueryValue("code", queryParam);
            string systemVal = Utilities.GetQueryValue("system", queryParam);

            // need to determine task (initialise /add concept / version check / resync) from what is passed
            // nb client can pass a version or concept(s), but not both
            if (string.IsNullOrEmpty(nameVal))
            {
                throw new Exception(MISSING_CLOSURE_NAME);
            }

            // nb if versionVal = 0 will re-synch entire table (BUT if >0 changes since certain version OR specific version?)
            resync = !string.IsNullOrEmpty(versionVal);

            if (resync && !string.IsNullOrEmpty(codeVals))
            {
                throw new Exception(INVALID_CLOSURE_PARAMS);
            }

            if (!string.IsNullOrEmpty(codeVals) || resync)
            {
                if (systemVal != FhirSnomed.URI && !resync)
                {
                    throw new Exception(UNSUPPORTED_CODE_SYSTEM);
                }

                string newConcepts = string.Empty;
                short dbVer = 0;
                string  storedConcepts = Closure.GetClosureConcepts(nameVal, (short)spReturn, out dbVer);
                bool codesToAdd = false;

                foreach (string cd in codeVals.Split(';'))
                {
                    if (!string.IsNullOrEmpty(cd))
                    {
                        string newCode = cd.Trim() + "|";
                        if (storedConcepts.IndexOf(newCode)< 0)
                        {
                            storedConcepts += newCode;
                            codesToAdd = true;
                        }            
                    }
                }

                if (codesToAdd)
                {
                    spReturn = Closure.UpdateClosureTable(nameVal, storedConcepts, CLOSURE_CODE_SYSTEM_ID, CLOSURE_CODE_SYSTEM_VERSION);
                }
                
                if (spReturn == -1)
                {
                    throw new Exception(INVALID_CLOSURE_NAME);
                }
                else if (spReturn == 99)
                {
                    throw new Exception(REINITIALISE_CLOSURE);
                }

                ConceptMap cMap = new ConceptMap();
                cMap.Id = Guid.NewGuid().ToString();
                cMap.Version = dbVer.ToString();
                cMap.Name = "Updates for Closure Table " + nameVal;
                cMap.Status = PublicationStatus.Active;
                cMap.Experimental = true;
                cMap.Date = Hl7.Fhir.Model.Date.Today().Value;

                ConceptMap.GroupComponent gc = new ConceptMap.GroupComponent();
                gc.Source = CLOSURE_CODE_SYSTEM_ID;
                gc.SourceVersion = CLOSURE_CODE_SYSTEM_VERSION;
                gc.Target = CLOSURE_CODE_SYSTEM_ID;
                gc.TargetVersion = CLOSURE_CODE_SYSTEM_VERSION;

                if (resync)
                {
                    codeVals = storedConcepts.Replace("|", ";");
                }

                string subSuper = string.Empty;

                // look for sub-type / super-type relationships between existing codes and new codes
                foreach (string newCode in codeVals.Split(';'))
                {
                    foreach (string storedCode in storedConcepts.Split('|'))
                    {
                        if (!string.IsNullOrEmpty(newCode) && newCode != storedCode)
                        { 
                            // does one of these codes subsume another?
                            string relat = TerminologyCodeSystem.GetRelationship(storedCode, newCode);
                            string supertype = string.Empty;
                            string subType = string.Empty;

                            if (relat == TerminologyCodeSystem.CODE_RELATIONSHIP_SUBSUMED_BY)
                            {
                                subType = storedCode;
                                supertype = newCode;
                            }
                            else if (relat == TerminologyCodeSystem.CODE_RELATIONSHIP_SUBSUMES)
                            {
                                subType = newCode;
                                supertype = storedCode;
                            }

                            // check if this combination used already (reSync)
                            string combo = subType + "+" + supertype + "|";
                            bool newPair = false;

                            if (!string.IsNullOrEmpty(supertype))
                            {
                                if (subSuper.IndexOf(combo) < 0)
                                {
                                    subSuper += combo;
                                    newPair = true;
                                }
                            }

                            if (newPair)
                            {
                                ConceptMap.SourceElementComponent sec = new ConceptMap.SourceElementComponent { Code = subType};
                                ConceptMap.TargetElementComponent tec = new ConceptMap.TargetElementComponent { Code = supertype, Equivalence = ConceptMap.ConceptMapEquivalence.Subsumes };
                                sec.Target.Add(tec);
                                gc.Element.Add(sec);
                            }     
                        }
                    }
                }

                cMap.Group.Add(gc);

                return cMap;

            }
            else
            {
                // initialise Closure Table
                outcome = Closure.AddClosureTable(nameVal, CLOSURE_CODE_SYSTEM_ID, CLOSURE_CODE_SYSTEM_VERSION);
                if (outcome == false)
                {
                    throw new Exception(INVALID_CLOSURE_NAME);
                }
            }

            Parameters param = new Parameters();
            param.Add("outcome", new FhirBoolean(outcome));

            return param;
        }

        internal static ConceptMap AddNarrative(ConceptMap conceptMap)
        {
            // create display text for ConceptMap Resource
            string textString = string.Empty;
            //string nameTitle = conceptMap.Name.ToString() + (!string.IsNullOrEmpty(conceptMap.Title) ? " | " + conceptMap.Title : "");
            try
            {

                XNamespace ns = "http://www.w3.org/1999/xhtml";

                var summary = new XElement(ns + "div",
                    new XElement(ns + "h2", conceptMap.Title),
                    new XElement(ns + "p", conceptMap.Description),
                    new XElement(ns + "table",
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Id"),
                        new XElement(ns + "td", conceptMap.Id)
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Url"),
                        new XElement(ns + "td", conceptMap.Url)
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Version"),
                        new XElement(ns + "td", conceptMap.Version)
                        ),
                         new XElement(ns + "tr",
                        new XElement(ns + "td", "Publisher"),
                        new XElement(ns + "td", conceptMap.Publisher)
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Status"),
                        new XElement(ns + "td", conceptMap.Status.ToString())
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Status Date"),
                        new XElement(ns + "td", conceptMap.Date)
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Experimental"),
                        new XElement(ns + "td", conceptMap.Experimental.ToString())
                        )
                     ),
                    renderConceptMapList(conceptMap, ns)
                  );

                textString = summary.ToString();
            }
            catch
            {
                throw new Exception(UNRECOGNISED_CONCEPTMAP);
            }

            conceptMap.Text = new Narrative();
            conceptMap.Text.Status = Narrative.NarrativeStatus.Generated;
            conceptMap.Text.Div = textString;

            // format Name property correctly
            conceptMap.Name = conceptMap.Name.First().ToString().ToUpper() + conceptMap.Name.Substring(1).Replace('-', '_');

            return conceptMap;
        }

        private static XElement renderConceptMapList(ConceptMap conceptMap, XNamespace ns)
        {
            XElement xConceptMapList = null;

            if (conceptMap.Group.Count > 0)
            {
                xConceptMapList = new XElement(ns + "table",
                        new XElement(ns + "tr",
                        new XElement(ns + "th", "Source Code"),
                        new XElement(ns + "th", "Source Display"),
                        new XElement(ns + "th", "Source Code System"),
                        new XElement(ns + "th", "Equivalence"),
                        new XElement(ns + "th", "Target Code"),
                        new XElement(ns + "th", "Target Display"),
                        new XElement(ns + "th", "Target Code System"),
                        new XElement(ns + "th", "Comments")
                        ),
                        from cmg in conceptMap.Group
                        from exp in cmg.Element
                        select new XElement(ns + "tr",
                            new XElement(ns + "td", exp.Code),
                            new XElement(ns + "td", exp.Display),
                            new XElement(ns + "td", cmg.Source),
                            new XElement(ns + "td", exp.Target[0].Equivalence.ToString()),
                            new XElement(ns + "td", exp.Target[0].Code),
                            new XElement(ns + "td", exp.Target[0].Display),
                            new XElement(ns + "td", cmg.Target),
                            new XElement(ns + "td", exp.Target[0].Comment)
                    )
                 );
            }

            return xConceptMapList;
        }
        
        private static string GetIdentifier(string id, NameValueCollection queryParam)
        {
            string identifier = id;

            // get parameter values that might uniquely identify the Resource

            if (string.IsNullOrEmpty(identifier))
            {
                identifier = Utilities.GetQueryValue("_id", queryParam);
            }

            if (string.IsNullOrEmpty(identifier))
            {
                identifier = Utilities.GetQueryValue("identifier", queryParam);
            }

            if (string.IsNullOrEmpty(identifier))
            {
                identifier = Utilities.GetQueryValue("url", queryParam); ;
            }

            return identifier;
        }
    }
}