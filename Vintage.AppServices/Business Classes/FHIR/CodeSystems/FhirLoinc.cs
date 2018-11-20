namespace Vintage.AppServices.BusinessClasses.FHIR.CodeSystems
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Hl7.Fhir.Model;
    using Vintage.AppServices.DataAccessClasses;

    /// <summary>
    ///  LOINC Code System & FHIR Value Sets created from it
    /// </summary>

    public class FhirLoinc
    {

        public const string NAME = "LOINC";
        public const string DESCRIPTION = "Logical Observation Identifiers Names and Codes";
        public const string URI = "http://loinc.org";
        public const string CURRENT_VERSION = "2.61";

        public CodeSystem codeSystem { get; set; }
        public ValueSet valueSet { get; set; }

        public FhirLoinc()
        {
            this.FillValues(TerminologyOperation.define_vs, string.Empty, string.Empty, string.Empty, string.Empty, -1, -1);
        }

        public FhirLoinc(TerminologyOperation termOp, string version, string code, string filter, string identifier, int offsetNo, int countNo)
        {
            this.FillValues(termOp, version, code, filter, identifier, offsetNo, countNo);
        }

        private void FillValues(TerminologyOperation termOp, string version, string code, string filter, string identifier, int offsetNo, int countNo)
        {

            // determine if filter contains a LOINC property-related query
            string loincProperty = string.Empty;
            string loincValue = string.Empty;

            // Look for property filter & place code in filter e.g. SYSTEM=Arterial System
            int component_Filter = identifier.IndexOf("/COMPONENT=");
            int property_Filter = identifier.IndexOf("/PROPERTY=");
            int time_aspct_Filter = identifier.IndexOf("/TIME_ASPCT=");
            int system_Filter = identifier.IndexOf("/SYSTEM=");
            int scale_typ_Filter = identifier.IndexOf("/SCALE_TYP=");
            int method_typ_Filter = identifier.IndexOf("/METHOD_TYP=");

            if (component_Filter > 0)
            {
                loincProperty = "COMPONENT";
                loincValue = identifier.Substring(component_Filter + 11);
            }
            else if (property_Filter > 0)
            {
                loincProperty = "PROPERTY";
                loincValue = identifier.Substring(property_Filter + 10);
            }
            else if (time_aspct_Filter > 0)
            {
                loincProperty = "TIME_ASPCT";
                loincValue = identifier.Substring(time_aspct_Filter + 12);
            }
            else if (system_Filter > 0)
            {
                loincProperty = "SYSTEM";
                loincValue = identifier.Substring(system_Filter + 8);
            }
            else if (scale_typ_Filter > 0)
            {
                loincProperty = "SCALE_TYP";
                loincValue = identifier.Substring(scale_typ_Filter + 11);
            }
            else if (method_typ_Filter > 0)
            {
                loincProperty = "METHOD_TYP";
                loincValue = identifier.Substring(method_typ_Filter + 12);
            }

            string description = "All LOINC Codes Filtered By: " + filter;
            string name = "LOINC";
            string idSuffix = filter.Replace(" ", "_");

            if (!string.IsNullOrEmpty(loincProperty) && !string.IsNullOrEmpty(loincValue))
            {
                description = "LOINC codes where " + loincProperty + " = " + loincValue;
                name = "LOINC Codes: " + loincProperty + "=" + loincValue;
                idSuffix = loincValue.Replace(" ", "_");
            }

            this.valueSet = new ValueSet();
            this.codeSystem = new CodeSystem();

            this.valueSet.Id = "fhir_loinc_vs_" + idSuffix;
            this.codeSystem.Id = "LOINC";

            this.codeSystem.CaseSensitive = true;
            this.codeSystem.Content = CodeSystem.CodeSystemContentMode.NotPresent;
            this.codeSystem.Experimental = false;
            this.codeSystem.Compositional = false;
            this.codeSystem.VersionNeeded = false;
            this.codeSystem.HierarchyMeaning = CodeSystem.CodeSystemHierarchyMeaning.PartOf;

            // Code System filters

            List<FilterOperator?> fops = new List<FilterOperator?>();
            fops.Add(FilterOperator.Equal);
            this.codeSystem.Filter.Add(new CodeSystem.FilterComponent { Code = "property", Description = "Allows the selection of a set of LOINC codes with a common property value.", Value = "A LOINC Code", Operator = fops.AsEnumerable() });


            this.valueSet.Url = ServerCapability.TERMINZ_CANONICAL + "/ValueSet/loinc/" + idSuffix;
            this.codeSystem.Url = FhirLoinc.URI;

            this.codeSystem.ValueSet = "http://loinc.org/vs";
            
            this.valueSet.Name = name;
            this.codeSystem.Name = FhirLoinc.NAME;

            this.valueSet.Description = new Markdown(description);
            this.codeSystem.Description = new Markdown(FhirLoinc.DESCRIPTION);

            this.valueSet.Version = FhirLoinc.CURRENT_VERSION;
            this.codeSystem.Version = FhirLoinc.CURRENT_VERSION;

            this.valueSet.Experimental = true;

            this.valueSet.Status = PublicationStatus.Active;
            this.codeSystem.Status = PublicationStatus.Active;

            this.valueSet.Date = Hl7.Fhir.Model.Date.Today().Value;
            this.codeSystem.Date = Hl7.Fhir.Model.Date.Today().Value;

            this.valueSet.Publisher = "Patients First Ltd";
            this.codeSystem.Publisher = "Regenstrief Institute, Inc.";

            this.valueSet.Copyright = new Markdown("This content LOINC is copyright © 1995 Regenstrief Institute, Inc. and the LOINC Committee, and available at no cost under the license at http://loinc.org/terms-of-use");
            this.codeSystem.Copyright = this.valueSet.Copyright;

            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "peter.jordan@patientsfirst.org.nz" };
            ContactDetail cd = new ContactDetail();
            cd.Telecom.Add(cp);
            this.valueSet.Contact.Add(cd);
            this.codeSystem.Contact.Add(cd);

            ValueSet.ConceptSetComponent cs = new ValueSet.ConceptSetComponent();
            ValueSet.ExpansionComponent es = new ValueSet.ExpansionComponent();

            cs.System = this.codeSystem.Url;
            cs.Version = this.codeSystem.Version;
            
            string codeCode = string.Empty;
            string codeDisplay = string.Empty;
            string codeDefinition = string.Empty;

            if ((string.IsNullOrEmpty(version) || version == cs.Version) && termOp != TerminologyOperation.define_cs)
            {

                List<Coding> codeVals = new List<Coding>();

                if (termOp != TerminologyOperation.define_vs)
                {
                    if (!string.IsNullOrEmpty(loincProperty) && !string.IsNullOrEmpty(loincValue))
                    {
                        codeVals = LoincSearch.GetConceptsByProperty(loincProperty, loincValue);
                    }
                    else if (!string.IsNullOrEmpty(code))
                    {
                        // returning Parameters Resource with False appears to be preferred to OperationOutcome failure
                        //if (!Utilities.IsDigitsOnly(code))
                        //{
                        //    throw new Exception(TerminologyValueSet.INVALID_CODE);
                        //}
                        if (termOp == TerminologyOperation.compose)
                        {
                            codeVals = LoincSearch.GetPropertiesByCode(code);
                        }
                        else
                        {
                            codeVals = LoincSearch.GetConceptByCode(code);
                        }
                    }
                    else if (!string.IsNullOrEmpty(filter))
                    {
                        if (filter.Length < 3)
                        {
                            throw new Exception(TerminologyValueSet.INVALID_FILTER);
                        }
                        codeVals = LoincSearch.GetConceptsByTerm(filter);
                    }
                    else // can't pass LOINC in its entirety!
                    {
                        throw new Exception(TerminologyValueSet.MISSING_FILTER);
                    }

                    if (codeVals.Count > 9999)
                    {
                        throw new Exception(TerminologyValueSet.MAX_VALUES_EXCEEDED);
                    }
                }

                // filtering performed at DB Layer, so add all returned concepts
                foreach (Coding codeVal in codeVals)
                {
                    cs.Concept.Add(new ValueSet.ConceptReferenceComponent { Code = codeVal.Code, Display = codeVal.Display });
                    es.Contains.Add(new ValueSet.ContainsComponent { Code = codeVal.Code, Display = codeVal.Display, System = cs.System });
                    this.codeSystem.Concept.Add(new CodeSystem.ConceptDefinitionComponent { Code = codeVal.Code, Display = codeVal.Display });
                }

                if (termOp == TerminologyOperation.expand || termOp == TerminologyOperation.validate_code)
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


        internal static TerminologyCapabilities.CodeSystemComponent GetCapabilities()
        {
            TerminologyCapabilities.CodeSystemComponent csc = new TerminologyCapabilities.CodeSystemComponent();
            csc.Uri = FhirLoinc.URI;

            TerminologyCapabilities.VersionComponent vc = new TerminologyCapabilities.VersionComponent();
            vc.Code = FhirLoinc.CURRENT_VERSION;
            vc.IsDefault = true;
            vc.Compositional = false;
            vc.LanguageElement.Add(new Code("en-US"));

            TerminologyCapabilities.FilterComponent filt_1 = new TerminologyCapabilities.FilterComponent();
            filt_1.CodeElement = new Code("property");
            filt_1.OpElement.Add(new Code("ValueSet-expand"));
            vc.Filter.Add(filt_1);

            vc.PropertyElement.Add(new Code("code"));
            vc.PropertyElement.Add(new Code("system"));
            vc.PropertyElement.Add(new Code("version"));
            vc.PropertyElement.Add(new Code("definition"));
            vc.PropertyElement.Add(new Code("designation"));
            vc.PropertyElement.Add(new Code("COMPONENT"));
            vc.PropertyElement.Add(new Code("PROPERTY"));
            vc.PropertyElement.Add(new Code("TIME_ASPCT"));
            vc.PropertyElement.Add(new Code("SYSTEM"));
            vc.PropertyElement.Add(new Code("SCALE_TYP"));
            vc.PropertyElement.Add(new Code("METHOD_TYP"));
            
            csc.Version.Add(vc);

            return csc;
        }
    }
}

