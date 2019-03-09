namespace Vintage.AppServices.BusinessClasses.FHIR.CodeSystems
{
    using System;
    using System.Collections.Generic;
    using Hl7.Fhir.Model;

    /// <summary>
    ///  NZ Ethnicity Codes - Level 3
    /// </summary>

    public class NzEthnicityL3
    {
        public const string URI = "https://standards.digital.health.nz/codesystem/ethnic-group-level-3";

        public CodeSystem codeSystem { get; set; }
        public ValueSet valueSet { get; set; }

        public NzEthnicityL3()
        {
            this.FillValues(TerminologyOperation.define_vs, string.Empty, string.Empty, string.Empty, -1, -1);
        }

        public NzEthnicityL3(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            this.FillValues(termOp, version, code, filter, offsetNo, countNo);
        }

        private void FillValues(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {

            this.valueSet = new ValueSet();
            this.codeSystem = new CodeSystem();

            this.valueSet.Id = "NzEthnicityL3";
            this.codeSystem.Id = "NzEthnicityL3";

            this.codeSystem.CaseSensitive = true;
            this.codeSystem.Content = CodeSystem.CodeSystemContentMode.Complete;
            this.codeSystem.Experimental = true;
            this.codeSystem.Compositional = false;
            this.codeSystem.VersionNeeded = false;

            this.valueSet.Url = ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzEthnicityL3";
            this.codeSystem.Url = NzEthnicityL3.URI;

            this.codeSystem.ValueSet = this.valueSet.Url;

            this.valueSet.Title = "NZ Ethnicity Level 3";
            this.codeSystem.Title = "NZ Ethnicity Level 3";

            this.valueSet.Name = this.valueSet.Id;
            this.codeSystem.Name = this.codeSystem.Id;

            this.valueSet.Description = new Markdown("Value Set of all NZ Ethnicity Level 3 Codes");
            this.codeSystem.Description = new Markdown("NZ Ethnicity Level 3 Codes");

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

                codeVals.Add("100", "European NFD");
                codeVals.Add("111", "New Zealand European");
                codeVals.Add("120", "Other European NFD");
                codeVals.Add("121", "British and Irish");
                codeVals.Add("122", "Dutch");
                codeVals.Add("123", "Greek (including Greek Cypriot)");
                codeVals.Add("124", "Polish");
                codeVals.Add("125", "South Slav (formerly Yugoslav)");
                codeVals.Add("126", "Italian");
                codeVals.Add("127", "German");
                codeVals.Add("128", "Australian");
                codeVals.Add("129", "Other European");
                codeVals.Add("211", "Māori");
                codeVals.Add("300", "Pacific Peoples NFD");
                codeVals.Add("311", "Samoan");
                codeVals.Add("321", "Cook Island Māori");
                codeVals.Add("331", "Tongan");
                codeVals.Add("341", "Niuean");
                codeVals.Add("351", "Tokelauan");
                codeVals.Add("361", "Fijian");
                codeVals.Add("371", "Other Pacific Peoples");
                codeVals.Add("400", "Asian NFD");
                codeVals.Add("410", "Southeast Asian NFD");
                codeVals.Add("411", "Filipino");
                codeVals.Add("412", "Khmer / Kampuchean / Cambodian");
                codeVals.Add("413", "Vietnamese");
                codeVals.Add("414", "Other Southeast Asian");
                codeVals.Add("421", "Chinese");
                codeVals.Add("431", "Indian");
                codeVals.Add("441", "Sri Lankan");
                codeVals.Add("442", "Japanese");
                codeVals.Add("443", "Korean");
                codeVals.Add("444", "Other Asian");
                codeVals.Add("511", "Middle Eastern");
                codeVals.Add("521", "Latin American / Hispanic");
                codeVals.Add("531", "African(or cultural group of African origin)");
                codeVals.Add("611", "Other Ethnicity");
                codeVals.Add("944", "Don't Know");
                codeVals.Add("955", "Refused to Answer");
                codeVals.Add("966", "Repeated value");
                codeVals.Add("977", "Response unidentifiable");
                codeVals.Add("988", "Response outside scope");
                codeVals.Add("999", "Not stated");


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