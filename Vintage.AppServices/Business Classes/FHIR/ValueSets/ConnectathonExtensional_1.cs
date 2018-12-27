namespace Vintage.AppServices.BusinessClasses.FHIR.ValueSets
{
    using System.Collections.Generic;
    using Hl7.Fhir.Model;

    /// <summary>
    ///  Connectathon Extensional Test value set 1 ... Patient Contact Relationship Codes
    /// </summary>

    public class ConnectathonExtensional_1
    {

        public ValueSet valueSet { get; set; }

        public ConnectathonExtensional_1()
        {
            this.FillValues(TerminologyOperation.define_vs, string.Empty, string.Empty, string.Empty, -1, -1);
        }

        public ConnectathonExtensional_1(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            this.FillValues(termOp, version, code, filter, offsetNo, countNo);
        }

        private void FillValues(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {            
            this.valueSet = new ValueSet();

            this.valueSet.Id = "extensional-case-1";
            this.valueSet.Url = "http://www.healthintersections.com.au/fhir/ValueSet/extensional-case-1";
            this.valueSet.Identifier.Add(new Identifier { Value = this.valueSet.Id });
            this.valueSet.Name = "Terminology Services FHIR Connectathon #20: Extensional case #1";
            this.valueSet.Description = new Markdown("http://hl7.org/fhir/administrative-gender");
            this.valueSet.Version = "C20";
            this.valueSet.Experimental = true;
            this.valueSet.Status = PublicationStatus.Active;
            this.valueSet.Date = Hl7.Fhir.Model.Date.Today().Value;
            this.valueSet.Publisher = "Grahame Grieve";

            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "grahame@healthintersections.com.au" };
            ContactDetail cd = new ContactDetail();
            cd.Telecom.Add(cp);
            this.valueSet.Contact.Add(cd);

            ValueSet.ComposeComponent comp = new ValueSet.ComposeComponent();
            ValueSet.ExpansionComponent es = new ValueSet.ExpansionComponent();

            ValueSet.ConceptSetComponent csc = new ValueSet.ConceptSetComponent{ System = "http://hl7.org/fhir/administrative-gender" };

            if (string.IsNullOrEmpty(version) || version == this.valueSet.Version)
            {

                Dictionary<string, string> codeVals = new Dictionary<string, string>
                {
                    { "male", "Male" },
                    { "female", "Female" },
                    { "other", "Other" },
                    { "unknown", "Unknown" }
                };

                foreach (KeyValuePair<string, string> codeVal in codeVals)
                {
                    if (TerminologyValueSet.MatchValue(codeVal.Key, codeVal.Value, code, filter))
                    {
                        csc.Concept.Add(new ValueSet.ConceptReferenceComponent{ Code = codeVal.Key, Display = codeVal.Value });
                        es.Contains.Add(new ValueSet.ContainsComponent { Code = codeVal.Key, Display = codeVal.Value, System = csc.System });
                    }
                }

                if (termOp == TerminologyOperation.expand || termOp == TerminologyOperation.validate_code)
                {
                    this.valueSet = TerminologyValueSet.AddExpansion(this.valueSet, es, offsetNo, countNo);
                }
                else if (termOp == TerminologyOperation.define_vs)
                {
                    comp.Include.Add(csc);
                    this.valueSet.Compose = comp;
                }
            }

        }

    }
}
