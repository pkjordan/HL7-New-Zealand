namespace Vintage.AppServices.BusinessClasses.FHIR.ValueSets
{
    using System.Collections.Generic;
    using Hl7.Fhir.Model;
    using Vintage.AppServices.DataAccessClasses;
    using Vintage.AppServices.BusinessClasses.FHIR.CodeSystems;

    /// <summary>
    ///  Connectathon Intensional Test value set 1 ...All LOINC codes for SYSTEM = Arterial system
    /// </summary>

    public class ConnectathonIntensional_1
    {

        public ValueSet valueSet { get; set; }

        public ConnectathonIntensional_1()
        {
            this.FillValues(TerminologyOperation.define_vs, string.Empty, string.Empty, string.Empty, -1, -1);
        }

        public ConnectathonIntensional_1(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            this.FillValues(termOp, version, code, filter, offsetNo, countNo);
        }

        private void FillValues(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            this.valueSet = new ValueSet();
            this.valueSet.Id = "intensional-case-1";
            this.valueSet.Url = "http://www.healthintersections.com.au/fhir/ValueSet/intensional-case-1";
            this.valueSet.Identifier.Add(new Identifier { Value = this.valueSet.Id });
            this.valueSet.Name = this.valueSet.Id;
            this.valueSet.Title = "Terminology Services FHIR Connectathon #20: Intensional case #1";
            this.valueSet.Description = new Markdown("All loinc codes for system = Arterial system");
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

            ValueSet.ConceptSetComponent cs = new ValueSet.ConceptSetComponent();
            cs.System = FhirLoinc.URI;
            cs.Version = FhirLoinc.CURRENT_VERSION;
            cs.Filter.Add(new ValueSet.FilterComponent { Property = "SYSTEM", Op = FilterOperator.Equal, Value = "Arterial System" });

            string codeCode = string.Empty;
            string codeDisplay = string.Empty;
            string codeDefinition = string.Empty;

            if (string.IsNullOrEmpty(version) || version == cs.Version)
            {

                if (termOp == TerminologyOperation.expand || termOp == TerminologyOperation.validate_code)
                {
                    List<Coding> codeVals = new List<Coding>();
                    codeVals = LoincSearch.GetConceptsByProperty(cs.Filter[0].Property, cs.Filter[0].Value);
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

