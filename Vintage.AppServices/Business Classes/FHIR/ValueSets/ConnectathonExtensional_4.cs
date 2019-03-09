namespace Vintage.AppServices.BusinessClasses.FHIR.ValueSets
{
    using System.Collections.Generic;
    using Hl7.Fhir.Model;

    /// <summary>
    ///  Connectathon Extensional Test value set 4 ... Patient Contact Relationship Codes
    /// </summary>

    public class ConnectathonExtensional_4
    {

        public ValueSet valueSet { get; set; }

        public ConnectathonExtensional_4()
        {
            this.FillValues(TerminologyOperation.define_vs, string.Empty, string.Empty, string.Empty, -1, -1);
        }

        public ConnectathonExtensional_4(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            this.FillValues(termOp, version, code, filter, offsetNo, countNo);
        }

        private void FillValues(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            this.valueSet = new ValueSet();

            this.valueSet.Id = "extensional-case-4";
            this.valueSet.Url = "http://www.healthintersections.com.au/fhir/ValueSet/extensional-case-4";
            this.valueSet.Identifier.Add( new Identifier { Value = this.valueSet.Id});
            this.valueSet.Name = this.valueSet.Id;
            this.valueSet.Title = "Terminology Services FHIR Connectathon #20: Extensional case #4";
            this.valueSet.Description = new Markdown("A mixed enumeration of codes from FHIR, and from V2 administrative gender code");
            this.valueSet.Version = "C20";
            this.valueSet.Status = PublicationStatus.Active;
            this.valueSet.Experimental = true;
            this.valueSet.Date = Hl7.Fhir.Model.Date.Today().Value;
            this.valueSet.Publisher = "Grahame Grieve";

            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "grahame@healthintersections.com.au" };
            ContactDetail cd = new ContactDetail();
            cd.Telecom.Add(cp);
            this.valueSet.Contact.Add(cd);

            ValueSet.ComposeComponent comp = new ValueSet.ComposeComponent();
            ValueSet.ExpansionComponent es = new ValueSet.ExpansionComponent();

            ValueSet.ConceptSetComponent csc = new ValueSet.ConceptSetComponent { System = "http://hl7.org/fhir/administrative-gender" };
            ValueSet.ConceptSetComponent csc2 = new ValueSet.ConceptSetComponent { System = "http://hl7.org/fhir/v2/0001" };

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
                        csc.Concept.Add(new ValueSet.ConceptReferenceComponent { Code = codeVal.Key, Display = codeVal.Value });
                        es.Contains.Add(new ValueSet.ContainsComponent { Code = codeVal.Key, Display = codeVal.Value, System = csc.System });
                    }
                }

                Dictionary<string, string> codeVals2 = new Dictionary<string, string>
                {
                    { "A", "Ambiguous" },
                    { "M", "Male" },
                    { "F", "Female" },
                    { "N", "Not applicable" },
                    { "O", "Other" },
                    { "U", "Unknown" }
                };

                foreach (KeyValuePair<string, string> codeVal in codeVals2)
                {
                    if (TerminologyValueSet.MatchValue(codeVal.Key, codeVal.Value, code, filter))
                    {
                        csc2.Concept.Add(new ValueSet.ConceptReferenceComponent { Code = codeVal.Key, Display = codeVal.Value });
                        es.Contains.Add(new ValueSet.ContainsComponent { Code = codeVal.Key, Display = codeVal.Value, System = csc2.System });
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
