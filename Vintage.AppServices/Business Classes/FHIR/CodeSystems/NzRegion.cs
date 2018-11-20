namespace Vintage.AppServices.BusinessClasses.FHIR.CodeSystems
{
    using System;
    using System.Collections.Generic;
    using Hl7.Fhir.Model;

    /// <summary>
    ///  Test (extensional) value set of NZ Regional Codes & underlying Code System
    /// </summary>

    public class NzRegion
    {
        public const string URI = ServerCapability.HL7_FHIR_CANONICAL + "/CodeSystem/NzRegion";

        public CodeSystem codeSystem { get; set; }
        public ValueSet valueSet { get; set; }

        public NzRegion()
        {
            this.FillValues(TerminologyOperation.define_vs, string.Empty, string.Empty,string.Empty,-1, -1);
        }

        public NzRegion(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            this.FillValues(termOp, version, code, filter,offsetNo, countNo);
        }

        private void FillValues(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            
            this.valueSet = new ValueSet();
            this.codeSystem = new CodeSystem();

            this.valueSet.Id = "NzRegion";
            this.codeSystem.Id = "NzRegion";

            this.codeSystem.CaseSensitive = true;
            this.codeSystem.Content = CodeSystem.CodeSystemContentMode.Complete;
            this.codeSystem.Experimental = true;
            this.codeSystem.Compositional = false;
            this.codeSystem.VersionNeeded = false;

            this.valueSet.Url = ServerCapability.HL7_FHIR_CANONICAL + "/ValueSet/NzRegion";
            this.codeSystem.Url = NzRegion.URI;

            this.codeSystem.ValueSet = this.valueSet.Url;

            this.valueSet.Name = "NZ Region";
            this.codeSystem.Name = "NZ Region";

            this.valueSet.Description = new Markdown("Value Set of all NZ DHB Regional Codes");
            this.codeSystem.Description = new Markdown("NZ DHB Regional Codes");

            this.valueSet.Version = "1.0.1";
            this.codeSystem.Version = "1.0.1";

            this.valueSet.Experimental = true;

            this.valueSet.Status = PublicationStatus.Active;
            this.codeSystem.Status = PublicationStatus.Draft;

            this.valueSet.Date = Hl7.Fhir.Model.Date.Today().Value;
            this.codeSystem.Date = Hl7.Fhir.Model.Date.Today().Value;

            this.valueSet.Publisher = "Patients First Ltd";
            this.codeSystem.Publisher = "Patients First Ltd";

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

            if ( (string.IsNullOrEmpty(version) || version == cs.Version))
            {

                Dictionary<string, string> codeVals = new Dictionary<string, string>();

                codeVals.Add("NORTH", "Northern");
                codeVals.Add("MIDLAND", "Midland");
                codeVals.Add("CENTRAL", "Central");
                codeVals.Add("SOUTH", "Southern");

                foreach (KeyValuePair<string, string> codeVal in codeVals)
                {
                    if (TerminologyValueSet.MatchValue(codeVal.Key, codeVal.Value, code, filter))
                    {
                        cs.Concept.Add(new ValueSet.ConceptReferenceComponent { Code = codeVal.Key, Display = codeVal.Value });
                        es.Contains.Add(new ValueSet.ContainsComponent{ Code = codeVal.Key, Display = codeVal.Value, System = cs.System});
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

