namespace Vintage.AppServices.BusinessClasses.FHIR.ConceptMaps
{
    using System.Collections.Generic;
    using Hl7.Fhir.Model;
    using Vintage.AppServices.DataAccessClasses;
    using Vintage.AppServices.BusinessClasses.FHIR.CodeSystems;

    /// <summary>
    /// SCT Concepts Mappings to NZ Read Codes 
    /// </summary>

    public class SctToNZRead
    {

        public const string REFSET_ID = "41000210103";

        public ConceptMap conceptMap { get; set; }

        public SctToNZRead()
        {
            this.FillValues(string.Empty, string.Empty);
        }

        public SctToNZRead(string version, string sctid)
        {
            this.FillValues(version, sctid);
        }

        private void FillValues(string version, string sctid)
        {
            this.conceptMap = new ConceptMap();

            this.conceptMap.Id = "SCT_NZREAD";
            this.conceptMap.Url = ServerCapability.TERMINZ_CANONICAL + "/ConceptMap/Sct_NzRead";
            //this.conceptMap.Identifier = new Identifier { System = "urn:ietf:rfc:3986", Value = "urn:uuid:53cd62ee-033e-414c-9f58-3ca97b5ffc3b" };

            this.conceptMap.Name = "SNOMED CT to NZ Read Codes";

            if (string.IsNullOrEmpty(sctid))
            {
                this.conceptMap.Title = "Definition Only - Full Map too large to download (>100,000 elements).";
            }

            this.conceptMap.Description = new Markdown("A mapping between SNOMED CT and the NZ Read Codes, published by NHS Digital in Oct 2017 and augmented for use in NZ.");
            this.conceptMap.Version = "20170801";
            this.conceptMap.Status = PublicationStatus.Draft;
            this.conceptMap.Experimental = true;
            this.conceptMap.Publisher = "Ministry of Health";
            this.conceptMap.Date = new FhirDateTime(2017, 08, 01).Value;
            this.conceptMap.Purpose = new Markdown("To help primary care facilities translate SCT concepts to legacy Read Codes");
            this.conceptMap.Copyright = new Markdown("© 2010+ New Zealand Crown Copyright");

            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "peter.jordan@patientsfirst.org.nz" };
            ContactDetail cd = new ContactDetail();
            cd.Telecom.Add(cp);
            cd.Name = "Patients First Ltd";
            this.conceptMap.Contact.Add(cd);

            string sourceCodeSystemUri = FhirSnomed.URI;
            string sourceValueSetUri = @"http://snomed.info/sct?fhir_vs";

            string targetCodeSystemUri = @"http://health.govt.nz/read-codes";
            string targetValueSetUri = @"http://health.govt.nz/read-codes/fhir_vs";

            this.conceptMap.Source = new FhirUri(sourceValueSetUri);
            this.conceptMap.Target = new FhirUri(targetValueSetUri);

            if ((string.IsNullOrEmpty(version) || version == this.conceptMap.Version) && !string.IsNullOrEmpty(sctid))
            {
                List<Coding> map = SnomedCtSearch.GetConceptMap_NZ(REFSET_ID, sctid);

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

