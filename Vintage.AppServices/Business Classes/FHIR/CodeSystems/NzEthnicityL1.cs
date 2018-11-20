namespace Vintage.AppServices.BusinessClasses.FHIR.CodeSystems
{
    using System;
    using System.Collections.Generic;
    using Hl7.Fhir.Model;

    /// <summary>
    ///  NZ Ethnicity Codes - Level 1
    /// </summary>

    public class NzEthnicityL1
    {

        public const string URI = "https://standards.digital.health.nz/codesystem/ethnic-group-level-1";

        public CodeSystem codeSystem { get; set; }
        public ValueSet valueSet { get; set; }

        public NzEthnicityL1()
        {
            this.FillValues(TerminologyOperation.define_vs, string.Empty, string.Empty, string.Empty, -1, -1);
        }

        public NzEthnicityL1(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            this.FillValues(termOp, version, code, filter, offsetNo, countNo);
        }

        private void FillValues(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {

            this.valueSet = new ValueSet();
            this.codeSystem = new CodeSystem();

            this.valueSet.Id = "NzEthnicityL1";
            this.codeSystem.Id = "NzEthnicityL1";

            this.codeSystem.CaseSensitive = true;
            this.codeSystem.Content = CodeSystem.CodeSystemContentMode.Complete;
            this.codeSystem.Experimental = true;
            this.codeSystem.Compositional = false;
            this.codeSystem.VersionNeeded = false;

            this.valueSet.Url = ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzEthnicityL1";
            this.codeSystem.Url = NzEthnicityL1.URI;

            this.codeSystem.ValueSet = this.valueSet.Url;

            this.valueSet.Name = "NZ Ethnicity Level 1";
            this.codeSystem.Name = "NZ Ethnicity Level 1";

            this.valueSet.Description = new Markdown("Value Set of all NZ Ethnicity Level 1 Codes");
            this.codeSystem.Description = new Markdown("NZ Ethnicity Level 1 Codes");

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

                codeVals.Add("1", "European");
                codeVals.Add("2", "Māori");
                codeVals.Add("3", "Pacific Peoples");
                codeVals.Add("4", "Asian");
                codeVals.Add("5", "Middle Eastern/Latin American/African");
                codeVals.Add("6", "Other Ethnicity");
                codeVals.Add("9", "Residual Categories");

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

