namespace Vintage.AppServices.BusinessClasses.FHIR.ConceptMaps
{
    using System.Collections.Generic;
    using Hl7.Fhir.Model;
    using Vintage.AppServices.DataAccessClasses;
    using Vintage.AppServices.BusinessClasses.FHIR.CodeSystems;

    /// <summary>
    ///  NZ Medicinal Product mapping to SCT Concepts
    /// </summary>

    public class NzMpToSCT
    {

        public const string REFSET_ID = "311000210107";

        public ConceptMap conceptMap { get; set; }

        public NzMpToSCT()
        {
            this.FillValues(string.Empty, string.Empty);
        }

        public NzMpToSCT(string version, string nzmpCode)
        {
            this.FillValues(version, nzmpCode);
        }

        private void FillValues(string version, string nzmpCode)
        {
            this.conceptMap = new ConceptMap();

            this.conceptMap.Id = "NZMP_SCT";
            this.conceptMap.Url = ServerCapability.TERMINZ_CANONICAL + "/ConceptMap/NzMp_Sct";
          
            this.conceptMap.Name = "NZMT medicinal product to SNOMED CT map";
            this.conceptMap.Description = new Markdown("A mapping between NZMT Medicinal Products and SNOMED CT, published by NZMT in May 2018.");
            this.conceptMap.Version = "20180501";
            this.conceptMap.Status = PublicationStatus.Draft;
            this.conceptMap.Experimental = true;
            this.conceptMap.Publisher = "Ministry of Health";
            this.conceptMap.Date = new FhirDateTime(2018, 05, 1).Value;
            this.conceptMap.Purpose = new Markdown("To begin alignment between NZMT and SNOMED CT");
            this.conceptMap.Copyright = new Markdown("© 2018+ New Zealand Crown Copyright");

            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "peter.jordan@patientsfirst.org.nz" };
            ContactDetail cd = new ContactDetail();
            cd.Telecom.Add(cp);
            cd.Name = "Patients First Ltd";
            this.conceptMap.Contact.Add(cd);

            string sourceCodeSystemUri = NzMt.URI;
            string sourceValueSetUri = @"http://itp.patientsfirst.org.nz/ValueSet/NzulmMp";

            string targetCodeSystemUri = FhirSnomed.URI;
            string targetValueSetUri = @"http://snomed.info/sct?fhir_vs";

            this.conceptMap.Source = new FhirUri(sourceValueSetUri);
            this.conceptMap.Target = new FhirUri(targetValueSetUri);

            if ((string.IsNullOrEmpty(version) || version == this.conceptMap.Version))
            {
                List<Coding> map = SnomedCtSearch.GetConceptMap_NZ(REFSET_ID, nzmpCode);

                ConceptMap.GroupComponent gc = new ConceptMap.GroupComponent();
                gc.Source = sourceCodeSystemUri;
                gc.Target = targetCodeSystemUri;

                foreach (Coding mv in map)
                {
                    ConceptMap.ConceptMapEquivalence cme = ConceptMap.ConceptMapEquivalence.Equivalent;
                    ConceptMap.SourceElementComponent sec = new ConceptMap.SourceElementComponent { Code = mv.Version, Display = mv.System };
                    ConceptMap.TargetElementComponent tec = new ConceptMap.TargetElementComponent { Code = mv.Code, Equivalence = cme, Display = mv.Display };

                    sec.Target.Add(tec);
                    gc.Element.Add(sec);
                }

                this.conceptMap.Group.Add(gc);
            }

        }

    }
}

