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
    using Vintage.AppServices.DataAccessClasses;

    public static class TerminologyCodeSystem
    {

        internal const string FIND_MATCHES_UNSUPPORTED = "UNSUPPORTED FIND-MATCHES CODE SYSTEM OPERATION";
        internal const string INVALID_CODE = "INVALID CODE";
        internal const string MISSING_CODE_CODING = "MISSING CODE OR CODING VALUE";
        internal const string MISSING_FOCUS_CONCEPT = "MISSING FOCUS VALUE";
        internal const string MISSING_ATTRIBUTE = "NO ATTRIBUTE PROPERTY VALUES";
        internal const string MISSING_EXACT = "MISSING EXACT PROPERTY VALUE";
        internal const string NO_EXACT_MATCH = "NO EXACT MATCHES";
        internal const string MISSING_CODESYSTEM = "MISSING CODE SYSTEM";
        internal const string SUBSUMPTION_UNSUPPORTED = "UNSUPPORTED SUBSUMES CODE SYSTEM OPERATION";
        internal const string UNRECOGNISED_OPERATION = "UNRECOGNISED OPERATION";
        internal const string UNSUPPORTED_CODESYSTEM = "UNSUPPORTED CODE SYSTEM";
        internal const string UNSUPPORTED_EXACT_SETTING = "UNSUPPORTED EXACT PROPERTY VALUE";
        internal const string UNSUPPORTED_DISPLAY_LANGUAGE = "UNSUPPORTED DISPLAY LANGUAGE_PARAMETER";

        internal const string CODE_RELATIONSHIP_EQUIVALENT = "equivalent";
        internal const string CODE_RELATIONSHIP_NOT_SUBSUMED = "not-subsumed";
        internal const string CODE_RELATIONSHIP_SUBSUMES = "subsumes";
        internal const string CODE_RELATIONSHIP_SUBSUMED_BY = "subsumed-by";

        // entry method - handles all exceptions (converts to outcome operations)
        public static Resource PerformOperation(string id, string operation, NameValueCollection queryParam)
        {
            Resource fhirResource = null;

            try
            {
                string identifier = GetIdentifier(id, queryParam);
                string systemURL = GetSystem(queryParam);

                // check requested operation
                if (string.IsNullOrEmpty(operation))
                {
                    if (string.IsNullOrEmpty(identifier) && string.IsNullOrEmpty(systemURL))
                    {
                        fhirResource = GetAllCodeSystems(queryParam);
                    }
                    else
                    {
                        fhirResource = GetRequest(identifier, systemURL, queryParam);
                    }
                }
                else if (operation == "$lookup")
                {
                    fhirResource = LookupOperation(systemURL,queryParam);
                }
                else if (operation == "$validate-code")
                {
                   fhirResource = ValidateCodeOperation(systemURL, queryParam);
                }
                else if (operation == "$subsumes")
                {
                    fhirResource = SubsumesOperation(systemURL, queryParam);
                }
                else if (operation == "$find-matches")
                {
                    throw new Exception(FIND_MATCHES_UNSUPPORTED);
                    //fhirResource = FindMatchesOperation(systemURL, queryParam);
                }                
                else
                {
                    return OperationOutcome.ForMessage("Unrecognised Operation in Request..." + operation, OperationOutcome.IssueType.Unknown, OperationOutcome.IssueSeverity.Error);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == MISSING_CODE_CODING)
                {
                    return OperationOutcome.ForMessage("No code or coding value in Request", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == MISSING_CODESYSTEM)
                {
                    return OperationOutcome.ForMessage("No system to define code in Request", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == INVALID_CODE)
                {
                    return OperationOutcome.ForMessage("Invalid Code.", OperationOutcome.IssueType.CodeInvalid, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message ==  UNSUPPORTED_CODESYSTEM)
                {
                    return OperationOutcome.ForMessage("Code System not supported.", OperationOutcome.IssueType.NotSupported, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == SUBSUMPTION_UNSUPPORTED)
                {
                    return OperationOutcome.ForMessage("Subsumption not supported for specified Code System.", OperationOutcome.IssueType.NotSupported, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == FIND_MATCHES_UNSUPPORTED)
                {
                    return OperationOutcome.ForMessage("Composition not supported for specified Code System.", OperationOutcome.IssueType.NotSupported, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == MISSING_ATTRIBUTE)
                {
                    return OperationOutcome.ForMessage("Must submit at least one Attribute/Value Pair Property.", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == UNSUPPORTED_EXACT_SETTING)
                {
                    return OperationOutcome.ForMessage("Exact Property value not supported.", OperationOutcome.IssueType.NotSupported, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == UNSUPPORTED_DISPLAY_LANGUAGE)
                {
                    return OperationOutcome.ForMessage("Display Language requested is not supported (please omit or request en-NZ).", OperationOutcome.IssueType.NotSupported, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == MISSING_EXACT)
                {
                    return OperationOutcome.ForMessage("Missing Exact Property.", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == MISSING_FOCUS_CONCEPT)
                {
                    return OperationOutcome.ForMessage("Missing Focus Concept.", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == NO_EXACT_MATCH)
                {
                    return OperationOutcome.ForMessage("No Exact Matches.", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                throw;
            }

            return fhirResource;
        }

        private static Resource GetAllCodeSystems(NameValueCollection queryParam)
        {

            string contentMode = Utilities.GetQueryValue("content-mode", queryParam);
            string supplements = Utilities.GetQueryValue("supplements", queryParam);

            if (supplements == "bundle-type" || supplements == "http://hl7.org/fhir/bundle-type")
            {
                return GetRequest("bundle-type-german", string.Empty, queryParam);
            }

            Bundle csBundle = new Bundle();

            csBundle.Id = Guid.NewGuid().ToString();
            csBundle.Type = Bundle.BundleType.Searchset;
            csBundle.Link.Add(new Bundle.LinkComponent { Url = ServerCapability.TERMINZ_CANONICAL + "/CodeSystem", Relation = "self" });

            if (string.IsNullOrEmpty(contentMode) || contentMode == "not-present")
            {
                csBundle.AddResourceEntry(GetRequest(string.Empty, FhirSnomed.URI, queryParam), FhirSnomed.URI);
                csBundle.AddResourceEntry(GetRequest(string.Empty, FhirLoinc.URI, queryParam), FhirLoinc.URI);
                csBundle.AddResourceEntry(GetRequest(string.Empty, NzMt.URI, queryParam), NzMt.URI);
            }

            if (string.IsNullOrEmpty(contentMode) || contentMode == "complete")
            {
                csBundle.AddResourceEntry(GetRequest("NzEthnicityL1", string.Empty, queryParam), NzEthnicityL1.URI);
                csBundle.AddResourceEntry(GetRequest("NzEthnicityL2", string.Empty, queryParam), NzEthnicityL2.URI);
                csBundle.AddResourceEntry(GetRequest("NzEthnicityL3", string.Empty, queryParam), NzEthnicityL3.URI);
                csBundle.AddResourceEntry(GetRequest("NzEthnicityL4", string.Empty, queryParam), NzEthnicityL4.URI);
                csBundle.AddResourceEntry(GetRequest("NzRegion", string.Empty, queryParam), NzRegion.URI);
                csBundle.AddResourceEntry(GetRequest("bundle-type", string.Empty, queryParam), "http://hl7.org/fhir/bundle-type");
            }

            if (string.IsNullOrEmpty(contentMode) || contentMode == "supplement")
            {
                csBundle.AddResourceEntry(GetRequest("bundle-type-german", string.Empty, queryParam), "http://hl7.org/fhir/bundle-type-de");
            }

            csBundle.Total = csBundle.Entry.Count();
           
            if (csBundle.Total == 0)
            {
                return OperationOutcome.ForMessage("No Code Systems match search parameter values.", OperationOutcome.IssueType.NotFound, OperationOutcome.IssueSeverity.Information);
            }

            return csBundle;
        }

        private static Resource GetRequest(string identifier, string systemURL, NameValueCollection queryParam)
        {

            if (string.IsNullOrEmpty(identifier) && string.IsNullOrEmpty(systemURL))
            {
                throw new Exception(MISSING_CODESYSTEM);
            }

            string resourceFilePath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) + @"\Test Files\";

            CodeSystem codeSys = new CodeSystem();

            string codeVal = Utilities.GetQueryValue("code", queryParam);
            string versionVal = Utilities.GetQueryValue("version", queryParam);

            if (identifier == "NzRegion" || systemURL == NzRegion.URI)
            {
                NzRegion nzr = new NzRegion(TerminologyOperation.define_cs, versionVal, codeVal, string.Empty, -1, -1);
                codeSys = nzr.codeSystem;               
            }
            else if (identifier == "NzEthnicityL1" || systemURL == NzEthnicityL1.URI)
            {
                NzEthnicityL1 nzEth1 = new NzEthnicityL1(TerminologyOperation.define_cs, versionVal, codeVal, string.Empty, -1, -1);
                codeSys = nzEth1.codeSystem;
            }
            else if (identifier == "NzEthnicityL2" || systemURL == NzEthnicityL2.URI)
            {
                NzEthnicityL2 nzEth2 = new NzEthnicityL2(TerminologyOperation.define_cs, versionVal, codeVal, string.Empty, -1, -1);
                codeSys = nzEth2.codeSystem;
            }
            else if (identifier == "NzEthnicityL3" || systemURL == NzEthnicityL3.URI)
            {
                NzEthnicityL3 nzEth3 = new NzEthnicityL3(TerminologyOperation.define_cs, versionVal, codeVal, string.Empty, -1, -1);
                codeSys = nzEth3.codeSystem;
            }
            else if (identifier == "NzEthnicityL4" || systemURL == NzEthnicityL4.URI)
            {
                NzEthnicityL4 nzEth4 = new NzEthnicityL4(TerminologyOperation.define_cs, versionVal, codeVal, string.Empty, -1, -1);
                codeSys = nzEth4.codeSystem;
            }
            else if (identifier == "LOINC" || systemURL == FhirLoinc.URI)
            {
                FhirLoinc loinc = new FhirLoinc(TerminologyOperation.define_cs, versionVal, codeVal, string.Empty, string.Empty, -1, -1);
                codeSys = loinc.codeSystem;
            }
            else if (identifier == "SNOMEDCT" || systemURL == FhirSnomed.URI)
            {
                FhirSnomed snomed = new FhirSnomed(TerminologyOperation.define_cs, versionVal, codeVal, string.Empty, string.Empty, -1, -1);
                codeSys = snomed.codeSystem;
            }
            else if (identifier == "NZMT" || systemURL == NzMt.URI)
            {
                NzMt nzmt = new NzMt(TerminologyOperation.define_cs, versionVal, codeVal, string.Empty, string.Empty, -1, -1);
                codeSys = nzmt.codeSystem;
            }
            else if (identifier == "bundle-type" || systemURL == "http://hl7.org/fhir/bundle-type")
                {
                FhirJsonParser jsp = new FhirJsonParser();
                codeSys = jsp.Parse<CodeSystem>(File.ReadAllText(resourceFilePath + "BundleType.json"));
            }
            else if (identifier == "bundle-type-german" || systemURL == "http://hl7.org/fhir/bundle-type-de")
            {
                FhirJsonParser jsp = new FhirJsonParser();
                codeSys = jsp.Parse<CodeSystem>(File.ReadAllText(resourceFilePath + "BundleTypeGerman.json"));
            }
            else
            {
                throw new Exception(UNSUPPORTED_CODESYSTEM);
            }

            AddNarrative(codeSys);

            return codeSys;
        }

        private static CodeSystem GetCodeSystem(TerminologyOperation termOp, string systemURL, NameValueCollection queryParam)
        {
            CodeSystem codeSys = new CodeSystem();

            string codeVal = Utilities.GetQueryValue("code", queryParam);
            string versionVal = Utilities.GetQueryValue("version", queryParam);
            string languageVal = Utilities.GetQueryValue("displayLanguage", queryParam);
         
            // NB: these won't be passed in a GET request & the POST processing places the atomic elements in the queryParam collection
            //string codingVal = Utilities.GetQueryValue("coding", queryParam);
            //string codeableConceptVal = Utilities.GetQueryValue("codeableConcept", queryParam)

            if (string.IsNullOrEmpty(systemURL))
            {
                throw new Exception(MISSING_CODESYSTEM);
            }
            else if (string.IsNullOrEmpty(codeVal))
            {
                throw new Exception(MISSING_CODE_CODING);
            }
            else if (!string.IsNullOrEmpty(languageVal) && languageVal != "en-NZ")
            {
                throw new Exception(UNSUPPORTED_DISPLAY_LANGUAGE);
            }

            if (systemURL == NzRegion.URI)
            {
                NzRegion nzr = new NzRegion(termOp, versionVal, codeVal, string.Empty, -1, -1);
                codeSys = nzr.codeSystem;
            }
            else if (systemURL == NzEthnicityL1.URI)
            {
                NzEthnicityL1 nzEth1 = new NzEthnicityL1(termOp, versionVal, codeVal, string.Empty, -1, -1);
                codeSys = nzEth1.codeSystem;
            }
            else if (systemURL == NzEthnicityL2.URI)
            {
                NzEthnicityL2 nzEth2 = new NzEthnicityL2(termOp, versionVal, codeVal, string.Empty, -1, -1);
                codeSys = nzEth2.codeSystem;
            }
            else if (systemURL == NzEthnicityL3.URI)
            {
                NzEthnicityL3 nzEth3 = new NzEthnicityL3(termOp, versionVal, codeVal, string.Empty, -1, -1);
                codeSys = nzEth3.codeSystem;
            }
            else if (systemURL == NzEthnicityL4.URI)
            {
                NzEthnicityL4 nzEth4 = new NzEthnicityL4(termOp, versionVal, codeVal, string.Empty, -1, -1);
                codeSys = nzEth4.codeSystem;
            }
            else if (systemURL == NzMt.URI)
            {
                NzMt nzmt = new NzMt(termOp, versionVal, codeVal, string.Empty, string.Empty, -1, -1);
                codeSys = nzmt.codeSystem;
            }
            else if (systemURL == FhirLoinc.URI)
            {
                FhirLoinc loinc = new FhirLoinc(termOp, versionVal, codeVal, string.Empty, string.Empty, -1, -1);
                codeSys = loinc.codeSystem;
            }
            else if (FhirSnomed.IsValidURI(systemURL))
            {
                FhirSnomed snomed = new FhirSnomed(termOp, versionVal, codeVal, string.Empty, string.Empty, -1, -1);
                codeSys = snomed.codeSystem;
            }
            else
            {
                throw new Exception(UNSUPPORTED_CODESYSTEM);
            }

            return codeSys;
        }

        private static Resource LookupOperation(string systemURL, NameValueCollection queryParam)
        {
            CodeSystem codeSys = GetCodeSystem(TerminologyOperation.lookup ,systemURL, queryParam);

            if (codeSys.Concept.Count < 1)
            {
                throw new Exception(INVALID_CODE);
            }

            string codeVal = Utilities.GetQueryValue("code", queryParam);
            //string versionVal = Utilities.GetQueryValue("version", queryParam);
            string propertyVal = Utilities.GetQueryValue("property", queryParam);
            string languageVal = Utilities.GetQueryValue("displayLanguage", queryParam);

            if (!string.IsNullOrEmpty(languageVal) && languageVal != "en-NZ")
            {
                throw new Exception(UNSUPPORTED_DISPLAY_LANGUAGE);
            }

            bool displayCode = false;
            bool displaySystem = false;
            bool displayVersion = true;
            bool displayDisplay = true; 
            bool displayDefinition = false;
            bool displayDesignation = false;
            bool displayParents = false;
            bool displayChildren = false;
            bool displaySubstance = false;
            bool displayInactive = false;
            bool displayModuleId = false;
            bool displaySufficientlyDefined = false;
            bool displayNf = false;
            bool displayNfTerse = false;
            bool displayAllAttributes = false;
            bool displaySingleAttribute = false;

            // standard properties...defined for all Code Systems
            /*
             // The following properties are defined for all code systems: 
             // system, version, display, definition, designation, parent, child, and lang.X where X is a language code in a designation
             // SCT properties
             // inactive	boolean	Whether the code is active or not (defaults to false). This is derived from the active column in the Concept file of the RF2 Distribution (by inverting the value)
             // sufficientlyDefined	boolean	True if the description logic definition of the concept includes sufficient conditions (i.e., if the concept is not primitive).
             // moduleId	code	The SNOMED CT concept id of the module that the concept belongs to.
             // normalForm	string	Generated Normal form expression for the provided code or expression, with terms
             // normalFormTerse	string	Generated Normal form expression for the provided code or expression, conceptIds only
             // In addition, any SNOMED CT relationships where the relationship type is subsumed by Attribute (246061005) automatically become properties. For example, laterality:
             // Laterality	code	In this case, the URI (See the code system definition) is http://snomed.info/id/272741003, which can be used to unambiguously map to the underlying concept
             */

            if (!string.IsNullOrEmpty(propertyVal))
            {
                string pvl = propertyVal.ToLower();
                displayCode = (pvl.Contains("code"));
                displaySystem = (pvl.Contains("system"));
                //displayVersion = (pvl.Contains("version")); // always return code system version
                //displayDisplay = true;  // default true - don't see much point in looking up the code and not displaying its description! (otherwise use validate-code)
                displayDefinition = (pvl.Contains("definition"));
                displayDesignation = (pvl.Contains("designation"));
                displayParents = (pvl.Contains("parent"));
                displayChildren = (pvl.Contains("child"));
                displaySubstance = (pvl.Contains("substance"));
                displayInactive = (pvl.Contains("inactive"));
                displaySufficientlyDefined = (pvl.Contains("sufficientlydefined"));
                displayModuleId = (pvl.Contains("moduleid"));
                displayNfTerse = (pvl.Contains("normalformterse"));
                displayNf = (pvl.Contains("normalform") && !displayNfTerse);
                displayAllAttributes = (pvl.Contains(FhirSnomed.SCT_ATTRIBUTE_CONCEPT));
                if (!displayAllAttributes)
                {
                    displaySingleAttribute = (pvl.Count(x => Char.IsDigit(x))>7);
                } 
            }
       
            List<Coding> loincPropertyVals = new List<Coding>();
            List<Coding> substanceCodeVals = new List<Coding>();
            List<Coding> designationCodeVals = new List<Coding>();
            List<Coding> childCodeVals = new List<Coding>();
            List<Coding> parentCodeVals = new List<Coding>();
            List<Coding> propertyCodeVals = new List<Coding>();
            List<Coding> attributeCodeVals = new List<Coding>();
            List<Coding> proximalPrimitiveCodeVals = new List<Coding>();

            // CodeSystem-specific actions
            
            if (systemURL == NzMt.URI)
            {
                string nzulmType = codeSys.Concept[0].Definition;
                string mp_id = codeSys.Concept[0].ElementId;
                if (displaySubstance)
                {
                    substanceCodeVals = NzUlmSearch.GetConceptSubstanceDataByCode(mp_id, nzulmType);
                }
            }

            if (systemURL == FhirLoinc.URI)
            {
                if (!string.IsNullOrEmpty(propertyVal))
                {
                    loincPropertyVals = LoincSearch.GetPropertiesByCode(codeVal);
                }
            }

            if (systemURL == FhirSnomed.URI)
            {
                displaySystem = true;
                displayDesignation = true;
                displayInactive = true;

                if (codeSys.Concept.Count > 0)
                {
                    if (displayDesignation)
                    {
                        designationCodeVals = SnomedCtSearch.GetConceptDesignationsByCode(codeVal);
                    }

                    if (displayInactive || displayModuleId || displaySufficientlyDefined)
                    {
                        propertyCodeVals = SnomedCtSearch.GetConceptPropertiesByCode(codeVal);
                    }

                    if (displayChildren)
                    {
                        childCodeVals = SnomedCtSearch.GetChildCodes(codeVal);
                    }

                    if (displayParents)
                    {
                        parentCodeVals = SnomedCtSearch.GetParentCodes(codeVal);
                    }

                    if (displayAllAttributes || displaySingleAttribute)
                    {
                        List<Coding> acv = SnomedCtSearch.GetAttributes(codeVal);
                        if (displayAllAttributes)
                        {
                            attributeCodeVals = acv;
                        }
                        else
                        {
                            foreach(Coding cv in acv)
                            {
                                if(propertyVal.Contains(cv.Code))
                                {
                                    attributeCodeVals.Add(cv);
                                }
                            }
                        }
                    }

                }
            }
               
            // build return parameters resource using default & requested properties
            Parameters param = new Parameters();

            if (codeSys.Concept != null)
            { 
                param.Add("name", codeSys.NameElement);

                foreach (CodeSystem.ConceptDefinitionComponent comp in codeSys.Concept)
                {
                    if (displaySystem) { param.Add("system", codeSys.UrlElement); }
                    if (displayVersion) { param.Add("version", codeSys.VersionElement); }
                    if (displayCode) { param.Add("code", comp.CodeElement); }
                    if (displayDisplay) { param.Add("display", comp.DisplayElement); }
                }

                foreach (Coding prop in loincPropertyVals)
                {
                    // return all of them
                    List<Tuple<string, Base>> tuples = new List<Tuple<string, Base>>
                    {
                        new Tuple<string, Base>("code", new FhirString(prop.Code)),
                        new Tuple<string, Base>("value", new FhirString(prop.Display))
                    };
                    param.Add("property", tuples);
                }

                foreach (Coding desig in designationCodeVals)
                {
                    List<Tuple<string, Base>> tuples = new List<Tuple<string, Base>>
                    {
                        new Tuple<string, Base>("use", new Coding { Display = desig.System, System = FhirSnomed.URI, Code = FhirSnomed.GetDesignationTypeId(desig.System) }),
                        new Tuple<string, Base>("value", new FhirString(desig.Display))
                    };
                    param.Add("designation", tuples);
                }

                foreach (Coding prop in propertyCodeVals)
                {
                    if ( (prop.Code == "inactive" && displayInactive) ||
                         (prop.Code == "sufficientlyDefined" && displaySufficientlyDefined) ||
                         (prop.Code == "moduleId" && displayModuleId) )
                    {
                        List<Tuple<string, Base>> tuples = new List<Tuple<string, Base>>
                        {
                            new Tuple<string, Base>("code", new FhirString(prop.Code)),
                            new Tuple<string, Base>("value", new FhirString(prop.Display))
                        };
                        param.Add("property", tuples);
                    }
                }

                if (displayNf || displayNfTerse)
                {
                    string nf = FhirSnomed.GetNormalFormDisplay(codeVal, displayNf);
                    List<Tuple<string, Base>> tuples = new List<Tuple<string, Base>>
                    {
                        new Tuple<string, Base>("code", new FhirString(displayNf ? "normalForm" : "normalFormTerse")),
                        new Tuple<string, Base>("value", new FhirString(nf))
                    };
                    param.Add("property", tuples);
                }

                foreach (Coding subst in substanceCodeVals)
                {
                    List<Tuple<string, Base>> tuples = new List<Tuple<string, Base>>
                    {
                        new Tuple<string, Base>("use", new Coding { Display = subst.System })
                    };
                    if (!string.IsNullOrEmpty(subst.Code))
                    {
                        tuples.Add(new Tuple<string, Base>("code", new FhirString(subst.Code)));
                    }
                    tuples.Add(new Tuple<string, Base>("value", new FhirString(subst.Display)));
                    param.Add("substance", tuples);
                }

                foreach (Coding parent in parentCodeVals)
                {
                    List<Tuple<string, Base>> tuples = new List<Tuple<string, Base>>
                    {
                        new Tuple<string, Base>("code", new FhirString("Parent")),
                        new Tuple<string, Base>("value", new FhirString(parent.Code)),
                        new Tuple<string, Base>("description", new FhirString(parent.Display))
                    };
                    param.Add("property", tuples);
                }

                foreach (Coding child in childCodeVals)
                {
                    List<Tuple<string, Base>> tuples = new List<Tuple<string, Base>>
                    {
                        new Tuple<string, Base>("code", new FhirString("Child")),
                        new Tuple<string, Base>("value", new FhirString(child.Code)),
                        new Tuple<string, Base>("description", new FhirString(child.Display))
                    };
                    param.Add("property", tuples);
                }

                foreach (Coding attrib in attributeCodeVals)
                {
                    List<Tuple<string, Base>> tuples = new List<Tuple<string, Base>>
                    {
                        new Tuple<string, Base>("code", new Code { Value = attrib.Code }),
                        new Tuple<string, Base>("valueCode", new Code { Value = attrib.Display })
                    };
                    param.Add("property", tuples);
                }
            }

            return param;
        }

        private static Resource ValidateCodeOperation(string systemURL, NameValueCollection queryParam)
        {
           
            string codeVal = Utilities.GetQueryValue("code", queryParam);
            string displayVal = Utilities.GetQueryValue("display", queryParam);
            string languageVal = Utilities.GetQueryValue("displayLanguage", queryParam);

            if (!string.IsNullOrEmpty(languageVal) && languageVal != "en-NZ")
            {
                throw new Exception(UNSUPPORTED_DISPLAY_LANGUAGE);
            }

            FhirBoolean validCode = new FhirBoolean(false);
            FhirString validDisplay = new FhirString();

            if (FhirSnomed.IsValidURI(systemURL))
            {
                validCode = new FhirBoolean(FhirSnomed.ValidateCode(codeVal, displayVal, out string prefTerm));
                validDisplay = new FhirString(prefTerm);
            }
            else
            {
                CodeSystem codeSys = GetCodeSystem(TerminologyOperation.validate_code, systemURL, queryParam);

                foreach (CodeSystem.ConceptDefinitionComponent cc in codeSys.Concept)
                {
                    if (string.IsNullOrEmpty(displayVal))
                    {
                        validCode = new FhirBoolean(true);
                        validDisplay = new FhirString(cc.Display);
                    }
                    else if (displayVal.Trim().ToLower() == cc.Display.Trim().ToLower())
                    {
                        validCode = new FhirBoolean(true);
                        validDisplay = new FhirString(cc.Display);
                    }
                    else
                    {
                        validDisplay = new FhirString(cc.Display);
                    }
                }               
            }
                    
            // build return parameters resource from search result
            
            Parameters param = new Parameters();
            param.Add("result", validCode);

            if (validCode.Value == false)
            {
                param.Add("message", new FhirString("The code/display value " + codeVal + " / " + displayVal + " passed is incorrect"));
            }

            if (!string.IsNullOrEmpty(validDisplay.Value))
            {
                param.Add("display", validDisplay);
            }

            return param;
        }

        private static Resource SubsumesOperation(string systemURL, NameValueCollection queryParam)
        {
            // POST processing places the atomic elements from codingA & codeSuper data types in the queryParam collection

            string codeA = Utilities.GetQueryValue("codeA", queryParam) + Utilities.GetQueryValue("codingA", queryParam);  // sub-type
            string codeB = Utilities.GetQueryValue("codeB", queryParam) + Utilities.GetQueryValue("codingB", queryParam);  // super-type

            if (string.IsNullOrEmpty(systemURL))
            {
                throw new Exception(MISSING_CODESYSTEM);
            }
            else if (string.IsNullOrEmpty(codeA))
            {
                throw new Exception(MISSING_CODE_CODING);
            }
            else if (string.IsNullOrEmpty(codeB))
            {
                throw new Exception(MISSING_CODE_CODING);
            }
           
            //string versionVal = Utilities.GetQueryValue("version", queryParam);

            CodeSystem codeSys = new CodeSystem();

            string outcome = CODE_RELATIONSHIP_NOT_SUBSUMED;
            string relat = string.Empty;

            if(systemURL == FhirSnomed.URI)
            {
                if (!int.TryParse(codeA, out int testRes))
                {
                    throw new Exception(INVALID_CODE);
                }
                else if (!int.TryParse(codeB, out testRes))
                {
                    throw new Exception(INVALID_CODE);
                }
                relat = GetRelationship(codeA, codeB);
                outcome = (string.IsNullOrEmpty(relat)) ? outcome : relat;
            }
            else
            {
                throw new Exception(SUBSUMPTION_UNSUPPORTED);
            }

            Parameters param = new Parameters();
            param.Add("outcome", new Code(outcome));

            return param;
        }

        private static Resource FindMatchesOperation(string systemURL, NameValueCollection queryParam)
        {
            string versionVal = Utilities.GetQueryValue("version", queryParam);
            string focusConcept = Utilities.GetQueryValue("focus", queryParam);
            string exactVal = Utilities.GetQueryValue("exact", queryParam);

            List<Coding> attributeCodeVals = GetAttributePairs(queryParam);

            if (string.IsNullOrEmpty(systemURL))
            {
                throw new Exception(MISSING_CODESYSTEM);
            }
            else if (string.IsNullOrEmpty(focusConcept))
            {
                throw new Exception(MISSING_FOCUS_CONCEPT);
            }
            else if (attributeCodeVals.Count < 1)
            {
                throw new Exception(MISSING_ATTRIBUTE);
            }
            else if (string.IsNullOrEmpty(exactVal))
            {
                throw new Exception(MISSING_EXACT);
            }
            else if (exactVal.ToLower() != "true")
            {
                throw new Exception(UNSUPPORTED_EXACT_SETTING);
            }

            Parameters param = new Parameters();

            if (systemURL == (FhirSnomed.URI))
            {
                // Get Proximal Primitive(s) of Focus Code
                List<Coding> proximalPrimitiveCodeVals = SnomedCtSearch.GetProximalPrimitives(focusConcept, false);
                if (proximalPrimitiveCodeVals.Count < 1)
                {
                    throw new Exception(INVALID_CODE);
                }

                // load proximal primitves into string array (5 is max number of proximal primitives for any one concept)
                string[] pp = new string[5];
                int ppCount = 0;
                foreach (Coding cc in proximalPrimitiveCodeVals)
                {
                    //if (cc.Code != focusConcept)
                    //{
                    pp[ppCount] = cc.Code;
                    ppCount++;
                    //}
                }

                // load attribute types and values into arrays (30 is max number of non-isa relationships for any one concept)
                string[] attType = new string[30];
                string[] attValue = new string[30];
                int attributeCount = 0;
                int relGroup;
                int relGroupSize = 1;

                foreach (Coding cc in attributeCodeVals)
                {
                    attType[attributeCount] = cc.Code;
                    attValue[attributeCount] = cc.Display;
                    int.TryParse(cc.Version, out relGroup);
                    relGroupSize = (relGroup > relGroupSize) ? relGroup : relGroupSize;
                    attributeCount++;
                }

                // get attributes and definition status from focus concept (as may need to add attributes if concept fully-defined)
                List<Coding> focusAttributes = SnomedCtSearch.GetAttributes(focusConcept);

                foreach (Coding cc in focusAttributes)
                {
                    if (cc.System.ToUpper() != "FULLY DEFINED")
                    {
                        break;
                    }

                    bool addFocusAttribute = true;

                    foreach (Coding acv in attributeCodeVals)
                    {
                        if (acv.Code == cc.Code)
                        {
                            addFocusAttribute = false;
                            break;
                        }
                    }

                    if (addFocusAttribute)
                    {
                        for (int rg = 0; rg < relGroupSize; rg++ )
                        {
                            attType[attributeCount] = cc.Code;
                            attValue[attributeCount] = cc.Display;
                            attributeCount++;
                        }
                    }
                }

                List<Coding> matchVals = SnomedCtSearch.GetCompositionMatch(attType, attValue, pp, attributeCount);

                if (matchVals.Count < 1)
                {
                    throw new Exception(NO_EXACT_MATCH);
                }
                
                List<Tuple<string, Base>> tuples = new List<Tuple<string, Base>>();
                foreach (Coding cc in matchVals)
                {
                    tuples.Add(new Tuple<string, Base>("code", new Coding { Code = cc.Code, Display = cc.Display, System = systemURL }));
                    param.Add("match", tuples);
                }
            }
            else
            {
                throw new Exception(FIND_MATCHES_UNSUPPORTED);
            }

            return param;
        }

        internal static string GetIdentifier(string id, NameValueCollection queryParam)
        {
            string identifier = id;

            // get parameter values that might uniquely identify the ValueSet

            if (string.IsNullOrEmpty(id))
            {
                identifier = Utilities.GetQueryValue("_id", queryParam);
            }

            if (string.IsNullOrEmpty(identifier))
            {
                identifier = Utilities.GetQueryValue("identifier", queryParam);
            }

            return identifier;
        }

        internal static List<Coding> GetAttributePairs(NameValueCollection queryParam)
        {
            List<Coding> attributePairs = new List<Coding>();

            foreach (string key in queryParam)
            {
                int testInt;
                if (int.TryParse(key, out testInt))
                {
                    string[] vals = queryParam[key].Split(',');
                    foreach (string val in vals)
                    {
                        attributePairs.Add(new Coding { Code = key, Display = val, Version = vals.Length.ToString() });
                    }
                }
            }

            return attributePairs;
        }

        internal static string GetSystem(NameValueCollection queryParam)
        {
            string system = Utilities.GetQueryValue("system", queryParam);

            if (string.IsNullOrEmpty(system))
            {
                system = Utilities.GetQueryValue("url", queryParam); ;
            }

            return system;
        }

        internal static string GetRelationship(string codeA, string codeB)
        {
            string outcome = string.Empty;

            if (codeA == codeB)
            {
                outcome = CODE_RELATIONSHIP_EQUIVALENT;
            }
            // then test Code A is subsumed by Code B - i.e.Code B is the supertype
            else if ((bool)SnomedCtSearch.IsSubsumedBy(codeA, codeB))
            {
                outcome = CODE_RELATIONSHIP_SUBSUMED_BY;
            }
            else if ((bool)SnomedCtSearch.IsSubsumedBy(codeB, codeA))
            {
                // if not...test if Code A subsumes code B - i.e. code A is actually the supertype
                outcome = CODE_RELATIONSHIP_SUBSUMES;
            }

            return outcome;
        }

        private static CodeSystem AddNarrative(CodeSystem codeSys)
        {

            // create display text for CodeSystem resource

            XNamespace ns = "http://www.w3.org/1999/xhtml";

            var summary = new XElement(ns + "div",
                new XElement(ns + "h2", codeSys.Name),
                new XElement(ns + "p", codeSys.Description),
                new XElement(ns + "table",
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Id: "),
                    new XElement(ns + "td", codeSys.Id)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Url: "),
                    new XElement(ns + "td", codeSys.Url)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Version: "),
                    new XElement(ns + "td", codeSys.Version)
                    ),
                     new XElement(ns + "tr",
                    new XElement(ns + "td", "Publisher: "),
                    new XElement(ns + "td", codeSys.Publisher)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Copyright: "),
                    new XElement(ns + "td", codeSys.Copyright)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Status: "),
                    new XElement(ns + "td", codeSys.Status.ToString())
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Status Date: "),
                    new XElement(ns + "td", codeSys.Date)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Experimental: "),
                    new XElement(ns + "td", codeSys.Experimental.Value.ToString())
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Content: "),
                    new XElement(ns + "td", codeSys.Content.Value.ToString())
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Case Sensitive: "),
                    new XElement(ns + "td", codeSys.CaseSensitive.Value.ToString())
                    )

                 ),
                 renderConceptList(codeSys,ns)
              );

            codeSys.Text = new Narrative();
            codeSys.Text.Status = Narrative.NarrativeStatus.Generated;
            codeSys.Text.Div = summary.ToString();

            return codeSys;
        }

        private static XElement renderConceptList(CodeSystem codeSys, XNamespace ns)
        {
            XElement xConceptList = null;

            if (codeSys.Concept.Count > 0)
            {
                xConceptList = new XElement(ns + "table",
                    new XElement(ns + "tr",
                    new XElement(ns + "th", "Code"),
                    new XElement(ns + "th", "Display")
                    ),
                    from conc in codeSys.Concept
                    select new XElement(ns + "tr",
                        new XElement(ns + "td", conc.Code),
                        new XElement(ns + "td", conc.Display)
                    )
                 );
            }

            return xConceptList;
        }
    }

}
