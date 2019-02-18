namespace Vintage.AppServices.BusinessClasses.FHIR.CodeSystems
{
    using Hl7.Fhir.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Vintage.AppServices.DataAccessClasses;

    /// <summary>
    ///  FHIR SNOMED CT Value Sets & underlying Code System
    /// </summary>

    public class FhirSnomed
    {

        // create dictionary of root concepts
        internal static Dictionary<string, string> rootConcepts = new Dictionary<string, string>();

        public const string NAME = "SNOMED Clinical Terms: New Zealand Edition";
        public const string DESCRIPTION = "Systematized Nomenclature of Medicine -- Clinical Terms";
        public const string URI = "http://snomed.info/sct";
        //"SNOMED CT NZ Beta Edition : Feb 01, 2019 Version"
        public const string CURRENT_VERSION = "http://snomed.info/sct/21000210109/version/20190201";
        public const string SHORT_VERSION = "http://snomed.info/sct/version/201990201";
        //public const string CURRENT_VERSION = "http://snomed.info/sct/900000000000207008/version/20190131";
        public const string SCT_ROOT_CONCEPT = "138875005";
        public const string SCT_ROOT_DESCRIPTION = "SNOMED CT Concept (SNOMED RT+CTV3)";
        public const string SCT_ATTRIBUTE_CONCEPT = "246061005";
        public const string SCT_NZ_EDITION_REFSETS = "NZ_EDITION_ALL";
        public const string NZ_PATIENT_FRIENDLY_TERMS = "281000210109";

        public CodeSystem codeSystem { get; set; }
        public ValueSet valueSet { get; set; }

        public FhirSnomed()
        {
            this.FillValues(TerminologyOperation.define_vs, string.Empty, string.Empty, string.Empty, string.Empty, -1, -1, string.Empty);
        }

        public FhirSnomed(TerminologyOperation termOp, string version, string code, string filter, string refsetId, int offsetNo, int countNo, string useContext)
        {
            this.FillValues(termOp, version, code, filter, refsetId, offsetNo, countNo, useContext);
        }

        internal static string GetNormalFormDisplay(string codeVal, bool fullDisplay)
        {
            string retValue = string.Empty;

            // get proximal primitive(s) and defining attributes
            List<Coding> proximalPrimitiveCodeVals = SnomedCtSearch.GetProximalPrimitives(codeVal, fullDisplay);
            List<Coding> attributeCodeVals = SnomedCtSearch.GetAttributeExpressions(codeVal, fullDisplay);

            // add focus concept(s)
            int ppCount = 0;
            foreach (Coding pp in proximalPrimitiveCodeVals)
            {
                retValue += ( (ppCount > 0) ? "+" : "") + pp.Display;
                ppCount++;
            }

            // add refinement indicator if there is at least one attribute
            retValue += (attributeCodeVals.Count > 0) ? ":" : "";

            int attCount = 0;
            string relGroup = string.Empty;
            bool bracesRequired = false;
            bool newGroup = false;

           // add refinements
            foreach (Coding att in attributeCodeVals)
            {

                newGroup = (att.Code != relGroup);

                // see if refinement attribute grouping characters are required (always for ungrouped=0)
                if (newGroup || relGroup == "0")
                {
                    // closing braces required for previous group ?
                    retValue += (bracesRequired) ? "}" : "";
                    // NB should be no comma between ungrouped and grouped attributes (TIG page 107)
                    retValue += (bracesRequired && relGroup != "0") ? "," : string.Empty;
                    // braces required for this group (not if only 1 pair in a group, or ungrouped=0)
                    attCount = attributeCodeVals.Count(ac => ac.Code.Equals(att.Code));
                    bracesRequired = (attCount > 1 || att.Code == "0");
                    // opening braces for new group
                    retValue += (bracesRequired) ? "{" : string.Empty;
                    attCount = 0;
                }

                // is the attribute value a fully defined concept requiring expansion?
                int fdcIndictor = att.Display.IndexOf("=*");
                if (fdcIndictor > 0)
                {
                    // add nested refinement
                    string att1 = att.Display.Substring(0, fdcIndictor) + "=(";
                    string avCode = att.Display.Substring(fdcIndictor + 2);
                    if (fullDisplay)
                    {
                        avCode = avCode.Substring(0, avCode.IndexOf("|"));
                    }
                    att.Display = att1 + GetNormalFormDisplay(avCode, fullDisplay) + ")";
                }

                retValue += ((attCount > 0) ? "," : "") + att.Display;
                relGroup = att.Code;
                attCount++;
            }

            retValue += (bracesRequired) ? "}" : "";

            return retValue;
        }

        private void FillValues(TerminologyOperation termOp, string version, string code, string filter, string refsetId, int offsetNo, int countNo, string useContext)
        {

            bool refset = !string.IsNullOrEmpty(refsetId);

            if (version.StartsWith("SCT-REFSET-NZ"))
            {
                refset = true;
                refsetId = GetNzRefSetId(version);
            }

            SetRootConcepts();

            // detect ECL query from filter
            bool ecl = filter.StartsWith("=ecl/");

            // detect subsumption request from filter & refset indicator
            bool subsumption = Utilities.IsDigitsOnly(filter) && !refset && !ecl;

            string description = "All SNOMED CT Codes Filtered By: " + filter;
            string name = "SNOMED CT";

            if (refset)
            {
                description = "All SNOMED CT codes in the NZ Reference Set: " + refsetId;
                name = "SNOMED CT NZ Reference Set: " + refsetId;
            }
            else if (subsumption)
            {
                description = "All SNOMED CT codes Subsumed By: " + filter;
                name = "SNOMED CT Concepts Subsumed By: " + filter;
            }
            else if (ecl)
            {
                filter = filter.Replace("=ecl/", "");
                description = "All SNOMED CT codes Filtered By Expression: " + filter;
                name = "SNOMED CT Concepts Filtered By Expression: " + filter;
            }

            // Version will contain ValueSet Identifier if a Value Set constrained to a Root Concept has been requested
            bool subset = false;
            string superTypeCode = string.Empty;

            if (version.StartsWith("SCT-") && !refset)
            {
                string hName = version.Replace("SCT-", "").Replace("-"," ").ToUpper();

                foreach (var rc in rootConcepts)
                {
                    if (rc.Value.ToUpper().StartsWith(hName))
                    {
                        superTypeCode = rc.Key;
                        subset = true;
                        description = "All SNOMED CT codes From " + version.Replace("SCT-", "") + " Content Hierarchy: filtered by " + filter;
                        name = version;
                        break;
                    }
                }
                subset = true;
            }

            this.valueSet = new ValueSet();
            this.codeSystem = new CodeSystem();

            if (refset)
            {
                if (version.StartsWith("SCT-REFSET-NZ"))
                {
                    this.valueSet.Id = version;
                }
                else
                {
                    this.valueSet.Id = "fhir_sct_vs_" + refsetId + (!string.IsNullOrEmpty(filter) ? "_" + filter.Replace(" ", "_") : "");
                }
            }
            else if (ecl)
            {
                this.valueSet.Id = "fhir_sct_vs_ecl";
            }
            else
            {
                this.valueSet.Id = "fhir_sct_vs_" + filter.Replace(" ", "_");
            }

            this.codeSystem.Id = "SNOMEDCT";
            this.codeSystem.CaseSensitive = false;
            this.codeSystem.Content = CodeSystem.CodeSystemContentMode.NotPresent;
            this.codeSystem.Experimental = false;
            this.codeSystem.Compositional = true;
            this.codeSystem.VersionNeeded = false;
            this.codeSystem.HierarchyMeaning = CodeSystem.CodeSystemHierarchyMeaning.IsA;

            // Code System filters

            List<FilterOperator?> fops = new List<FilterOperator?>();
            fops.Add(FilterOperator.IsA);
            this.codeSystem.Filter.Add(new CodeSystem.FilterComponent { Code = "concept", Description = "Includes all concept ids that have a transitive is-a relationship with the code provided as the value.", Value = "A SNOMED CT concept id", Operator = fops.AsEnumerable() });

            fops.Clear();
            fops.Add(FilterOperator.In);
            this.codeSystem.Filter.Add(new CodeSystem.FilterComponent { Code = "concept", Description = "Includes all concept ids that are active members of the reference set identified by the concept id provided as the value.", Value = "A SNOMED CT concept id", Operator = fops.AsEnumerable() });

            fops.Clear();
            fops.Add(FilterOperator.Equal);
            this.codeSystem.Filter.Add(new CodeSystem.FilterComponent { Code = "expression", Description = "The result of the filter is the result of executing the given SNOMED CT Expression Constraint.", Value = "A SNOMED CT concept id", Operator = fops.AsEnumerable() });

            // Code System properties
            this.codeSystem.Property.Add(new CodeSystem.PropertyComponent { Code = "parent", Description = "The SNOMED CT concept id that is a direct parent of the concept.", Type = CodeSystem.PropertyType.Code });
            this.codeSystem.Property.Add(new CodeSystem.PropertyComponent { Code = "child", Description = "The SNOMED CT concept id that is a direct child of the concept.", Type = CodeSystem.PropertyType.Code });
            this.codeSystem.Property.Add(new CodeSystem.PropertyComponent { Code = "sufficientlyDefined", Description = "True if the description logic definition of the concept includes sufficient conditions (i.e., if the concept is not primitive).", Type = CodeSystem.PropertyType.Boolean });
            this.codeSystem.Property.Add(new CodeSystem.PropertyComponent { Code = "inactive", Description = "Whether the code is active or not (defaults to false). This is derived from the active column in the Concept file of the RF2 Distribution (by inverting the value)", Type = CodeSystem.PropertyType.Boolean });
            this.codeSystem.Property.Add(new CodeSystem.PropertyComponent { Code = "moduleId", Description = "The SNOMED CT concept id of the module that the concept belongs to.", Type = CodeSystem.PropertyType.Code });
            this.codeSystem.Property.Add(new CodeSystem.PropertyComponent { Code = "normalForm", Description = "Generated Normal form expression for the provided code or expression, with terms", Type = CodeSystem.PropertyType.String });
            this.codeSystem.Property.Add(new CodeSystem.PropertyComponent { Code = "normalFormTerse", Description = "Generated Normal form expression for the provided code or expression, conceptIds only", Type = CodeSystem.PropertyType.String });
            this.codeSystem.Property.Add(new CodeSystem.PropertyComponent { Code = FhirSnomed.SCT_ATTRIBUTE_CONCEPT, Description = "Any SNOMED CT relationships where the relationship type is subsumed by Attribute (246061005).", Type = CodeSystem.PropertyType.Code });

            if (refset && version.StartsWith("SCT-REFSET-NZ"))
            {
                this.valueSet.Url = ServerCapability.TERMINZ_CANONICAL + "/ValueSet/" + version;
                version = string.Empty;
            }
            else
            {
                this.valueSet.Url = ServerCapability.TERMINZ_CANONICAL + "/ValueSet/sct/" + valueSet.Id;
            }

            this.codeSystem.Url = FhirSnomed.URI;

            this.codeSystem.ValueSet = "http://snomed.info/sct?fhir_vs";

            this.valueSet.Name = name ;
            this.codeSystem.Name = FhirSnomed.NAME;

            this.valueSet.Description = new Markdown(description);
            this.codeSystem.Description = new Markdown(FhirSnomed.DESCRIPTION);

            this.valueSet.Version = "http://snomed.info/sct?version=" + FhirSnomed.CURRENT_VERSION;
            this.codeSystem.Version = FhirSnomed.CURRENT_VERSION;
            string csShortVer = FhirSnomed.SHORT_VERSION;
            
            this.valueSet.Experimental = true;

            this.valueSet.Status = PublicationStatus.Active;
            this.codeSystem.Status = PublicationStatus.Draft;

            this.valueSet.Date = Hl7.Fhir.Model.Date.Today().Value;
            this.codeSystem.Date = Hl7.Fhir.Model.Date.Today().Value;

            this.valueSet.Publisher = "Patients First Ltd";
            this.codeSystem.Publisher = "SNOMED International";

            this.codeSystem.Copyright = new Markdown("copyright © 2002+ International Health Terminology Standards Development Organisation (IHTSDO) (trading as SNOMED International)");
            this.valueSet.Copyright = this.codeSystem.Copyright;

            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "peter.jordan@patientsfirst.org.nz" };
            ContactDetail cd = new ContactDetail();
            cd.Telecom.Add(cp);
            this.valueSet.Contact.Add(cd);
            this.codeSystem.Contact.Add(cd);

            ValueSet.ConceptSetComponent cs = new ValueSet.ConceptSetComponent();
            ValueSet.ExpansionComponent es = new ValueSet.ExpansionComponent();

            cs.System = this.codeSystem.Url;
            cs.Version = this.codeSystem.Version;

            if (version.StartsWith("SCT-") && !refset)
            {
                cs.Filter.Add(new ValueSet.FilterComponent { Property = "code", Op = FilterOperator.IsA, Value = superTypeCode });
            }

            string codeCode = string.Empty;
            string codeDisplay = string.Empty;
            string codeDefinition = string.Empty;

            if ( (string.IsNullOrEmpty(version) || version == cs.Version || version == csShortVer || subset) && termOp != TerminologyOperation.define_cs)
            {
                List<Coding> codeVals = new List<Coding>();

                if (termOp != TerminologyOperation.define_vs)
                {
                    if (subset && !string.IsNullOrEmpty(filter))
                    {
                        codeVals = SnomedCtSearch.GetConceptsFromHierarchyByTerm(superTypeCode, Utilities.StripSemanticTag(filter.Trim()));
                    }
                    else if (subset && !string.IsNullOrEmpty(code))
                    {
                        if (termOp == TerminologyOperation.validate_code)
                        {
                            if ((bool)SnomedCtSearch.IsSubsumedBy(code, superTypeCode))
                            {
                                codeVals = SnomedCtSearch.GetConceptByCode(code);
                            }
                        }
                        else
                        {
                            codeVals = SnomedCtSearch.GetSubsumedCodes(code,true);
                        }
                    }
                    else if (refset)
                    {
                        if (refsetId == SCT_NZ_EDITION_REFSETS)
                        {
                            codeVals = SnomedCtSearch.GetNzRefSets();
                        }
                        else
                        {
                            codeVals = SnomedCtSearch.GetRefSet(refsetId, filter);
                            try
                            {
                                valueSet.Name = SnomedCtSearch.GetConceptByCode(refsetId)[0].Display;
                            }
                            catch { }
                        }
                    }
                    else if (subsumption)
                    {
                        // prevent subsumptions on root concepts...
                        if (rootConcepts.ContainsKey(filter))
                        {
                            throw new Exception(TerminologyValueSet.MAX_VALUES_EXCEEDED);
                        }
                        codeVals = SnomedCtSearch.GetSubsumedCodes(filter,true);
                    }
                    else if (!string.IsNullOrEmpty(code))
                    {
                        // returning Parameters Resource with False appears to be preferred to OperationOutcome failure
                        //if (!Utilities.IsDigitsOnly(code))
                        //{
                        //    throw new Exception(TerminologyValueSet.INVALID_CODE);
                        //}
                        if (termOp == TerminologyOperation.validate_code)
                        {
                            codeVals = SnomedCtSearch.GetConceptDesignationsByCode(code);
                        }
                        else
                        {
                            codeVals = SnomedCtSearch.GetConceptByCode(code);
                        }
                    }
                    else if (ecl)
                    {
                        codeVals = EclHandler.ExecuteEclQuery(filter);   
                    }
                    else if (!string.IsNullOrEmpty(filter))
                    {
                        if (filter.Length < 3)
                        {
                            throw new Exception(TerminologyValueSet.INVALID_FILTER);
                        }
                        codeVals = SnomedCtSearch.GetConceptsByTerm(Utilities.StripSemanticTag(filter.Trim()));
                    }
                    else if (countNo > 0)
                    {
                        // just a random set of concepts
                        codeVals = SnomedCtSearch.GetConceptsByTerm("fire");
                        this.valueSet.Description = new Markdown("Random Set of SNOMED CT Codes Filtered By: 'fire'");
                        this.valueSet.Id = "fhir_sct_vs_fire";
                        this.valueSet.Url += "fire";
                    }
                    else // can't pass SNOMED CT in its entirety!
                    {
                        throw new Exception(TerminologyValueSet.MISSING_FILTER);
                    }

                    if (codeVals.Count > 9999)
                    {
                        throw new Exception(TerminologyValueSet.MAX_VALUES_EXCEEDED);
                    }
                }

                // NZ patient-friendly terms
                if (useContext == NZ_PATIENT_FRIENDLY_TERMS && codeVals.Count > 0)
                {
                    List<Coding> pfterms = SnomedCtSearch.GetNzEnPatientFriendlyTerms();
                    pfterms.RemoveAll(x => !codeVals.Select(y => y.Code).Contains(x.Code));
                    if (pfterms.Count > 0)
                    {
                        codeVals.RemoveAll(x => pfterms.Select(y => y.Code).Contains(x.Code));
                        codeVals.InsertRange(0, pfterms);
                    }
                }

                // filtering performed at DB Layer, so add all returned concepts
                foreach (Coding codeVal in codeVals)
                {
                    cs.Concept.Add(new ValueSet.ConceptReferenceComponent { Code = codeVal.Code, Display = codeVal.Display});
                    es.Contains.Add(new ValueSet.ContainsComponent { Code = codeVal.Code, Display = codeVal.Display, System = cs.System });
                    this.codeSystem.Concept.Add(new CodeSystem.ConceptDefinitionComponent { Code = codeVal.Code, Display = codeVal.Display, Definition = codeVal.System });
                }

                if (termOp == TerminologyOperation.expand || termOp == TerminologyOperation.validate_code || termOp == TerminologyOperation.subsumes)
                {
                    this.valueSet = TerminologyValueSet.AddExpansion(this.valueSet, es, offsetNo, countNo);
                }
                else if (termOp == TerminologyOperation.define_vs)
                {
                    this.valueSet.Compose = new ValueSet.ComposeComponent();
                    this.valueSet.Compose.Include.Add(cs);
                }

            }

        }

        internal static void SetRootConcepts()
        {
            // populate dictionary of root concepts
            rootConcepts = new Dictionary<string, string>();
            rootConcepts.Add(SCT_ROOT_CONCEPT, SCT_ROOT_DESCRIPTION);
            rootConcepts.Add("123037004", "Body structure (body structure)");
            rootConcepts.Add("404684003", "Clinical finding (finding)");
            rootConcepts.Add("308916002", "Environment or geographical location (environment / location)");
            rootConcepts.Add("272379006", "Event (event)");
            rootConcepts.Add("363787002", "Observable entity (observable entity)");
            rootConcepts.Add("410607006", "Organism (organism)");
            rootConcepts.Add("373873005", "Pharmaceutical / biologic product (product)");
            rootConcepts.Add("78621006", "Physical force (physical force)");
            rootConcepts.Add("260787004", "Physical object (physical object)");
            rootConcepts.Add("71388002", "Procedure (procedure)");
            rootConcepts.Add("362981000", "Qualifier value (qualifier value)");
            rootConcepts.Add("419891008", "Record artifact (record artifact)");
            rootConcepts.Add("243796009", "Situation with explicit context (situation)");
            rootConcepts.Add("900000000000441003", "SNOMED CT Model Component (metadata)");
            rootConcepts.Add("48176007", "Social context (social concept)");
            rootConcepts.Add("370115009", "Special concept (special concept)");
            rootConcepts.Add("3038009", "Specimen (specimen)");
            rootConcepts.Add("254291000", "Staging and scales (staging scale)");
            rootConcepts.Add("105590001", "Substance (substance)");
        }

        internal static string GetDesignationTypeId(string designation)
        {
            string desigTypeId = "900000000000549004";  // acceptable synonym

            if (designation.ToUpper() == "FULLY SPECIFIED NAME")
            {
                desigTypeId = "900000000000003001";
            }
            else if (designation.ToUpper() == "PREFERRED TERM")
            {
                desigTypeId = "900000000000548007";
            }

            return desigTypeId;
        }

        internal static bool ValidateCode(string code, string description, out string prefTerm)
        {
            bool retValue = false;
            prefTerm = string.Empty;

            List<Coding> codeVals = SnomedCtSearch.GetConceptDesignationsByCode(code);

            if (codeVals.Count > 1)
            {
                if (string.IsNullOrEmpty(description))
                {
                    retValue = true;
                }

                foreach (Coding concept in codeVals)
                {
                    if (!retValue)
                    {
                        if (concept.Version == "900000000000448009") // Entire term case insensitive
                        {
                            retValue = (description.Trim().ToLower() == concept.Display.Trim().ToLower());
                        }
                        else if (concept.Version == "900000000000017005") // Entire term case sensitive
                        {
                            retValue = (description.Trim() == concept.Display.Trim());
                        }
                        else if (concept.Version == "900000000000020002") // Only initial character case insensitive
                        {
                            retValue = (description.Substring(0, 1) == concept.Display.Substring(0, 1) && description.Trim().ToLower() == concept.Display.Trim().ToLower());
                        }
                    }

                    if (concept.System.ToUpper() == "PREFERRED TERM")
                    {
                        prefTerm = concept.Display;
                    }
                }
            }

            return retValue;
        }

        internal static bool IsValidURI(string sctUrl)
        {
            bool retVal = (sctUrl == URI || sctUrl == CURRENT_VERSION || sctUrl == SHORT_VERSION);
            return retVal;
        }

        internal static TerminologyCapabilities.CodeSystemComponent GetCapabilities()
        {
            TerminologyCapabilities.CodeSystemComponent csc = new TerminologyCapabilities.CodeSystemComponent();
            csc.Uri = FhirSnomed.URI;

            TerminologyCapabilities.VersionComponent vc = new TerminologyCapabilities.VersionComponent();
            vc.Code = FhirSnomed.CURRENT_VERSION;
            vc.IsDefault = true;
            vc.Compositional = true;
            vc.LanguageElement.Add(new Code("en-NZ"));

            TerminologyCapabilities.FilterComponent filt_1 = new TerminologyCapabilities.FilterComponent();
            filt_1.CodeElement = new Code("is-a");
            filt_1.OpElement.Add(new Code("CodeSystem-subsumes"));
            filt_1.OpElement.Add(new Code("ValueSet-expand"));
            vc.Filter.Add(filt_1);

            TerminologyCapabilities.FilterComponent filt_2 = new TerminologyCapabilities.FilterComponent();
            filt_2.CodeElement = new Code("in");
            filt_2.OpElement.Add(new Code("ValueSet-expand"));
            vc.Filter.Add(filt_2);

            TerminologyCapabilities.FilterComponent filt_3 = new TerminologyCapabilities.FilterComponent();
            filt_3.CodeElement = new Code("ecl");
            filt_3.OpElement.Add(new Code("ValueSet-expand"));
            vc.Filter.Add(filt_3);

            vc.PropertyElement.Add(new Code("code"));
            vc.PropertyElement.Add(new Code("system"));
            vc.PropertyElement.Add(new Code("version"));
            vc.PropertyElement.Add(new Code("definition"));
            vc.PropertyElement.Add(new Code("designation"));
            vc.PropertyElement.Add(new Code("parent"));
            vc.PropertyElement.Add(new Code("child"));
            vc.PropertyElement.Add(new Code("inactive"));
            vc.PropertyElement.Add(new Code("sufficientlyDefined"));
            vc.PropertyElement.Add(new Code("moduleId"));
            vc.PropertyElement.Add(new Code("normalForm"));
            vc.PropertyElement.Add(new Code("normalFormTerse"));
            vc.PropertyElement.Add(new Code(FhirSnomed.SCT_ATTRIBUTE_CONCEPT));

            csc.Version.Add(vc);

            return csc;
        }

        internal static string GetNzRefSetId(string refSetName)
        {
            string refSetId = string.Empty;

            if (refSetName == "SCT-REFSET-NZ-CARDIOLOGY") { refSetId = "91000210107"; }

            else if (refSetName == "SCT-REFSET-NZ-DISABILITY") { refSetId = "261000210101"; }

            else if (refSetName == "SCT-REFSET-NZ-EC-DIAGNOSIS") { refSetId = "61000210102"; }

            else if (refSetName == "SCT-REFSET-NZ-EC-PRESENTING-COMPLAINT") { refSetId = "71000210108"; }

            else if (refSetName == "SCT-REFSET-NZ-EC-PROCEDURE") { refSetId = "321000210102"; }

            else if (refSetName == "SCT-REFSET-NZ-GATEWAY-CHILD-HEALTH") { refSetId = "241000210102"; }

            else if (refSetName == "SCT-REFSET-NZ-GYNAECOLOGY") { refSetId = "101000210108"; }

            else if (refSetName == "SCT-REFSET-NZ-ACC-TRANSLATION-TABLE") { refSetId = "81000210105"; }

            else if (refSetName == "SCT-REFSET-NZ-NOTIFIABLE-DISEASE") { refSetId = "251000210104"; }

            else if (refSetName == "SCT-REFSET-NZ-RHEUMATOLOGY") { refSetId = "121000210100"; }

            else if (refSetName == "SCT-REFSET-NZ-SMOKING") { refSetId = "51000210100"; }

            else if (refSetName == "SCT-REFSET-NZ-ADVERSE-REACTION-MANIFESTATION") { refSetId = "351000210106"; }

            else if (refSetName == "SCT-REFSET-NZ-AMBULANCE-CLINICAL-IMPRESSION") { refSetId = "421000210109"; }

            else if (refSetName == "SCT-REFSET-NZ-MICROORGANISM") { refSetId = "391000210104"; }

            else if (refSetName == "SCT-REFSET-NZ-ENDOCRINOLOGY") { refSetId = "141000210106"; }
            
            else if (refSetName == "SCT-REFSET-NZ-CHILD-DEVELOPMENTAL-SERVCES") { refSetId = "191000210102"; }

            else if (refSetName == "SCT-REFSET-NZ-CLINICAL-SPECIALTY") { refSetId = "471000210108"; }

            else if (refSetName == "SCT-REFSET-NZ-GENERAL-PAEDIATRIC") { refSetId = "181000210104"; }

            else if (refSetName == "SCT-REFSET-NZ-GENERAL-SURGERY") { refSetId = "131000210103"; }

            else if (refSetName == "SCT-REFSET-NZ-HEALTH-OCCUPATION") { refSetId = "451000210100"; }

            else if (refSetName == "SCT-REFSET-NZ-HEALTH-SERVICE") { refSetId = "461000210102"; }
            
            return refSetId;
        }
    }

    public class EclHandler
    {
        internal static List<Coding> ExecuteEclQuery(string eclQuery)
        {
            List<Coding> codeVals = new List<Coding>();

            try
            {
                eclQuery = Uri.UnescapeDataString(eclQuery); // just in case
                string focus = GetEclSection(eclQuery, true);
                bool anyFocus = (focus.StartsWith("ANY"));
                string refine = GetEclSection(eclQuery, false);
                bool refined = !string.IsNullOrEmpty(refine);
                List<EclElement> focusElements = GetEclFocusElements(focus);
                codeVals = GetEclFocusConcepts(focusElements, refined);

                if (refined)
                {
                    List<EclRefinement> refinements = GetEclRefinements(refine);
                    codeVals = ApplyEclRefinements(refinements, codeVals, anyFocus);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == TerminologyValueSet.INVALID_EXPRESSION || ex.Message == TerminologyValueSet.UNSUPPORTED_EXPRESSION)
                {
                    throw ex;
                }
                else
                {
                    // for debugging
                    throw new Exception(ex.Message + " for ECL Query " + eclQuery);
                }
            }

            return codeVals;
        }

        internal static string EclExpressionFormatter(string ecQuery)
        {
            // remove white space 
            // BUT Whitespace within the quotation marks of a concrete value is treated as a significant character.
            string formattedEc = ecQuery; //.Replace(" ", string.Empty);

            // remove terms
            formattedEc = Regex.Replace(formattedEc, "\\|.*?\\|", string.Empty);

            // remove comments
            formattedEc = Regex.Replace(formattedEc, @"/\*(.*?)\*/", string.Empty);

            // convert remaining text to upper case
            formattedEc = formattedEc.ToUpper();

            // identify unsupported expression types (nesting within focus concept)
            if (formattedEc.Count(f => f == '(') > 1)
            {
                throw new Exception(TerminologyValueSet.UNSUPPORTED_EXPRESSION);
            }

            // identify unsupported expression types (>2 chained dot notations)
            if (formattedEc.Count(f => f == '.') > 2)
            {
                throw new Exception(TerminologyValueSet.UNSUPPORTED_EXPRESSION);
            }

            //convert Unary Operator symbols to text versions
            // <TODO> < and > can be used as mathematical against concrete values in a refinement
            formattedEc = formattedEc.Replace(">!", "PARENTOF ");
            formattedEc = formattedEc.Replace(">>", "ANCESTORORSELFOF ");
            formattedEc = formattedEc.Replace(">", "ANCESTOROF ");
            formattedEc = formattedEc.Replace("<!", "CHILDRENOF ");
            formattedEc = formattedEc.Replace("<<", "DESCENDANTORSELFOF ");
            formattedEc = formattedEc.Replace("<", "DESCENDANTOF ");
            formattedEc = formattedEc.Replace("^", "MEMBEROF ");
            formattedEc = formattedEc.Replace("*", "ANY ");
            formattedEc = formattedEc.Replace(" R", "REVERSEOF ");

            //convert Mathematical Operator symbols to text versions
            formattedEc = formattedEc.Replace("=", "EQUALS ");
            formattedEc = formattedEc.Replace("!=", "NOTEQUALS ");
            formattedEc = formattedEc.Replace("<", "LESSTHAN ");
            formattedEc = formattedEc.Replace("<=", "LESSTHANOREQUALS ");
            formattedEc = formattedEc.Replace(">", "GREATERTHAN ");
            formattedEc = formattedEc.Replace(">=", "GREATERTHANOREQUALS ");
            formattedEc = formattedEc.Replace(".", "DOT ");

            // ensure brackets surrounded by a space so can be identified by parser
            formattedEc = formattedEc.Replace("(", " ( ");
            formattedEc = formattedEc.Replace(")", " ) ");
            formattedEc = formattedEc.Replace("[", " [ ");
            formattedEc = formattedEc.Replace("]", " ] ");
            formattedEc = formattedEc.Replace("{", " { ");
            formattedEc = formattedEc.Replace("}", " } ");

            return formattedEc;
        }

        internal static string GetEclSection(string ecl, bool focus)
        {
            // identify focus concepts and refinements
            string focusConcepts = string.Empty;
            string refinements = string.Empty;

            // identify unsupported expression types (compound)
            if (ecl.Count(f => f == ':') > 1)
            {
                throw new Exception(TerminologyValueSet.UNSUPPORTED_EXPRESSION);
            }

            bool dottedAttributes = ecl.Contains(".");
            int refinementStart = ecl.IndexOf(":");
            int andRefinementStart = ecl.IndexOf("AND ");

            if (dottedAttributes && refinementStart < 0)
            {
                if (andRefinementStart > 0)
                {
                    focusConcepts = ecl.Substring(0, andRefinementStart - 1);
                    refinements = ecl.Substring(andRefinementStart + 1);
                }
                else
                {
                    focusConcepts = "ANY ";
                    refinements = ecl;
                }
            }
            else if (refinementStart > 0)
            {
                focusConcepts = ecl.Substring(0, refinementStart - 1);
                refinements = ecl.Substring(refinementStart + 1);
            }
            else
            {
                focusConcepts = ecl;
            }

            return (focus ? EclExpressionFormatter(focusConcepts) : EclExpressionFormatter(refinements));
        }

        internal static List<EclElement> GetEclFocusElements(string focus)
        {
            List<EclElement> focusElements = new List<EclElement>();

            EclBinaryOperator bop = EclBinaryOperator.NONE;
            EclUnaryOperator uop1 = EclUnaryOperator.ANY;
            EclUnaryOperator uop2 = EclUnaryOperator.ANY;
            short uopCount = 0;
            bool brackets = false;

            foreach (string ecElement in Regex.Split(focus, @"\s+"))
            {

                if (string.IsNullOrEmpty(ecElement))
                {
                }
                else if (ecElement == "(")
                {
                    brackets = true;
                    bop = EclBinaryOperator.NONE;
                }
                else if (ecElement == ")")
                {
                    brackets = false;
                }
                else if (ecElement == "AND" || ecElement == "OR" || ecElement == "MINUS")
                {
                    bop = (EclBinaryOperator)Enum.Parse(typeof(EclBinaryOperator), ecElement);
                }
                else if (ecElement == "ANY")
                {
                    focusElements.Add(new EclElement
                    {
                        UnaryOperator = EclUnaryOperator.ANY,
                        UnaryOperator2 = EclUnaryOperator.ANY,
                        BinaryOperator = EclBinaryOperator.NONE,
                        SCTID = FhirSnomed.SCT_ROOT_CONCEPT,
                        ElementOrder = (brackets ? 0 : focusElements.Count + 1)
                    });
                }
                else if (Utilities.IsDigitsOnly(ecElement))
                {
                    // element will always end with the SCTID
                    focusElements.Add(new EclElement
                    {
                        UnaryOperator = (focus.StartsWith(ecElement) ? EclUnaryOperator.SELF : uop1),
                        UnaryOperator2 = uop2,
                        BinaryOperator = bop,
                        SCTID = ecElement,
                        ElementOrder = (brackets ? 0 : focusElements.Count + 1)
                    });
                    // set other properties to defaults in case not contained in next element
                    bop = EclBinaryOperator.NONE;
                    uop1 = EclUnaryOperator.ANY;
                    uop2 = EclUnaryOperator.ANY;
                    uopCount = 0;
                }
                else
                {
                    uopCount++;
                    if (uopCount == 1)
                    {
                        uop1 = (EclUnaryOperator)Enum.Parse(typeof(EclUnaryOperator), ecElement);
                    }
                    else if (uopCount == 2)
                    {
                        uop2 = uop1;
                        uop1 = (EclUnaryOperator)Enum.Parse(typeof(EclUnaryOperator), ecElement);
                    }
                    else
                    {
                        throw new Exception(TerminologyValueSet.INVALID_EXPRESSION);
                    }
                }
            }

            return focusElements;
        }

        internal static List<Coding> GetEclFocusConcepts(List<EclElement> focusElements, bool refined)
        {
            List<Coding> masterConcepts = new List<Coding>();

            foreach (EclElement ee in focusElements.OrderBy(x => x.ElementOrder))
            {
                // if no refinements - only permit requests for immediate children of root concepts
                if (!refined
                    && ee.UnaryOperator != EclUnaryOperator.SELF
                    && ee.UnaryOperator != EclUnaryOperator.CHILDRENOF
                    && FhirSnomed.rootConcepts.ContainsKey(ee.SCTID))
                {
                    throw new Exception(TerminologyValueSet.MAX_VALUES_EXCEEDED);
                }

                // get result set for this element using first Unary Operator
                List<Coding> txConcepts = ApplyEclUnaryOperation(ee.UnaryOperator, ee.SCTID);

                // apply second unary operator (IF applied to reference set)
                if (ee.UnaryOperator2 != EclUnaryOperator.ANY && ee.UnaryOperator == EclUnaryOperator.MEMBEROF)
                {
                    List<Coding> refConcepts = new List<Coding>();
                    foreach (Coding cc in txConcepts)
                    {
                        refConcepts.AddRange(ApplyEclUnaryOperation(ee.UnaryOperator2, cc.Code));

                    }
                    txConcepts.AddRange(refConcepts);
                }

                // apply binary operator (always applied AFTER unary operators)
                masterConcepts = ApplyEclBinaryOperation(ee.BinaryOperator, masterConcepts, txConcepts);
            }

            return masterConcepts;
        }

        internal static List<EclRefinement> GetEclRefinements(string refine)
        {

            List<EclRefinement> refinements = new List<EclRefinement>();
            bool newRef = true;
            bool newMath = true;
            bool reverse = false;
            bool chainedReversal = (Regex.Matches(refine, "DOT").Count) > 1;
            string attNameID = string.Empty;
            string attValueID = string.Empty;
            EclUnaryOperator attNameOp = EclUnaryOperator.ANY;
            EclUnaryOperator attValueOp = EclUnaryOperator.ANY;
            EclMathematicalOperator mathOp = EclMathematicalOperator.EQUALS;

            foreach (string refElement in Regex.Split(refine, @"\s+"))
            {
                if (string.IsNullOrEmpty(refElement))
                {
                }
                else if (Utilities.IsDigitsOnly(refElement))
                {
                    if (newRef)
                    {
                        attNameID = refElement;
                        newRef = false;
                    }
                    else
                    {
                        refinements.Add(new EclRefinement
                        {
                            AttributeNameID = attNameID,
                            UnaryOperatorName = attNameOp,
                            MathematicalOperator = mathOp,
                            AttributeValueID = refElement,
                            UnaryOperatorValue = attValueOp,
                            ReverseAttribute = reverse,
                            ChainedReversal = chainedReversal
                        });
                        attNameOp = EclUnaryOperator.ANY;
                        attValueOp = EclUnaryOperator.ANY;
                        mathOp = EclMathematicalOperator.EQUALS;
                        attValueID = refElement;
                        newRef = true;
                        newMath = false;
                        reverse = false;
  
                    }
                }
                else if (newRef)
                {
                    // see if chained dot operator
                    try
                    {
                        mathOp = (EclMathematicalOperator)Enum.Parse(typeof(EclMathematicalOperator), refElement);
                        if (mathOp == EclMathematicalOperator.DOT)
                        {
                            reverse = true;
                            newRef = false;
                            newMath = false;
                            attNameID = refinements[0].AttributeValueID;// attribute value from previous refinement
                            attValueOp = refinements[0].UnaryOperatorValue;
                        }
                    }
                    catch { }

                    // check to see if unary operator on attribute type or reverse
                    try
                    {
                        attNameOp = (EclUnaryOperator)Enum.Parse(typeof(EclUnaryOperator), refElement);
                        if (attNameOp == EclUnaryOperator.ANY)
                        {
                            attNameID = string.Empty;
                            newRef = false;
                        }
                        else if (attNameOp == EclUnaryOperator.REVERSEOF)
                        {
                            attNameOp = EclUnaryOperator.ANY;
                            reverse = true;
                        }
                    }
                    catch { }
                }
                else if (newMath)
                {
                    try
                    {
                        mathOp = (EclMathematicalOperator)Enum.Parse(typeof(EclMathematicalOperator), refElement);
                        if (mathOp == EclMathematicalOperator.DOT)
                        {
                            reverse = true;
                        }
                        newMath = false;
                    }
                    catch { }
                }
                else
                {
                    // check to see if unary operator on attribute value
                    try
                    {
                        attValueOp = (EclUnaryOperator)Enum.Parse(typeof(EclUnaryOperator), refElement);
                    }
                    catch { }
                }
            }
            return refinements;
        }

        internal static List<Coding> ApplyEclRefinements(List<EclRefinement> refinements, List<Coding> focusConcepts, bool anyFocus)
        {
            int refNo = 0;
            string focusConcept = string.Empty;
            bool focusDescendants = false;
            List<Coding> refineConcepts = new List<Coding>();

            // chained refinements
        
            foreach (EclRefinement refine in refinements)
            {
                refineConcepts.Clear();
                bool dottedAttribute = (refine.MathematicalOperator == EclMathematicalOperator.DOT);

                // apply unary operator to attribute values

                if (refine.ChainedReversal)
                {
                    if (refNo == 0)
                    {
                        focusConcept = refine.AttributeNameID;
                        focusDescendants = (refine.UnaryOperatorValue == EclUnaryOperator.DESCENDANTOF || refine.UnaryOperatorValue == EclUnaryOperator.DESCENDANTORSELFOF);
                    }
                    else
                    {
                        refineConcepts = SnomedCtSearch.GetAttributesByFocusTypeValue(
                          focusConcept,
                          focusDescendants,
                          refine.AttributeNameID,
                          (refine.UnaryOperatorValue == EclUnaryOperator.DESCENDANTOF || refine.UnaryOperatorValue == EclUnaryOperator.DESCENDANTORSELFOF),
                          refine.AttributeValueID);
                    }
                }
                else if (dottedAttribute) 
                {
                    refineConcepts = SnomedCtSearch.GetConceptsByAttributeNameValuePair(
                    refine.AttributeValueID,
                    refine.ReverseAttribute,
                    refine.AttributeNameID,
                    (refine.UnaryOperatorValue == EclUnaryOperator.ANCESTOROF || refine.UnaryOperatorValue == EclUnaryOperator.ANCESTORORSELFOF),
                    (refine.UnaryOperatorValue == EclUnaryOperator.DESCENDANTOF || refine.UnaryOperatorValue == EclUnaryOperator.DESCENDANTORSELFOF),
                    (refine.UnaryOperatorName == EclUnaryOperator.ANCESTOROF || refine.UnaryOperatorName == EclUnaryOperator.ANCESTORORSELFOF),
                    (refine.UnaryOperatorName == EclUnaryOperator.DESCENDANTOF || refine.UnaryOperatorName == EclUnaryOperator.DESCENDANTORSELFOF));
                }
                else
                {
                    refineConcepts = SnomedCtSearch.GetConceptsByAttributeNameValuePair(
                    refine.AttributeNameID,
                    refine.ReverseAttribute,
                    refine.AttributeValueID,
                    (refine.UnaryOperatorName == EclUnaryOperator.ANCESTOROF || refine.UnaryOperatorName == EclUnaryOperator.ANCESTORORSELFOF),
                    (refine.UnaryOperatorName == EclUnaryOperator.DESCENDANTOF || refine.UnaryOperatorName == EclUnaryOperator.DESCENDANTORSELFOF),
                    (refine.UnaryOperatorValue == EclUnaryOperator.ANCESTOROF || refine.UnaryOperatorValue == EclUnaryOperator.ANCESTORORSELFOF),
                    (refine.UnaryOperatorValue == EclUnaryOperator.DESCENDANTOF || refine.UnaryOperatorValue == EclUnaryOperator.DESCENDANTORSELFOF));
                }

                if ((anyFocus && refNo == 0) || (refine.ChainedReversal && refNo > 0)) 
                {
                    focusConcepts.Clear();
                    focusConcepts.AddRange(refineConcepts);
                }
                else
                {
                    focusConcepts.RemoveAll(x => !refineConcepts.Select(y => y.Code).Contains(x.Code));
                }

                refNo++;
            }

            return focusConcepts;
        }

        internal static List<Coding> ApplyEclBinaryOperation(EclBinaryOperator binOp, List<Coding> masterConcepts, List<Coding> txConcepts)
        {
            switch (binOp)
            {
                case EclBinaryOperator.AND:
                    //concept must be in both result sets
                    masterConcepts.RemoveAll(x => !txConcepts.Select(y => y.Code).Contains(x.Code));
                    break;
                case EclBinaryOperator.OR:
                    // concept can be in either result set (preventing duplicates)
                    masterConcepts.AddRange(txConcepts.Where(x => !masterConcepts.Any(y => y.Code == x.Code)));
                    break;
                case EclBinaryOperator.MINUS:
                    // exclude any concepts also in second set
                    masterConcepts.RemoveAll(x => txConcepts.Select(y => y.Code).Contains(x.Code));
                    break;
                case EclBinaryOperator.NONE:
                    // should only apply to first element
                    masterConcepts.AddRange(txConcepts);
                    //masterConcepts.AddRange(txConcepts.Where(x => !masterConcepts.Any(y => y.Code == x.Code)));
                    break;
                default:
                    break;
            }
            return masterConcepts;
        }

        internal static List<Coding> ApplyEclUnaryOperation(EclUnaryOperator euOp, string sctid)
        {
            List<Coding> concepts = new List<Coding>();

            switch (euOp)
            {
                case EclUnaryOperator.ANCESTORORSELFOF:
                    concepts = SnomedCtSearch.GetConceptByCode(sctid);
                    concepts.AddRange(SnomedCtSearch.GetSuperTypes(sctid));
                    break;
                case EclUnaryOperator.ANCESTOROF:
                    concepts = SnomedCtSearch.GetSuperTypes(sctid);
                    break;
                case EclUnaryOperator.DESCENDANTORSELFOF:
                    concepts = SnomedCtSearch.GetSubsumedCodes(sctid,true);
                    break;
                case EclUnaryOperator.DESCENDANTOF:
                    concepts = SnomedCtSearch.GetSubsumedCodes(sctid,false);
                    break;
                case EclUnaryOperator.CHILDRENOF:
                    concepts = SnomedCtSearch.GetChildCodes(sctid);
                    break;
                case EclUnaryOperator.PARENTOF:
                    concepts = SnomedCtSearch.GetParentCodes(sctid);
                    break;
                case EclUnaryOperator.MEMBEROF:
                    concepts = SnomedCtSearch.GetRefSet(sctid, string.Empty);
                    break;
                case EclUnaryOperator.ANY:
                    concepts.Add(new Coding {Code = FhirSnomed.SCT_ROOT_CONCEPT, Display = FhirSnomed.SCT_ROOT_DESCRIPTION });
                    //concepts = SnomedCtSearch.GetConceptByCode(sctid);
                    break;
                case EclUnaryOperator.SELF:
                    concepts = SnomedCtSearch.GetConceptByCode(sctid);
                    break;
                default:
                    break;
            }

            return concepts;
        }
    }

    public enum EclUnaryOperator
    {
        ANCESTORORSELFOF,
        ANCESTOROF,
        DESCENDANTORSELFOF,
        DESCENDANTOF,
        CHILDRENOF,
        PARENTOF,
        MEMBEROF,
        REVERSEOF,
        ANY,
        SELF
    }

    public enum EclBinaryOperator
    {
        AND,
        OR,
        MINUS,
        NONE
    }

    public enum EclMathematicalOperator
    {
        EQUALS,
        NOTEQUALS,
        LESSTHAN,
        LESSTHANOREQUALS,
        GREATERTHAN,
        GREATERTHANOREQUALS,
        DOT
    }

    public class EclElement
    { 
 
        public string SCTID { get; set; }

        public EclBinaryOperator BinaryOperator{ get; set; }

        public EclUnaryOperator UnaryOperator { get; set; }

        public EclUnaryOperator UnaryOperator2{ get; set; }

        public int ElementOrder { get; set; }

    }

    public class EclRefinement
    {
        public string AttributeNameID { get; set; }

        public EclUnaryOperator UnaryOperatorName { get; set; }

        public EclMathematicalOperator MathematicalOperator { get; set; }

        public string AttributeValueID { get; set; }

        public EclUnaryOperator UnaryOperatorValue { get; set; }

        public bool ReverseAttribute { get; set; }
        
        public bool ChainedReversal { get; set; }
    }

}

