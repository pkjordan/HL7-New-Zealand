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
    using Vintage.AppServices.BusinessClasses.FHIR.ValueSets;

    public static class TerminologyValueSet
    {
        internal const string INVALID_CODE = "INVALID CODE";
        internal const string INVALID_EXPRESSION = "INVALID EXPRESSION";
        internal const string INVALID_FILTER = "INVALID FILTER";
        internal const string MAX_VALUES_EXCEEDED = "MAXIMUM VALUES EXCEEDED";
        internal const string MISSING_CODE_CODING = "MISSING CODE OR CODING VALUE";
        internal const string MISSING_CODESYSTEM = "MISSING CODE SYSTEM";
        internal const string MISSING_FILTER = "INVALID FILTER";
        internal const string MISSING_IDENTIFIER = "MISSING IDENTIFIER";
        internal const string UNFOUND_VALUESET = "UNFOUND VALUE SET";
        internal const string UNRECOGNISED_OPERATION = "UNRECOGNISED OPERATION";
        internal const string UNSUPPORTED_EXPRESSION = "UNSUPPORTED EXPRESSION";
        internal const string UNSUPPORTED_FILTER = "UNSUPPORTED FILTER";
        internal const string UNSUPPORTED_DISPLAY_LANGUAGE = "UNSUPPORTED DISPLAY LANGUAGE_PARAMETER";
        internal const string UNSUPPORTED_VERSION = "UNSUPPORTED VERSION";

        // entry method - handles all exceptions (converts to outcome operations)
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
                        fhirResource = GetValueSets(queryParam);
                    }
                    else
                    {
                        fhirResource = GetRequest(identifier, queryParam);
                    }
                }
                else if(string.IsNullOrEmpty(identifier))
                {
                    throw new Exception(TerminologyValueSet.MISSING_IDENTIFIER);
                }
                else if (operation == "$expand")
                {
                    fhirResource = ExpandOperation(identifier, queryParam);
                }
                else if (operation == "$validate-code")
                {
                    fhirResource = ValidateCodeOperation(identifier, queryParam);
                }
                else
                {
                    return OperationOutcome.ForMessage("Unrecognised Operation in Request..." + operation, OperationOutcome.IssueType.Unknown, OperationOutcome.IssueSeverity.Error);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == MAX_VALUES_EXCEEDED)
                {
                    return OperationOutcome.ForMessage("Cannot return a ValueSet with over 9,999 values - please use a filter or search criteria to restrict this number.", OperationOutcome.IssueType.TooCostly,OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == MISSING_CODE_CODING)
                {
                    return OperationOutcome.ForMessage("No code or coding value in Request", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == MISSING_CODESYSTEM)
                {
                    return OperationOutcome.ForMessage("No system to define code in Request", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == MISSING_FILTER)
                {
                    return OperationOutcome.ForMessage("No filter for Implict ValueSet in Request", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == MISSING_IDENTIFIER)
                {
                    return OperationOutcome.ForMessage("No ValueSet id, identifier or URL in Request", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == UNSUPPORTED_VERSION)
                {
                    return OperationOutcome.ForMessage("Requested ValueSet Version Not Supported.", OperationOutcome.IssueType.NotSupported, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == INVALID_CODE)
                {
                    return OperationOutcome.ForMessage("Invalid Code.", OperationOutcome.IssueType.CodeInvalid, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == INVALID_EXPRESSION)
                {
                    return OperationOutcome.ForMessage("Invalid Expression.", OperationOutcome.IssueType.Invalid, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == INVALID_FILTER)
                {
                    return OperationOutcome.ForMessage("Text Filter Must Contain At Least 3 Characters.", OperationOutcome.IssueType.BusinessRule, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == UNFOUND_VALUESET)
                {
                    return OperationOutcome.ForMessage("Value Set Not Found.", OperationOutcome.IssueType.NotFound, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == UNSUPPORTED_EXPRESSION)
                {
                    return OperationOutcome.ForMessage("Expression Not Supported", OperationOutcome.IssueType.NotSupported, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == UNSUPPORTED_FILTER)
                {
                    return OperationOutcome.ForMessage("Property Filter Not Supported.", OperationOutcome.IssueType.NotFound, OperationOutcome.IssueSeverity.Error);
                }
                else if (ex.Message == UNSUPPORTED_DISPLAY_LANGUAGE)
                {
                    return OperationOutcome.ForMessage("Display Language Parameter Not Supported.", OperationOutcome.IssueType.NotSupported, OperationOutcome.IssueSeverity.Error);
                }

                throw;
            }

            return fhirResource;
        }

        private static Resource GetValueSets(NameValueCollection queryParam)
        {
            // primitive name searching
            string nameFilter = Utilities.GetQueryValue("name", queryParam).ToUpper();

            Bundle csBundle = new Bundle();

            csBundle.Id = Guid.NewGuid().ToString();
            csBundle.Type = Bundle.BundleType.Searchset;
            csBundle.Link.Add(new Bundle.LinkComponent { Url = ServerCapability.TERMINZ_CANONICAL + "/ValueSet", Relation = "self" });

            if (string.IsNullOrEmpty(nameFilter))  // Implict SCT VS - no searching by name
            {
                ValueSet snomedImplicit = GetValueSet(TerminologyOperation.define_vs, string.Empty, string.Empty, FhirSnomed.URI, string.Empty, "[x]", -1, -1);
                try { GenerateCompositionNarrative(snomedImplicit); }
                catch { }
                csBundle.AddResourceEntry(snomedImplicit, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/sct/[x]");
            }

            if (string.IsNullOrEmpty(nameFilter) || nameFilter.StartsWith("SCT"))
            {             
                ValueSet nzRefSet1 = GetValueSet(TerminologyOperation.define_vs, "SCT-REFSET-NZ-CARDIOLOGY", string.Empty, string.Empty, "91000210107", string.Empty, -1, -1);
                try { GenerateCompositionNarrative(nzRefSet1); }
                catch { }
                csBundle.AddResourceEntry(nzRefSet1, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/SCT-REFSET-NZ-CARDIOLOGY");

                ValueSet nzRefSet2 = GetValueSet(TerminologyOperation.define_vs, "SCT-REFSET-NZ-DISABILITY", string.Empty, string.Empty, "261000210101", string.Empty, -1, -1);
                try { GenerateCompositionNarrative(nzRefSet2); }
                catch { }
                csBundle.AddResourceEntry(nzRefSet2, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/SCT-REFSET-NZ-DISABILITY");

                ValueSet nzRefSet3 = GetValueSet(TerminologyOperation.define_vs, "SCT-REFSET-NZ-EC-DIAGNOSIS", string.Empty, string.Empty, "61000210102", string.Empty, -1, -1); ;
                try { GenerateCompositionNarrative(nzRefSet3); }
                catch { }
                csBundle.AddResourceEntry(nzRefSet3, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/SCT-REFSET-NZ-EC-DIAGNOSIS");

                ValueSet nzRefSet4 = GetValueSet(TerminologyOperation.define_vs, "SCT-REFSET-NZ-EC-PRESENTING-COMPLAINT", string.Empty, string.Empty, "71000210108", string.Empty, -1, -1);
                try { GenerateCompositionNarrative(nzRefSet4); }
                catch { }
                csBundle.AddResourceEntry(nzRefSet4, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/SCT-REFSET-NZ-EC-PRESENTING-COMPLAINT");

                ValueSet nzRefSet4a = GetValueSet(TerminologyOperation.define_vs, "SCT-REFSET-NZ-EC-PROCEDURE", string.Empty, string.Empty, "321000210102", string.Empty, -1, -1); ;
                try { GenerateCompositionNarrative(nzRefSet4a); }
                catch { }
                csBundle.AddResourceEntry(nzRefSet4a, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/SCT-REFSET-NZ-EC-PROCEDURE");

                ValueSet nzRefSet5 = GetValueSet(TerminologyOperation.define_vs, "SCT-REFSET-NZ-GATEWAY-CHILD-HEALTH", string.Empty, string.Empty, "241000210102", string.Empty, -1, -1);
                try { GenerateCompositionNarrative(nzRefSet5); }
                catch { }
                csBundle.AddResourceEntry(nzRefSet5, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/SCT-REFSET-NZ-GATEWAY-CHILD-HEALTH");

                ValueSet nzRefSet6 = GetValueSet(TerminologyOperation.define_vs, "SCT-REFSET-NZ-GYNAECOLOGY", string.Empty, string.Empty, "101000210108", string.Empty, -1, -1);
                try { GenerateCompositionNarrative(nzRefSet6); }
                catch { }
                csBundle.AddResourceEntry(nzRefSet6, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/SCT-REFSET-NZ-GYNAECOLOGY");

                ValueSet nzRefSet7 = GetValueSet(TerminologyOperation.define_vs, "SCT-REFSET-NZ-ACC-TRANSLATION-TABLE", string.Empty, string.Empty, "81000210105", string.Empty, -1, -1);
                try { GenerateCompositionNarrative(nzRefSet7); }
                catch { }
                csBundle.AddResourceEntry(nzRefSet7, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/SCT-REFSET-NZ-ACC-TRANSLATION-TABLE");

                ValueSet nzRefSet8 = GetValueSet(TerminologyOperation.define_vs, "SCT-REFSET-NZ-NOTIFIABLE-DISEASE", string.Empty, string.Empty, "251000210104", string.Empty, -1, -1);
                try { GenerateCompositionNarrative(nzRefSet8); }
                catch { }
                csBundle.AddResourceEntry(nzRefSet8, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/SCT-REFSET-NZ-NOTIFIABLE-DISEASE");

                ValueSet nzRefSet9 = GetValueSet(TerminologyOperation.define_vs, "SCT-REFSET-NZ-RHEUMATOLOGY", string.Empty, string.Empty, "121000210100", string.Empty, -1, -1);
                try { GenerateCompositionNarrative(nzRefSet9); }
                catch { }
                csBundle.AddResourceEntry(nzRefSet9, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/SCT-REFSET-NZ-RHEUMATOLOGY");

                ValueSet nzRefSet10 = GetValueSet(TerminologyOperation.define_vs, "SCT-REFSET-NZ-SMOKING", string.Empty, string.Empty, "51000210100", string.Empty, -1, -1);
                try { GenerateCompositionNarrative(nzRefSet10); }
                catch { }
                csBundle.AddResourceEntry(nzRefSet10, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/SCT-REFSET-NZ-SMOKING");
               
                ValueSet nzRefSet11 = GetValueSet(TerminologyOperation.define_vs, "SCT-REFSET-NZ-ADVERSE-REACTION-MANIFESTATION", string.Empty, string.Empty, "351000210106", string.Empty, -1, -1);
                try { GenerateCompositionNarrative(nzRefSet11); }
                catch { }
                csBundle.AddResourceEntry(nzRefSet11, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/SCT-REFSET-NZ-ADVERSE-REACTION-MANIFESTATION");

                ValueSet nzRefSet12 = GetValueSet(TerminologyOperation.define_vs, "SCT-REFSET-NZ-AMBULANCE-CLINICAL-IMPRESSION", string.Empty, string.Empty, "421000210109", string.Empty, -1, -1);
                try { GenerateCompositionNarrative(nzRefSet12); }
                catch { }
                csBundle.AddResourceEntry(nzRefSet12, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/SCT-REFSET-NZ-AMBULANCE-CLINICAL-IMPRESSION");

                ValueSet nzRefSet13 = GetValueSet(TerminologyOperation.define_vs, "SCT-REFSET-NZ-MICROORGANISM", string.Empty, string.Empty, "391000210104", string.Empty, -1, -1);
                try { GenerateCompositionNarrative(nzRefSet13); }
                catch { }
                csBundle.AddResourceEntry(nzRefSet13, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/SCT-REFSET-NZ-MICROORGANISM");

                ValueSet nzRefSet14 = GetValueSet(TerminologyOperation.define_vs, "SCT-REFSET-NZ-ENDOCRINOLOGY", string.Empty, string.Empty, "141000210106", string.Empty, -1, -1);
                try { GenerateCompositionNarrative(nzRefSet14); }
                catch { }
                csBundle.AddResourceEntry(nzRefSet14, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/SCT-REFSET-NZ-ENDOCRINOLOGY");

                ValueSet snomedVsBodyStructure = GetValueSet(TerminologyOperation.define_vs, "SCT-BODY-STRUCTURE", string.Empty, string.Empty, string.Empty, "[x]", -1, -1);
                try { GenerateCompositionNarrative(snomedVsBodyStructure); }
                catch { }
                csBundle.AddResourceEntry(snomedVsBodyStructure, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/sct_body_structure/[x]");
            
                ValueSet snomedVsClinicalFinding = GetValueSet(TerminologyOperation.define_vs, "SCT-CLINICAL-FINDING", string.Empty, string.Empty, string.Empty, "[x]", -1, -1);
                try { GenerateCompositionNarrative(snomedVsClinicalFinding); }
                catch { }
                csBundle.AddResourceEntry(snomedVsClinicalFinding, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/sct_clinical_finding/[x]");
            
                ValueSet snomedVsObservableEntity = GetValueSet(TerminologyOperation.define_vs, "SCT-OBSERVABLE-ENTITY", string.Empty, string.Empty, string.Empty, "[x]", -1, -1);
                try { GenerateCompositionNarrative(snomedVsObservableEntity); }
                catch { }
                csBundle.AddResourceEntry(snomedVsObservableEntity, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/sct_observable_entity/[x]");
           
                ValueSet snomedVsProduct = GetValueSet(TerminologyOperation.define_vs, "SCT-PHARMACEUTICAL", string.Empty, string.Empty, string.Empty, "[x]", -1, -1);
                try { GenerateCompositionNarrative(snomedVsProduct); }
                catch { }
                csBundle.AddResourceEntry(snomedVsProduct, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/sct_pharmaceutical/[x]");
            
                ValueSet snomedVsProcedure = GetValueSet(TerminologyOperation.define_vs, "SCT-PROCEDURE", string.Empty, string.Empty, string.Empty, "[x]", -1, -1);
                try { GenerateCompositionNarrative(snomedVsProcedure); }
                catch { }
                csBundle.AddResourceEntry(snomedVsProcedure, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/sct_procedure/[x]");
           
                ValueSet snomedVsSubstance = GetValueSet(TerminologyOperation.define_vs, "SCT-SUBSTANCE", string.Empty, string.Empty, string.Empty, "[x]", -1, -1);
                try { GenerateCompositionNarrative(snomedVsSubstance); }
                catch { }
                csBundle.AddResourceEntry(snomedVsSubstance, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/sct_substance/[x]");
            }

            if (string.IsNullOrEmpty(nameFilter)) // implicit LOINC VS - no searching by name
            {
                ValueSet loincImplicit = GetValueSet(TerminologyOperation.define_vs, string.Empty, string.Empty, FhirLoinc.URI, string.Empty, "[x]", -1, -1);
                try { GenerateCompositionNarrative(loincImplicit); }
                catch { }
                csBundle.AddResourceEntry(loincImplicit, ServerCapability.TERMINZ_CANONICAL + "/ValueSet/loinc/[x]");
            }

            if (string.IsNullOrEmpty(nameFilter) || nameFilter.StartsWith("UCUM"))
            {
                csBundle.AddResourceEntry(GetRequest("UCUM-COMMON", queryParam), "http://hl7.org/fhir/ValueSet/ucum-common");
            }

            if (string.IsNullOrEmpty(nameFilter) || nameFilter.StartsWith("BUNDLE-TYPE-SUPPLEMENTED"))
            {
                csBundle.AddResourceEntry(GetRequest("BUNDLE-TYPE-SUPPLEMENTED", queryParam), "http://hl7.org/fhir/bundle-type-supplemented");
            }

            if (string.IsNullOrEmpty(nameFilter) || nameFilter.StartsWith("NZULM"))
            {
                csBundle.AddResourceEntry(GetRequest("NZULM_Containered_Trade_Product_Pack", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzulmCtpp");
                csBundle.AddResourceEntry(GetRequest("NZULM_Medicinal_Product", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzulmMp");
                csBundle.AddResourceEntry(GetRequest("NZULM_Medicinal_Product_Pack", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzulmMpp");
                csBundle.AddResourceEntry(GetRequest("NZULM_Medicinal_Product_Unit_Of_Use", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzulmMpuu");
                csBundle.AddResourceEntry(GetRequest("NZULM_Trade_Product", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzulmTp");
                csBundle.AddResourceEntry(GetRequest("NZULM_Trade_Product_Pack", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzulmTpp");
                csBundle.AddResourceEntry(GetRequest("NZULM_Trade_Product_Unit_Of_Use", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzulmTpuu");
                csBundle.AddResourceEntry(GetRequest("NZULM_Prescribing_Terms", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzulmPrescribingTerms");
                csBundle.AddResourceEntry(GetRequest("NZULM_Prescribing_Terms_Generic", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzulmPrescribingTermsGeneric");
                csBundle.AddResourceEntry(GetRequest("NZULM_Prescribing_Terms_Trade", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzulmPrescribingTermsTrade");
            }

            if (string.IsNullOrEmpty(nameFilter) || nameFilter.StartsWith("NZETHNIC"))
            {
                csBundle.AddResourceEntry(GetRequest("NzEthnicityL1", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzEthnicityL1");
                csBundle.AddResourceEntry(GetRequest("NzEthnicityL2", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzEthnicityL2");
                csBundle.AddResourceEntry(GetRequest("NzEthnicityL3", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzEthnicityL3");
                csBundle.AddResourceEntry(GetRequest("NzEthnicityL4", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzEthnicityL4");
            }

            if (string.IsNullOrEmpty(nameFilter) || nameFilter.StartsWith("NZREGION"))
            {
                csBundle.AddResourceEntry(GetRequest("NzRegion", queryParam), ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzRegion");
            }

            // test value sets (not searchable by name)
            if (string.IsNullOrEmpty(nameFilter))
            {
                csBundle.AddResourceEntry(GetRequest("extensional-case-1", queryParam), "http://www.healthintersections.com.au/fhir/ValueSet/extensional-case-1");
                csBundle.AddResourceEntry(GetRequest("extensional-case-2", queryParam), "http://www.healthintersections.com.au/fhir/ValueSet/extensional-case-2");
                csBundle.AddResourceEntry(GetRequest("extensional-case-3", queryParam), "http://www.healthintersections.com.au/fhir/ValueSet/extensional-case-3");
                csBundle.AddResourceEntry(GetRequest("extensional-case-4", queryParam), "http://www.healthintersections.com.au/fhir/ValueSet/extensional-case-4");
                csBundle.AddResourceEntry(GetRequest("intensional-case-1", queryParam), "http://www.healthintersections.com.au/fhir/ValueSet/intensional-case-1");
                csBundle.AddResourceEntry(GetRequest("intensional-case-2", queryParam), "http://www.healthintersections.com.au/fhir/ValueSet/intensional-case-2");
                //csBundle.AddResourceEntry(GetRequest("intensional-case-3", queryParam), "http://www.healthintersections.com.au/fhir/ValueSet/intensional-case-3");
            }

            csBundle.Total = csBundle.Entry.Count();

            return csBundle;
        }

        private static Resource GetRequest(string identifier, NameValueCollection queryParam)
        {
            // get parameter values
            int offsetNo = -1;
            int countNo = -1;

            string codeSystem = Utilities.GetQueryValue("system", queryParam);
            string codeVal = Utilities.GetQueryValue("code", queryParam);
            string versionVal = Utilities.GetQueryValue("version", queryParam);

            // _filter - implemented as a case-insensitive 'contains' operation on display text <TODO> check this - not referring to summary
            string filter = Utilities.GetQueryValue("_filter", queryParam);

            ValueSet valSet = GetValueSet(TerminologyOperation.define_vs, identifier, codeVal, codeSystem, versionVal, filter, offsetNo, countNo);

            try
            {
                GenerateCompositionNarrative(valSet);
            }
            catch { }

            return valSet;
        }

        private static Resource ExpandOperation(string identifier, NameValueCollection queryParam)
        {
  
            int offsetNo = 0;
            int countNo = -1;

            // filter - implemented as a case-insensitive 'contains' operation on display text
            string filter = Utilities.GetQueryValue("filter", queryParam);
            string offset = Utilities.GetQueryValue("offset", queryParam);
            string count = Utilities.GetQueryValue("count", queryParam);
            string languageVal = Utilities.GetQueryValue("displayLanguage", queryParam);
            string vsVersionVal = Utilities.GetQueryValue("valueSetVersion", queryParam);

            if (!string.IsNullOrEmpty(languageVal) && languageVal != "en-NZ")
            {
                throw new Exception(UNSUPPORTED_DISPLAY_LANGUAGE);
            }

            if (!int.TryParse(count, out countNo))
            {
                countNo = -1;
            }

            if (!int.TryParse(offset, out offsetNo))
            {
                offsetNo = 0;
            }

            //TerminologyOperation.expand
            ValueSet valSet = GetValueSet(TerminologyOperation.expand, identifier, string.Empty, string.Empty, vsVersionVal, filter, offsetNo, countNo);

            try
            {
                GenerateExpansionNarrative(valSet, countNo);
            }
            catch { }

            return valSet;
        }

        private static Resource ValidateCodeOperation(string identifier, NameValueCollection queryParam)
        {
            bool subsumptionTest = false;
            int offsetNo = -1;
            int countNo = -1;

            string systemVal = Utilities.GetQueryValue("system", queryParam);
            string codeVal = Utilities.GetQueryValue("code", queryParam);
            string displayVal = Utilities.GetQueryValue("display", queryParam);
            string versionVal = Utilities.GetQueryValue("valueSetVersion", queryParam);
            string identifierVal = Utilities.GetQueryValue("identifier", queryParam);
            string languageVal = Utilities.GetQueryValue("displayLanguage", queryParam);

            // NB: these won't be passed in a GET request & the POST processing places the atomic elements in the queryParam collection
            //string codingVal = Utilities.GetQueryValue("coding", queryParam);
            //string codeableConceptVal = Utilities.GetQueryValue("codeableConcept", queryParam);

            // SNOMED CT subsumption validation tests place SuperType in identifier parameter
            if (!string.IsNullOrEmpty(identifierVal))
            {
                identifier = identifierVal;
                subsumptionTest = true;
            }

            if (string.IsNullOrEmpty(codeVal))
            {
                throw new Exception(MISSING_CODE_CODING);
            }
            else if (string.IsNullOrEmpty(systemVal))
            {
                throw new Exception(MISSING_CODESYSTEM);
            }
            else if (!string.IsNullOrEmpty(languageVal) && languageVal != "en-NZ")
            {
                throw new Exception(UNSUPPORTED_DISPLAY_LANGUAGE);
            }

            // get a value set for requested code or display
            ValueSet valSet = GetValueSet(TerminologyOperation.validate_code, identifier, codeVal, systemVal, versionVal, displayVal, offsetNo, countNo);

            FhirBoolean validCode = new FhirBoolean(false);
            FhirString validDisplay = new FhirString();

            // n.b. Value Sets are expanded for the validate code operation
            if (valSet.Expansion != null)
            {
                foreach (ValueSet.ContainsComponent cc in valSet.Expansion.Contains)
                {
                    if (subsumptionTest)
                    {
                        if (cc.Code.Trim() == codeVal)
                        {
                            validCode = new FhirBoolean(true);
                        }
                    }
                    else
                    {
                        if (FhirSnomed.IsValidURI(systemVal))
                        {
                            // special validation for SCT due to varying Case Significance settings for descriptions
                            validCode = new FhirBoolean(FhirSnomed.ValidateCode(codeVal, displayVal, out string prefTerm));
                            validDisplay = new FhirString(prefTerm);
                        }
                        else
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
                }
            }

            // build return parameters resource from search result
            // just return simple true/false if this is a SNOMED CT subsumption test

            Parameters param = new Parameters();
            param.Add("result", validCode);

            if (!subsumptionTest)
            {
                if (validCode.Value == false)
                {
                    param.Add("message", new FhirString("The code/display value " + codeVal + " / " + displayVal + " passed is incorrect"));
                }
                if (!string.IsNullOrEmpty(validDisplay.Value))
                {
                    param.Add("display", validDisplay);
                }
            }

            return param;

        }

        private static ValueSet GetValueSet(TerminologyOperation termOp, string identifier, string code, string codeSystem, string valueSetVersion, string filter, int offsetNo, int countNo)
        {
            // <TODO> need a factory method here
            string requestedVersion = valueSetVersion;
            ValueSet valSet = new ValueSet();
            bool extensionalFromFile = false;

            string resourceFilePath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) + @"\Test Files\";

            if (identifier == "extensional-case-1" || identifier == "http://www.healthintersections.com.au/fhir/ValueSet/extensional-case-1")
            {
                if (!string.IsNullOrEmpty(codeSystem) && codeSystem != "http://hl7.org/fhir/administrative-gender")
                {
                    valueSetVersion = "0.0";
                }
                ConnectathonExtensional_1 vs = new ConnectathonExtensional_1(termOp, valueSetVersion, code, filter, offsetNo, countNo);
                valSet = vs.valueSet;
            }
            else if (identifier == "extensional-case-2" || identifier == "http://www.healthintersections.com.au/fhir/ValueSet/extensional-case-2")
            {
                if (!string.IsNullOrEmpty(codeSystem) && codeSystem != FhirLoinc.URI)
                {
                    valueSetVersion = "0.0";
                }
                ConnectathonExtensional_2 vs = new ConnectathonExtensional_2(termOp, valueSetVersion, code, filter, offsetNo, countNo);
                valSet = vs.valueSet;
            }
            else if (identifier == "extensional-case-3" || identifier == "http://www.healthintersections.com.au/fhir/ValueSet/extensional-case-3")
            {
                if (!string.IsNullOrEmpty(codeSystem) && codeSystem != FhirSnomed.URI)
                {
                    valueSetVersion = "0.0";
                }
                ConnectathonExtensional_3 vs = new ConnectathonExtensional_3(termOp, valueSetVersion, code, filter, offsetNo, countNo);
                valSet = vs.valueSet;
            }
            else if (identifier == "extensional-case-4" || identifier == "http://www.healthintersections.com.au/fhir/ValueSet/extensional-case-4")
            {
                if (!string.IsNullOrEmpty(codeSystem) && codeSystem != "http://hl7.org/fhir/administrative-gender" && codeSystem != "http://hl7.org/fhir/v2/0001")
                {
                    valueSetVersion = "0.0";
                }
                ConnectathonExtensional_4 vs = new ConnectathonExtensional_4(termOp, valueSetVersion, code, filter, offsetNo, countNo);
                valSet = vs.valueSet;
            }
            else if (identifier == "intensional-case-1" || identifier == "http://www.healthintersections.com.au/fhir/ValueSet/intensional-case-1")
            {
                if (!string.IsNullOrEmpty(codeSystem) && codeSystem != FhirLoinc.URI)
                {
                    valueSetVersion = "0.0";
                }
                ConnectathonIntensional_1 vs = new ConnectathonIntensional_1(termOp, valueSetVersion, code, filter, offsetNo, countNo);
                valSet = vs.valueSet;
            }
            else if (identifier == "intensional-case-2" || identifier == "http://www.healthintersections.com.au/fhir/ValueSet/intensional-case-2")
            {
                if (!string.IsNullOrEmpty(codeSystem) && codeSystem != FhirSnomed.URI)
                {
                    valueSetVersion = "0.0";
                }
                ConnectathonIntensional_2 vs = new ConnectathonIntensional_2(termOp, valueSetVersion, code, filter, offsetNo, countNo);
                valSet = vs.valueSet;
            }
            else if (identifier == "intensional-case-3" || identifier == "http://www.healthintersections.com.au/fhir/ValueSet/intensional-case-3")
            {
                // All Snomed codes that are subsumed by 404684003 (Clinical Finding) - 
                if (termOp == TerminologyOperation.define_vs)
                {
                    string descrip = Utilities.StripSemanticTag(filter);
                    FhirSnomed vs = new FhirSnomed(termOp, "SCT-CLINICAL-FINDING", code, descrip, string.Empty, offsetNo, countNo);
                    valSet = vs.valueSet;
                }
                else if (termOp == TerminologyOperation.expand || termOp == TerminologyOperation.validate_code)
                {
                    if (!string.IsNullOrEmpty(codeSystem) && codeSystem != FhirSnomed.URI)
                    {
                        FhirSnomed vs = new FhirSnomed(termOp, "0.0", code, filter, string.Empty, offsetNo, countNo);
                        valSet = vs.valueSet;
                    }
                    else if (!string.IsNullOrEmpty(filter) || !string.IsNullOrEmpty(code))
                    {
                        FhirSnomed vs = new FhirSnomed(termOp, "SCT-CLINICAL-FINDING", code, filter, string.Empty, offsetNo, countNo);
                        valSet = vs.valueSet;
                    }
                    else
                    {
                        throw new Exception(TerminologyValueSet.MAX_VALUES_EXCEEDED);
                    }
                }
                valSet.Name = "Terminology Services Connectation #19 Intensional case #3";
                valSet.Id = "intensional-case-3";
                valSet.Identifier.Add(new Identifier { Value = "intensional-case-3" });
                valSet.Version = "C19";
                valSet.Url = "http://www.healthintersections.com.au/fhir/ValueSet/intensional-case-3";
                valSet.Status = PublicationStatus.Active;
            }
            else if (identifier == "SctIntensionalExpressionTest")
            {
                SctIntensionalExpressionTest vs = new SctIntensionalExpressionTest(termOp, valueSetVersion, code, filter, offsetNo, countNo);
                valSet = vs.valueSet;
            }
            else if (identifier.Contains("SCT-REFSET-NZ"))
            {
                identifier = identifier.Replace(ServerCapability.TERMINZ_CANONICAL + "/ValueSet/", "");
                FhirSnomed vs = new FhirSnomed(termOp, identifier, code, filter, valueSetVersion, offsetNo, countNo);
                valSet = vs.valueSet;
                requestedVersion = string.Empty;  // used to pass Reference Set here
            }
            else if (identifier.StartsWith("SCT-"))
            {
                FhirSnomed vs = new FhirSnomed(termOp, identifier, code, filter, string.Empty, offsetNo, countNo);
                valSet = vs.valueSet;
            }
            else if (identifier.StartsWith(FhirSnomed.URI) || codeSystem.StartsWith(FhirSnomed.URI))
            {
                string refsetID = string.Empty;

                // Look for subsumption test request and place superType code in filter e.g. =isa/2229800
                int isa = identifier.IndexOf("=isa/");

                if (isa > 0)
                {
                    filter = identifier.Substring(isa + 5);
                }

                // Look for Reference Set request and place code in filter e.g. =refset/450970008
                int refset = identifier.IndexOf("=refset/");

                if (refset > 0)
                {
                    refsetID = identifier.Substring(refset + 8);
                }
                else
                {
                    // look for request for details of ALL reference sets
                    refset = identifier.IndexOf("=refset");
                    if (refset > 0)
                    {
                        refsetID = FhirSnomed.SCT_NZ_EDITION_REFSETS;
                    }
                }

                // Look for ecl query
                int eclQuery = identifier.IndexOf("=ecl/");
                if (eclQuery > 0)
                {
                    filter = identifier.Substring(eclQuery);
                }

                // SNOMED-CT version may be specified in the codeSystem URL or identifier - e.g. http://snomed.info/sct/[sctid]/version/[YYYYMMDD] 
                string codeSystemVersion = string.Empty;
                if (codeSystem.IndexOf("/version/") > 0)
                {
                    codeSystemVersion = codeSystem.Replace("?fhir_vs", "");
                }
                else if (identifier.IndexOf("/version/") > 0)
                {
                    codeSystemVersion = identifier.Replace("?fhir_vs", "");
                }

                FhirSnomed vs = new FhirSnomed(termOp, codeSystemVersion, code, filter, refsetID, offsetNo, countNo);
                valSet = vs.valueSet;
            }
            else if (identifier.StartsWith("http://loinc.org/vs") || codeSystem == FhirLoinc.URI)
            {
                FhirLoinc vs = new FhirLoinc(termOp, valueSetVersion, code, filter, identifier, offsetNo, countNo);
                valSet = vs.valueSet;
            }
            else if (identifier == "NzEthnicityL1" || identifier == ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzEthnicityL1" || codeSystem == NzEthnicityL1.URI)
            {
                NzEthnicityL1 vs = new NzEthnicityL1(termOp, valueSetVersion, code, filter, offsetNo, countNo);
                valSet = vs.valueSet;
            }
            else if (identifier == "NzEthnicityL2" || identifier == ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzEthnicityL2" || codeSystem == NzEthnicityL2.URI)
            {
                NzEthnicityL2 vs = new NzEthnicityL2(termOp, valueSetVersion, code, filter, offsetNo, countNo);
                valSet = vs.valueSet;
            }
            else if (identifier == "NzEthnicityL3" || identifier == ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzEthnicityL3" || codeSystem == NzEthnicityL3.URI)
            {
                NzEthnicityL3 vs = new NzEthnicityL3(termOp, valueSetVersion, code, filter, offsetNo, countNo);
                valSet = vs.valueSet;
            }
            else if (identifier == "NzEthnicityL4" || identifier == ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzEthnicityL4" || codeSystem == NzEthnicityL4.URI)
            {
                NzEthnicityL4 vs = new NzEthnicityL4(termOp, valueSetVersion, code, filter, offsetNo, countNo);
                valSet = vs.valueSet;
            }
            else if (identifier == "NzRegion" || identifier == ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzRegion" || codeSystem == NzRegion.URI)
            {
                NzRegion vs = new NzRegion(termOp, valueSetVersion, code, filter, offsetNo, countNo);
                valSet = vs.valueSet;
            }
            else if (identifier.StartsWith("NZULM") || identifier.StartsWith(ServerCapability.TERMINZ_CANONICAL + "/ValueSet/Nzulm") || codeSystem == NzMt.URI)
            {
                NzMt vs = new NzMt(termOp, valueSetVersion, code, filter, identifier, offsetNo, countNo);
                valSet = vs.valueSet;
            }
            else if (identifier.ToUpper() == "UCUM-COMMON" || identifier == ServerCapability.HL7_FHIR_CANONICAL + "/ValueSet/ucum-common")
            {
                FhirXmlParser fxp = new FhirXmlParser();
                valSet = fxp.Parse<ValueSet>(File.ReadAllText(resourceFilePath + "UCUM-common.xml"));
                extensionalFromFile = true;
            }
            else if (identifier.ToUpper() == "BUNDLE-TYPE-SUPPLEMENTED" || identifier == ServerCapability.HL7_FHIR_CANONICAL + "/bundle-type-supplemented")
            {
                FhirXmlParser fxp = new FhirXmlParser();
                valSet = fxp.Parse<ValueSet>(File.ReadAllText(resourceFilePath + "BundleTypeSupplemented.xml"));
                extensionalFromFile = true;
            }

            if (extensionalFromFile)
            {
                if (termOp == TerminologyOperation.expand || termOp == TerminologyOperation.validate_code)
                {
                    ValueSet.ExpansionComponent es = ExpandExtensional(valSet);
                    valSet.Compose = null;
                    valSet = TerminologyValueSet.AddExpansion(valSet, es, offsetNo, countNo);
                }
            }

            if (!string.IsNullOrEmpty(requestedVersion) && valSet.Version != requestedVersion)
            {
                throw new Exception(UNSUPPORTED_VERSION);
            }

            if (String.IsNullOrEmpty(valSet.Id))
            {
                throw new Exception(TerminologyValueSet.UNFOUND_VALUESET);
            }

            return valSet;
        }
        
        private static string GetIdentifier(string id, NameValueCollection queryParam)
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

            if (string.IsNullOrEmpty(identifier))
            {
                identifier = Utilities.GetQueryValue("url", queryParam); ;
            }

            return identifier;
        }

        internal static bool MatchValue(string code, string displayValue, string codeFilter, string descFilter)
        {
            bool match = false;
            
            // remove semantic tag from description filter as Preferred Term will be returned
            string descrip = Utilities.StripSemanticTag(descFilter);

            if (string.IsNullOrEmpty(descrip) || displayValue.ToLower().Contains(descrip.ToLower()))
            {
                if (string.IsNullOrEmpty(codeFilter) || code == codeFilter)
                {
                    match = true;
                }
            }

            return match;
        }

        internal static ValueSet.ExpansionComponent ExpandExtensional(ValueSet valSet)
        {
            ValueSet.ExpansionComponent es = new ValueSet.ExpansionComponent();

            foreach (ValueSet.ConceptSetComponent comp in valSet.Compose.Include)
            {
                foreach (ValueSet.ConceptReferenceComponent crc in comp.Concept)
                {
                    es.Contains.Add(new ValueSet.ContainsComponent { Code = crc.Code, Display = crc.Display, System = comp.System, Designation = crc.Designation });
                }
                
            }
            return es;
        }
        internal static ValueSet AddExpansion(ValueSet valSet, ValueSet.ExpansionComponent es, int offsetNo, int countNo)
        {
            // add expansion

            int expTotal = es.Contains.Count;
            valSet.Expansion = es;
            valSet.Expansion.Total = expTotal;
            valSet.Expansion.Offset = Math.Max(0, offsetNo);
            valSet.Expansion.Timestamp = DateTime.Now.ToString("yyyy-MM-dd");
            valSet.Expansion.Identifier = "urn:uuid:" + System.Guid.NewGuid().ToString();

            if (countNo > 0)
            {
                ValueSet.ParameterComponent vpc1 = new ValueSet.ParameterComponent { Name = "count", Value = new Integer(countNo) };
                valSet.Expansion.Parameter.Add(vpc1);
            }

            ValueSet.ParameterComponent vpc2 = new ValueSet.ParameterComponent { Name = "version", Value = new FhirUri(valSet.Version) };
            valSet.Expansion.Parameter.Add(vpc2);

            ValueSet.ParameterComponent vpc3 = new ValueSet.ParameterComponent { Name = "activeOnly", Value = new FhirBoolean(true) };
            valSet.Expansion.Parameter.Add(vpc3);

            // handle paging requests

            if (countNo == 0)  //count only requested
            {
                valSet.Expansion.Contains.Clear();
            }
            else if (countNo > 0 && offsetNo >= 0)
            {
                // return nothing if offset beyond bounds (expansion total and offset returned so client can see why this is)
                if (offsetNo >= expTotal)
                {
                    valSet.Expansion.Contains.Clear();
                }
                else
                {
                    // if offset ok, adjust out of bounds counts
                    if (offsetNo + countNo >= expTotal)
                    {
                        countNo = expTotal - offsetNo;
                    }
                    valSet.Expansion.Contains = es.Contains.GetRange(offsetNo, countNo);
                }
            }

            return valSet;
        }

        private static ValueSet GenerateExpansionNarrative(ValueSet valSet, int countNo)
        {
            // create display text for Expanded ValueSet Resource

            XNamespace ns = "http://www.w3.org/1999/xhtml";

            var summary = new XElement(ns + "div",
                new XElement(ns + "h2", valSet.Name),
                new XElement(ns + "p", valSet.Description),
                new XElement(ns + "table",
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Id"),
                    new XElement(ns + "td", valSet.Id)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Url"),
                    new XElement(ns + "td", valSet.Url)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Version"),
                    new XElement(ns + "td", valSet.Version)
                    ),
                     new XElement(ns + "tr",
                    new XElement(ns + "td", "Publisher"),
                    new XElement(ns + "td", valSet.Publisher)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Copyright"),
                    new XElement(ns + "td", valSet.Copyright)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Status"),
                    new XElement(ns + "td", valSet.Status.ToString())
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Status Date"),
                    new XElement(ns + "td", valSet.Date)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Experimental?"),
                    new XElement(ns + "td", valSet.Experimental.ToString())
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Expansion Total"),
                    new XElement(ns + "td", valSet.Expansion.Total.ToString())
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Expansion Offset/Count"),
                    new XElement(ns + "td", valSet.Expansion.Offset.ToString() + "/" + countNo.ToString())
                    )
                 ),
                new XElement(ns + "table",
                    new XElement(ns + "tr",
                    new XElement(ns + "th", "System"),
                    new XElement(ns + "th", "Code"),
                    new XElement(ns + "th", "Display")
                    ),
                    from exp in valSet.Expansion.Contains
                    select new XElement(ns + "tr",
                        new XElement(ns + "td", exp.System),
                        new XElement(ns + "td", exp.Code),
                        new XElement(ns + "td", exp.Display)
                    )
                 )
              );

            valSet.Text = new Narrative();
            valSet.Text.Status = Narrative.NarrativeStatus.Generated;
            valSet.Text.Div = summary.ToString();

            return valSet;
        }

        private static ValueSet GenerateCompositionNarrative(ValueSet valSet)
        {
            // create display text for Composition (i.e. definition) of a ValueSet Resource

            int codesReturned = 0;
            string filters = string.Empty;
            string codeSystems = string.Empty;

            List<ValueSet.ConceptReferenceComponent> conceptList = new List<ValueSet.ConceptReferenceComponent>();

            try
            {
                foreach (ValueSet.ConceptSetComponent comp in valSet.Compose.Include)
                {
                    string ver = (string.IsNullOrEmpty(comp.Version)) ? "?" : comp.Version;
                    string cs = comp.System + " (version " + ver + ")";

                    if (codeSystems.IndexOf(cs) < 0)
                    {
                        codeSystems += cs + " : ";
                    }

                    foreach (ValueSet.FilterComponent fil in comp.Filter)
                    {
                        filters += "(" + fil.Property + " " + fil.Op.Value.ToString() + " " + fil.Value + ") : ";
                    }

                    foreach (ValueSet.ConceptReferenceComponent crc in comp.Concept)
                    {
                        // place Code System in userdata to flatten structure for table rendering
                        codesReturned++;
                        conceptList.Add(crc);
                    }
                }
            }
            catch
            {
                //throw new Exception(UNFOUND_VALUESET);
            }

            XNamespace ns = "http://www.w3.org/1999/xhtml";

            var summary = new XElement(ns + "div",
                new XElement(ns + "h2", valSet.Name),
                new XElement(ns + "p", valSet.Description),
                new XElement(ns + "table",
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Id"),
                    new XElement(ns + "td", valSet.Id)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Url"),
                    new XElement(ns + "td", valSet.Url)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Version"),
                    new XElement(ns + "td", valSet.Version)
                    ),
                     new XElement(ns + "tr",
                    new XElement(ns + "td", "Publisher"),
                    new XElement(ns + "td", valSet.Publisher)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Copyright"),
                    new XElement(ns + "td", valSet.Copyright)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Status"),
                    new XElement(ns + "td", valSet.Status.ToString())
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Status Date"),
                    new XElement(ns + "td", valSet.Date)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Experimental"),
                    new XElement(ns + "td", valSet.Experimental.ToString())
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Code System(s)"),
                    new XElement(ns + "td", codeSystems)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Filter(s)"),
                    new XElement(ns + "td", filters)
                    )
                ),
                RenderConceptList(conceptList, ns)
              );

            valSet.Text = new Narrative();
            valSet.Text.Status = Narrative.NarrativeStatus.Generated;
            valSet.Text.Div = summary.ToString();

            return valSet;
        }

        private static XElement RenderConceptList( List<ValueSet.ConceptReferenceComponent> conceptList, XNamespace ns)
        {
            XElement xConceptList = null;

            if (conceptList.Count > 0)
            {
                xConceptList = new XElement(ns + "table",
                    new XElement(ns + "tr",
                    new XElement(ns + "th", "Code"),
                    new XElement(ns + "th", "Display")
                    ),
                    from conc in conceptList
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