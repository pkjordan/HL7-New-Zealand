namespace Vintage.AppServices.BusinessClasses.FHIR.ValueSets
{
    using System.Collections.Generic;
    using Hl7.Fhir.Model;
    using Vintage.AppServices.BusinessClasses.FHIR.CodeSystems;

    /// <summary>
    ///  Connectathon Extensional Test value set 2 ... LOINC Blood Pressure Codes
    /// </summary>

    public class ConnectathonExtensional_2
    {

        public ValueSet valueSet { get; set; }

        public ConnectathonExtensional_2()
        {
            this.FillValues(TerminologyOperation.define_vs, string.Empty, string.Empty, string.Empty, -1, -1);
        }

        public ConnectathonExtensional_2(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            this.FillValues(termOp, version, code, filter, offsetNo, countNo);
        }

        private void FillValues(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            this.valueSet = new ValueSet();

            this.valueSet.Id = "extensional-case-2";
            this.valueSet.Url = "http://www.healthintersections.com.au/fhir/ValueSet/extensional-case-2";
            this.valueSet.Identifier.Add( new Identifier { Value = this.valueSet.Id});
            this.valueSet.Name = "Terminology Services FHIR Connectathon #20: Extensional case #2";
            this.valueSet.Description = new Markdown("an enumeration of codes defined by LOINC");
            this.valueSet.Version = "C20";
            this.valueSet.Status = PublicationStatus.Active;
            this.valueSet.Experimental = true;
            this.valueSet.Date = Hl7.Fhir.Model.Date.Today().Value;
            this.valueSet.Publisher = "Grahame Grieve";
            this.valueSet.Copyright = new Markdown("This content LOINC is copyright © 1995 Regenstrief Institute, Inc. and the LOINC Committee, and available at no cost under the license at http://loinc.org/terms-of-use");

            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "grahame@healthintersections.com.au" };
            ContactDetail cd = new ContactDetail();
            cd.Telecom.Add(cp);
            this.valueSet.Contact.Add(cd);

            ValueSet.ComposeComponent comp = new ValueSet.ComposeComponent();
            ValueSet.ExpansionComponent es = new ValueSet.ExpansionComponent();

            ValueSet.ConceptSetComponent csc = new ValueSet.ConceptSetComponent { System = FhirLoinc.URI, Version = FhirLoinc.CURRENT_VERSION };

            if (string.IsNullOrEmpty(version) || version == this.valueSet.Version)
            {

                Dictionary<string, string> codeVals = new Dictionary<string, string>
                {
                    { "11378-7", "Systolic blood pressure at First encounter" },
                    { "8493-9", "Systolic blood pressure 10 hour minimum" },
                    { "8494-7", "Systolic blood pressure 12 hour minimum" },
                    { "8495-4", "Systolic blood pressure 24 hour minimum" },
                    { "8450-9", "Systolic blood pressure--expiration" },
                    { "8451-7", "Systolic blood pressure--inspiration" },
                    { "8452-5", "Systolic blood pressure.inspiration - expiration" },
                    { "8459-0", "Systolic blood pressure--sitting" },
                    { "8460-8", "Systolic blood pressure--standing" },
                    { "8461-6", "Systolic blood pressure--supine" },
                    { "8479-8", "Systolic blood pressure by palpation" },
                    { "8480-6", "Systolic blood pressure" },
                    { "8481-4", "Systolic blood pressure 1 hour maximum" },
                    { "8482-2", "Systolic blood pressure 8 hour maximum" },
                    { "8483-0", "Systolic blood pressure 10 hour maximum" },
                    { "8484-8", "Systolic blood pressure 12 hour maximum" },
                    { "8485-5", "Systolic blood pressure 24 hour maximum" },
                    { "8486-3", "Systolic blood pressure 1 hour mean" },
                    { "8487-1", "Systolic blood pressure 8 hour mean" },
                    { "8488-9", "Systolic blood pressure 10 hour mean" },
                    { "8489-7", "Systolic blood pressure 12 hour mean" },
                    { "8490-5", "Systolic blood pressure 24 hour mean" },
                    { "8491-3", "Systolic blood pressure 1 hour minimum" },
                    { "8492-1", "Systolic blood pressure 8 hour minimum" }
                };

                foreach (KeyValuePair<string, string> codeVal in codeVals)
                {
                    if (TerminologyValueSet.MatchValue(codeVal.Key, codeVal.Value, code, filter))
                    {
                        csc.Concept.Add(new ValueSet.ConceptReferenceComponent { Code = codeVal.Key, Display = codeVal.Value });
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
