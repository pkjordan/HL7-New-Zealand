namespace Vintage.AppServices.BusinessClasses.FHIR.CodeSystems
{
    using System;
    using System.Collections.Generic;
    using Hl7.Fhir.Model;

    /// <summary>
    ///  NZ Ethnicity Codes - Level 2
    /// </summary>

    public class NzEthnicityL2
    {

        public const string URI = "https://standards.digital.health.nz/codesystem/ethnic-group-level-2";

        public CodeSystem codeSystem { get; set; }
        public ValueSet valueSet { get; set; }

        public NzEthnicityL2()
        {
            this.FillValues(TerminologyOperation.define_vs, string.Empty, string.Empty, string.Empty, -1, -1);
        }

        public NzEthnicityL2(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            this.FillValues(termOp, version, code, filter, offsetNo, countNo);
        }

        private void FillValues(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {

            this.valueSet = new ValueSet();
            this.codeSystem = new CodeSystem();

            this.valueSet.Id = "NzEthnicityL2";
            this.codeSystem.Id = "NzEthnicityL2";

            this.codeSystem.CaseSensitive = true;
            this.codeSystem.Content = CodeSystem.CodeSystemContentMode.Complete;
            this.codeSystem.Experimental = true;
            this.codeSystem.Compositional = false;
            this.codeSystem.VersionNeeded = false;

            this.valueSet.Url = ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzEthnicityL2";
            this.codeSystem.Url = NzEthnicityL2.URI;

            this.codeSystem.ValueSet = this.valueSet.Url;

            this.valueSet.Title = "NZ Ethnicity Level 2";
            this.codeSystem.Title = "NZ Ethnicity Level 2";

            this.valueSet.Name = this.valueSet.Id;
            this.codeSystem.Name = this.codeSystem.Id;

            this.valueSet.Description = new Markdown("Value Set of all NZ Ethnicity Level 2 Codes");
            this.codeSystem.Description = new Markdown("NZ Ethnicity Level 2 Codes");

            this.valueSet.Version = "1.0.1";
            this.codeSystem.Version = "1.0.1";

            this.valueSet.Experimental = false;

            this.valueSet.Status = PublicationStatus.Active;
            this.codeSystem.Status = PublicationStatus.Active;

            this.valueSet.Date = Hl7.Fhir.Model.Date.Today().Value;
            this.codeSystem.Date = Hl7.Fhir.Model.Date.Today().Value;

            this.valueSet.Publisher = "Patients First Ltd";
            this.codeSystem.Publisher = "Ministry of Health";

            this.valueSet.Copyright = new Markdown("© 2010+ New Zealand Crown Copyright");
            this.codeSystem.Copyright = new Markdown("© 2010+ New Zealand Crown Copyright");

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

            if ((string.IsNullOrEmpty(version) || version == cs.Version))
            {

                Dictionary<string, string> codeVals = new Dictionary<string, string>();

                codeVals.Add("10", "European NFD");
                codeVals.Add("11", "NZ European");
                codeVals.Add("12", "Other European");
                codeVals.Add("21", "NZ Māori");
                codeVals.Add("30", "Pacific Island NFD");
                codeVals.Add("31", "Samoan");
                codeVals.Add("32", "Cook Island Māori");
                codeVals.Add("33", "Tongan");
                codeVals.Add("34", "Niuean");
                codeVals.Add("35", "Tokelauan");
                codeVals.Add("36", "Fijian");
                codeVals.Add("37", "Other Pacific Island");
                codeVals.Add("40", "Asian NFD");
                codeVals.Add("41", "Southeast Asian");
                codeVals.Add("42", "Chinese");
                codeVals.Add("43", "Indian");
                codeVals.Add("44", "Other Asian");
                codeVals.Add("51", "Middle Eastern");
                codeVals.Add("52", "Latin American / Hispanic");
                codeVals.Add("53", "African");
                codeVals.Add("54", "Other (retired on 1/07/2009)");
                codeVals.Add("61", "Other Ethnicity");
                codeVals.Add("94", "Don't Know");
                codeVals.Add("95", "Refused to Answer");
                codeVals.Add("96", "Repeated Value");
                codeVals.Add("97", "Response Unidentifiable");
                codeVals.Add("98", "Response Outside Scope");
                codeVals.Add("99", "Not Stated");

                foreach (KeyValuePair<string, string> codeVal in codeVals)
                {
                    if (TerminologyValueSet.MatchValue(codeVal.Key, codeVal.Value, code, filter))
                    {
                        cs.Concept.Add(new ValueSet.ConceptReferenceComponent { Code = codeVal.Key, Display = codeVal.Value });
                        es.Contains.Add(new ValueSet.ContainsComponent { Code = codeVal.Key, Display = codeVal.Value, System = cs.System });
                        this.codeSystem.Concept.Add(new CodeSystem.ConceptDefinitionComponent { Code = codeVal.Key, Display = codeVal.Value });
                    }
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
    }
}

