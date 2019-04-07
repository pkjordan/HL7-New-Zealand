namespace Vintage.AppServices.BusinessClasses.FHIR.CodeSystems
{
    using Hl7.Fhir.Model;
    using System.Collections.Generic;
    using Vintage.AppServices.DataAccessClasses;

    /// <summary>
    ///  RxNorm - USA Medicines Terminology
    /// </summary>

    public class FhirRxNorm
    {

        public const string TITLE = "RXNORM";
        public const string DESCRIPTION = "RxNorm - normalized names for clinical drugs in the USA";
        public const string URI = "http://www.nlm.nih.gov/research/umls/rxnorm";
        public const string CURRENT_VERSION = "02042019";

        public CodeSystem codeSystem { get; set; }
        public ValueSet valueSet { get; set; }

        internal string vsId = string.Empty;
        internal string vsTitle = string.Empty;
        internal string vsDescription = string.Empty;
        internal string vsURL = string.Empty;

        public FhirRxNorm()
        {
            this.FillValues(TerminologyOperation.define_vs, string.Empty, string.Empty, string.Empty, string.Empty, -1, -1);
        }

        public FhirRxNorm(TerminologyOperation termOp, string version, string code, string filter, string termType, int offsetNo, int countNo)
        {
            this.FillValues(termOp, version, code, filter, termType, offsetNo, countNo);
        }

        private void FillValues(TerminologyOperation termOp, string version, string code, string filter, string vsIdentifier, int offsetNo, int countNo)
        {

            this.valueSet = new ValueSet();
            this.codeSystem = new CodeSystem();

            string idSuffix = filter.Replace(" ", "_");
            this.valueSet.Id = "fhir_rxnorm_vs_" + idSuffix;
            this.codeSystem.Id = "RXNORM";

            this.codeSystem.CaseSensitive = false;
            this.codeSystem.Content = CodeSystem.CodeSystemContentMode.NotPresent;
            this.codeSystem.Experimental = false;
            this.codeSystem.Compositional = false;
            this.codeSystem.VersionNeeded = false;

            // Code System properties
            //this.codeSystem.Property.Add(new CodeSystem.PropertyComponent { Code = "substance", Description = "The Generic Substance (Medicinal Product) & ingredient data relating to this Code.", Type = CodeSystem.PropertyType.Code });

            // Value Set that contains all RxNorm CUIs
            this.codeSystem.ValueSet = "http://www.nlm.nih.gov/research/umls/rxnorm/vs";

            this.valueSet.Url = ServerCapability.TERMINZ_CANONICAL + "/ValueSet/rxnorm/" + idSuffix;
            this.codeSystem.Url = FhirRxNorm.URI;

            this.valueSet.Title = FhirRxNorm.TITLE;
            this.codeSystem.Title = FhirRxNorm.TITLE;

            this.valueSet.Name = this.valueSet.Id;
            this.codeSystem.Name = this.codeSystem.Id;

            this.valueSet.Description = new Markdown(FhirRxNorm.DESCRIPTION);
            this.codeSystem.Description = new Markdown(FhirRxNorm.DESCRIPTION);

            this.valueSet.Version = FhirRxNorm.CURRENT_VERSION;
            this.codeSystem.Version = FhirRxNorm.CURRENT_VERSION;

            this.valueSet.Experimental = true;

            this.valueSet.Status = PublicationStatus.Active;
            this.codeSystem.Status = PublicationStatus.Active;

            this.valueSet.Date = Hl7.Fhir.Model.Date.Today().Value;
            this.codeSystem.Date = Hl7.Fhir.Model.Date.Today().Value;

            this.valueSet.Publisher = "Patients First Ltd";
            this.codeSystem.Publisher = "http://www.nlm.nih.gov/";

            this.valueSet.Copyright = new Markdown("Unified Medical Language System® (UMLS®)");
            this.codeSystem.Copyright = new Markdown("Unified Medical Language System® (UMLS®)");

            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "peter.jordan@patientsfirst.org.nz" };
            ContactDetail cd = new ContactDetail();
            cd.Telecom.Add(cp);
            this.valueSet.Contact.Add(cd);
            this.codeSystem.Contact.Add(cd);

            ValueSet.ConceptSetComponent cs = new ValueSet.ConceptSetComponent();
            ValueSet.ExpansionComponent es = new ValueSet.ExpansionComponent();

            cs.System = this.codeSystem.Url;
            cs.Version = this.codeSystem.Version;

            //if (!string.IsNullOrEmpty(vsIdentifier))
            //{
            //    cs.Filter.Add(new ValueSet.FilterComponent { Property = "TTY", Op = FilterOperator.Equal, Value = vsIdentifier });
            //}

            string codeCode = string.Empty;
            string codeDisplay = string.Empty;
            string codeDefinition = string.Empty;

            if ((string.IsNullOrEmpty(version) || version == cs.Version) && termOp != TerminologyOperation.define_cs)
            {

                if (termOp != TerminologyOperation.define_vs)
                {
                    List<Coding> codeVals = new List<Coding>();

                    if (termOp == TerminologyOperation.lookup || termOp == TerminologyOperation.validate_code)
                    {
                        codeVals = RxNormSearch.GetConceptByCode(code);
                        if (codeVals.Count > 0 || termOp == TerminologyOperation.validate_code)
                        {
                            // create filter as need to subsequently check that it belongs in the passed Value Set
                            filter = codeVals[0].Display;
                        }
                    }

                    if (termOp == TerminologyOperation.expand || termOp == TerminologyOperation.validate_code)
                    {
                        codeVals = RxNormSearch.GetConceptsByTerm(filter);
                    }

                    // filtering performed at DB Layer, so add all returned concepts
                    foreach (Coding codeVal in codeVals)
                    {
                        ValueSet.DesignationComponent desig = new ValueSet.DesignationComponent { ElementId = "RXNORM", Value = codeVal.Version };
                        cs.Concept.Add(new ValueSet.ConceptReferenceComponent { Code = codeVal.Code, Display = codeVal.Display });
                        es.Contains.Add(new ValueSet.ContainsComponent { Code = codeVal.Code, Display = codeVal.Display, System = cs.System });
                        this.codeSystem.Concept.Add(new CodeSystem.ConceptDefinitionComponent { Code = codeVal.Code, Display = codeVal.Display, Definition = codeVal.Version, ElementId = codeVal.ElementId });
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
                else
                {
                    this.valueSet.Compose = new ValueSet.ComposeComponent();
                    this.valueSet.Compose.Include.Add(cs);
                }
            }
        }

        internal static string GetTermType(string tty)
        {
            string termType = string.Empty;

            if (tty=="IN") { termType = "Ingredient"; }
            else if (tty == "PIN") { termType = "Precise Ingredient"; }
            else if (tty == "MIN") { termType = "Multiple Ingredients"; }
            else if (tty == "SCDC") { termType = "Semantic Clinical Drug Component"; }
            else if (tty == "SCDF") { termType = "Semantic Clinical Drug Form"; }
            else if (tty == "SCDG") { termType = "Semantic Clinical Dose Form Group"; }
            else if (tty == "SCD") { termType = "Semantic Clinical Drug"; }
            else if (tty == "GPCK") { termType = "Generic Pack"; }
            else if (tty == "BN") { termType = "Brand Name"; }
            else if (tty == "SBDC") { termType = "Semantic Branded Drug Component"; }
            else if (tty == "SBDF") { termType = "Semantic Branded Drug Form"; }
            else if (tty == "SBDG") { termType = "Semantic Branded Dose Form Group"; }
            else if (tty == "SBD") { termType = "Semantic Branded Drug"; }
            else if (tty == "BPCK") { termType = "Brand Name Pack"; }
            else if (tty == "PSN") { termType = "Prescribable Name"; }
            else if (tty == "SY") { termType = "Synonym"; }
            else if (tty == "TMSY") { termType = "Tall Man Lettering Synonym"; }
            else if (tty == "DF") { termType = "Dose Form"; }
            else if (tty == "ET") { termType = "Dose Form Entry Term"; }
            else if (tty == "DFG") { termType = "Dose Form Group"; }
 
            return termType;
        }

        internal static TerminologyCapabilities.CodeSystemComponent GetCapabilities()
        {
            TerminologyCapabilities.CodeSystemComponent csc = new TerminologyCapabilities.CodeSystemComponent
            {
                Uri = FhirRxNorm.URI
            };

            TerminologyCapabilities.VersionComponent vc = new TerminologyCapabilities.VersionComponent
            {
                Code = FhirRxNorm.CURRENT_VERSION,
                IsDefault = true,
                Compositional = false
            };

            vc.LanguageElement.Add(new Code("en-US"));

            //TerminologyCapabilities.FilterComponent filt_1 = new TerminologyCapabilities.FilterComponent
            //{
            //    CodeElement = new Code("TermType")
            //};

            //filt_1.OpElement.Add(new Code("ValueSet-expand"));
            //vc.Filter.Add(filt_1);

            vc.PropertyElement.Add(new Code("code"));
            vc.PropertyElement.Add(new Code("system"));
            vc.PropertyElement.Add(new Code("version"));
            vc.PropertyElement.Add(new Code("definition"));
            vc.PropertyElement.Add(new Code("designation"));
            vc.PropertyElement.Add(new Code("termType"));

            csc.Version.Add(vc);

            return csc;
        }

    }
}

