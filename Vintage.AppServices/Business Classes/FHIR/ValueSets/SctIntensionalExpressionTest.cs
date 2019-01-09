namespace Vintage.AppServices.BusinessClasses.FHIR.ValueSets
{
    using System.Collections.Generic;
    using Hl7.Fhir.Model;
    using Vintage.AppServices.BusinessClasses.FHIR.CodeSystems;
    using Vintage.AppServices.DataAccessClasses;

    /// <summary>
    ///  Intensional Value Set Test for SCT Expression Filters
    /// </summary>

    public class SctIntensionalExpressionTest
    {

        public ValueSet valueSet { get; set; }

        public SctIntensionalExpressionTest()
        {
            this.FillValues(TerminologyOperation.define_vs, string.Empty, string.Empty, string.Empty, -1, -1);
        }

        public SctIntensionalExpressionTest(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            this.FillValues(termOp, version, code, filter, offsetNo, countNo);
        }

        private void FillValues(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {

            filter = EclHandler.EclExpressionFormatter("390802008|Goal achieved| OR << 390801001|Goal not achieved|");

            this.valueSet = new ValueSet();

            this.valueSet.Id = "SctIntensionalExpressionTest";
            this.valueSet.Url = ServerCapability.TERMINZ_CANONICAL + "ValueSet/SctIntensionalExpressionTest";
            this.valueSet.Identifier.Add(new Identifier { Value = this.valueSet.Id });
            this.valueSet.Name = "SCT Intensional Expression Test";
            this.valueSet.Description = new Markdown("Test for creating Intensional Value Sets filtered by SCT Expression Constraints");
            this.valueSet.Version = "1.0.1";
            this.valueSet.Status = PublicationStatus.Draft;
            this.valueSet.Experimental = true;
            this.valueSet.Date = Hl7.Fhir.Model.Date.Today().Value;
            this.valueSet.Publisher = "Peter Jordan";
            this.valueSet.Copyright = new Markdown("This value set includes content from SNOMED CT, which is copyright © 2002+ International Health Terminology Standards Development Organisation (IHTSDO), and distributed by agreement between IHTSDO and HL7. Implementer use of SNOMED CT is not covered by this agreement");

            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "pkjordan@xtra.co.nz" };
            ContactDetail cd = new ContactDetail();
            cd.Telecom.Add(cp);
            this.valueSet.Contact.Add(cd);

            ValueSet.ConceptSetComponent cs = new ValueSet.ConceptSetComponent();
            cs.System = FhirSnomed.URI;
            cs.Version = FhirSnomed.CURRENT_VERSION;
            cs.Filter.Add(new ValueSet.FilterComponent { Property = "constraint", Op = FilterOperator.Equal, Value = filter });

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
                        codeVals = EclHandler.ExecuteEclQuery(filter);
                        ValueSet.ExpansionComponent es = new ValueSet.ExpansionComponent();
                        foreach (Coding codeVal in codeVals)
                        {
                            es.Contains.Add(new ValueSet.ContainsComponent { Code = codeVal.Code, Display = codeVal.Display, System = cs.System });   
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

