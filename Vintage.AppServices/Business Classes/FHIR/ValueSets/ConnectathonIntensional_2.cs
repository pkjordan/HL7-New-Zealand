namespace Vintage.AppServices.BusinessClasses.FHIR.ValueSets
{
    using System.Collections.Generic;
    using Hl7.Fhir.Model;
    using Vintage.AppServices.DataAccessClasses;
    using Vintage.AppServices.BusinessClasses.FHIR.CodeSystems;

    /// <summary>
    ///  Connectathon Intensional Test value set 2 ...All Snomed codes that are subsumed by 38341003 (Hypertensive disorder, systemic arterial)
    /// </summary>

    public class ConnectathonIntensional_2
    {

        public ValueSet valueSet { get; set; }

        public ConnectathonIntensional_2()
        {
            this.FillValues(TerminologyOperation.define_vs, string.Empty, string.Empty, string.Empty, -1, -1);
        }

        public ConnectathonIntensional_2(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            this.FillValues(termOp, version, code, filter, offsetNo, countNo);
        }

        private void FillValues(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {

            this.valueSet = new ValueSet();

            this.valueSet.Id = "intensional-case-2";
            this.valueSet.Url = "http://www.healthintersections.com.au/fhir/ValueSet/intensional-case-2";
            this.valueSet.Identifier.Add(new Identifier { Value = this.valueSet.Id });
            this.valueSet.Name = this.valueSet.Id;
            this.valueSet.Title = "Terminology Services FHIR Connectathon #20: Intensional case #2";
            this.valueSet.Description = new Markdown("All Snomed codes that are subsumed by 38341003 (Hypertensive disorder, systemic arterial)");
            this.valueSet.Version = "C20";
            this.valueSet.Status = PublicationStatus.Active;
            this.valueSet.Experimental = true;
            this.valueSet.Date = Hl7.Fhir.Model.Date.Today().Value;
            this.valueSet.Publisher = "Grahame Grieve";
            this.valueSet.Copyright = new Markdown("This value set includes content from SNOMED CT, which is copyright © 2002+ International Health Terminology Standards Development Organisation (IHTSDO), and distributed by agreement between IHTSDO and HL7. Implementer use of SNOMED CT is not covered by this agreement");

            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "grahame@healthintersections.com.au" };
            ContactDetail cd = new ContactDetail();
            cd.Telecom.Add(cp);
            this.valueSet.Contact.Add(cd);

            ValueSet.ConceptSetComponent cs = new ValueSet.ConceptSetComponent();
            cs.System = FhirSnomed.URI;
            cs.Version = FhirSnomed.CURRENT_VERSION;
            cs.Filter.Add(new ValueSet.FilterComponent { Property = "concept", Op = FilterOperator.IsA, Value = "38341003" });

            string codeCode = string.Empty;
            string codeDisplay = string.Empty;
            string codeDefinition = string.Empty;

            if (string.IsNullOrEmpty(version) || version == cs.Version)
            {
                if (string.IsNullOrEmpty(version) || version == cs.Version)
                {

                    if (termOp == TerminologyOperation.expand || termOp == TerminologyOperation.validate_code)
                    {
                        List<Coding> codeVals = new List<Coding>();
                        codeVals = SnomedCtSearch.GetSubsumedCodes("38341003",true);
                        ValueSet.ExpansionComponent es = new ValueSet.ExpansionComponent();
                        foreach (Coding codeVal in codeVals)
                        {
                            if (TerminologyValueSet.MatchValue(codeVal.Code, codeVal.Display, code, filter))
                            {
                                es.Contains.Add(new ValueSet.ContainsComponent { Code = codeVal.Code, Display = codeVal.Display, System = cs.System });
                            }
                        }
                        this.valueSet = TerminologyValueSet.AddExpansion(this.valueSet, es, offsetNo, countNo);
                    }
                    else
                    {
                        this.valueSet.Compose = new ValueSet.ComposeComponent();
                        this.valueSet.Compose.Include.Add(cs);
                    }
                }

            }
        }

    }
}

