namespace Vintage.AppServices.BusinessClasses.FHIR.ValueSets
{
    using System.Collections.Generic;
    using Hl7.Fhir.Model;
    using Vintage.AppServices.BusinessClasses.FHIR.CodeSystems;

    /// <summary>
    ///  Connectathon Extensional Test value set 3 ... selection of SNOMED CT Codes
    /// </summary>

    public class ConnectathonExtensional_3
    {

        public ValueSet valueSet { get; set; }

        public ConnectathonExtensional_3()
        {
            this.FillValues(TerminologyOperation.define_vs, string.Empty, string.Empty, string.Empty, -1, -1);
        }

        public ConnectathonExtensional_3(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            this.FillValues(termOp, version, code, filter, offsetNo, countNo);
        }

        private void FillValues(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            this.valueSet = new ValueSet();

            this.valueSet.Id = "extensional-case-3";
            this.valueSet.Url = "http://www.healthintersections.com.au/fhir/ValueSet/extensional-case-3";
            this.valueSet.Identifier.Add( new Identifier { Value = this.valueSet.Id });
            this.valueSet.Name = this.valueSet.Id;
            this.valueSet.Title = "Terminology Services Test: Extensional case #3";
            this.valueSet.Description = new Markdown("an enumeration of codes defined by SNOMED CT");
            this.valueSet.Version = "R4";
            this.valueSet.Status = PublicationStatus.Active;
            this.valueSet.Experimental = true;
            this.valueSet.Date = Hl7.Fhir.Model.Date.Today().Value;
            this.valueSet.Publisher = "Grahame Grieve";
            this.valueSet.Copyright = new Markdown("This value set includes content from SNOMED CT, which is copyright © 2002+ International Health Terminology Standards Development Organisation (IHTSDO), and distributed by agreement between IHTSDO and HL7. Implementer use of SNOMED CT is not covered by this agreement");

            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "grahame@healthintersections.com.au" };
            ContactDetail cd = new ContactDetail();
            cd.Telecom.Add(cp);
            this.valueSet.Contact.Add(cd);

            ValueSet.ComposeComponent comp = new ValueSet.ComposeComponent();
            ValueSet.ExpansionComponent es = new ValueSet.ExpansionComponent();

            ValueSet.ConceptSetComponent csc = new ValueSet.ConceptSetComponent { System = FhirSnomed.URI };

            if (string.IsNullOrEmpty(version) || version == this.valueSet.Version)
            {

                Dictionary<string, string> codeVals = new Dictionary<string, string>
                {
                    { "371037005", "Systolic dysfunction" },
                    { "56218007", "Systolic hypertension" },
                    { "271657009", "Systolic cardiac thrill" },
                    { "429457004", "Systolic essential hypertension" },
                    { "417996009", "Systolic heart failure" },
                    { "44623008", "Systolic ejection sound" },
                    { "248677002", "Systolic flow murmur" },
                    { "31574009", "Systolic murmur" },
                    { "120871000119108", "Systolic heart failure stage B" },
                    { "120851000119104", "Systolic heart failure stage D" },
                    { "120861000119102", "Systolic heart failure stage C" },
                    { "609556008", "Systolic heart failure stage A" },
                    { "61926008", "Basal systolic thrill" },
                    { "248672008", "Soft systolic murmur" },
                    { "65254001", "Late systolic murmur" },
                    { "89985004", "Early systolic murmur" },
                    { "248692001", "Mid-systolic click" },
                    { "248678007", "Mitral late systolic murmur" },
                    { "443254009", "Acute systolic heart failure" },
                    { "441481004", "Chronic systolic heart failure" },
                    { "48965007", "Single non-ejection systolic click" },
                    { "68519006", "Multiple non-ejection systolic clicks" },
                    { "134401001", "Left ventricular systolic dysfunction" },
                    { "416158002", "Right ventricular systolic dysfunction" },
                    { "68494000", "Mid-systolic murmur" },
                    { "442304009", "Combined systolic and diastolic dysfunction" },
                    { "18352002", "Abnormal systolic arterial pressure" },
                    { "407596008", "Echocardiogram shows left ventricular systolic dysfunction" },
                    { "371857005", "Normal left ventricular systolic function and wall motion" },
                    { "430396006", "Chronic systolic dysfunction of left ventricle" },
                    { "12929001", "Normal systolic arterial pressure" },
                    { "275285009", "On examination - systolic murmur" },
                    { "443253003", "Acute on chronic systolic heart failure" },
                    { "371862006", "Depression of left ventricular systolic function" },
                    { "81010002", "Decreased systolic arterial pressure" },
                    { "163030003", "On examination - Systolic BP reading" },
                    { "163069009", "On examination - systolic cardiac thrill" },
                    { "18050000", "Increased systolic arterial pressure" },
                    { "163094005", "On examination - pulmonary systolic murmur" },
                    { "426263006", "Congestive heart failure due to left ventricular systolic dysfunction" },
                    { "417081007", "Systolic anterior movement of mitral valve" },
                    { "248679004", "Mitral pansystolic murmur" },
                    { "248687003", "Presystolic mitral murmur" },
                    { "248688008", "Presystolic tricuspid murmur" },
                    { "71201008", "Pansystolic murmur" },
                    { "23795000", "Presystolic murmur" },
                    { "248680001", "Tricuspid inspiratory pansystolic murmur" },
                    { "248681002", "Left parasternal pansystolic murmur" }
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