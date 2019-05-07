namespace Vintage.AppServices.BusinessClasses.FHIR.CodeSystems
{
    using System;
    using System.Collections.Generic;
    using Hl7.Fhir.Model;
    using Vintage.AppServices.DataAccessClasses;

    /// <summary>
    ///  NZMT - NZ Medicines Terminology (as used by NZ ULM - NZ Universal List of Medicines)
    /// </summary>

    public class NzMt
    {

        public const string TITLE = "NZMT";
        public const string DESCRIPTION = "New Zealand Medicines Terminology";
        public const string URI = "http://nzmt.org.nz";
        public const string CURRENT_VERSION = "3.4.12.2";

        public CodeSystem codeSystem { get; set; }
        public ValueSet valueSet { get; set; }

        internal string vsId = string.Empty;
        internal string vsTitle = string.Empty;
        internal string vsDescription = string.Empty;
        internal string vsURL = string.Empty;

        public NzMt()
        {
            this.FillValues(TerminologyOperation.define_vs, string.Empty, string.Empty, string.Empty, string.Empty, -1, -1);
        }

        public NzMt(TerminologyOperation termOp, string version, string code, string filter, string termType, int offsetNo, int countNo)
        {
            this.FillValues(termOp, version, code, filter, termType, offsetNo, countNo);
        }

        private void FillValues(TerminologyOperation termOp, string version, string code, string filter, string vsIdentifier, int offsetNo, int countNo)
        {

            this.GetVsProperties(vsIdentifier);

            this.valueSet = new ValueSet();
            this.codeSystem = new CodeSystem();

            this.valueSet.Id = this.vsId;
            this.codeSystem.Id = "NZMT";

            this.codeSystem.CaseSensitive = false;
            this.codeSystem.Content = CodeSystem.CodeSystemContentMode.NotPresent;
            this.codeSystem.Experimental = false;
            this.codeSystem.Compositional = false;
            this.codeSystem.VersionNeeded = false;

            // Code System properties
            this.codeSystem.Property.Add(new CodeSystem.PropertyComponent { Code = "substance", Description = "The Generic Substance (Medicinal Product) & ingredient data relating to this Code.", Type = CodeSystem.PropertyType.Code });

            // there is no Value Set that contains the whole of NZMT
            //this.codeSystem.ValueSet = "http://nzmt.org.nz/vs";

            this.valueSet.Url = this.vsURL;
            this.codeSystem.Url = NzMt.URI;

            this.valueSet.Title = this.vsTitle;
            this.codeSystem.Title = NzMt.TITLE;

            this.valueSet.Name = this.valueSet.Id;
            this.codeSystem.Name = this.codeSystem.Id;

            this.valueSet.Description = new Markdown(this.vsDescription);
            this.codeSystem.Description = new Markdown(NzMt.DESCRIPTION);

            this.valueSet.Version = NzMt.CURRENT_VERSION;
            this.codeSystem.Version = NzMt.CURRENT_VERSION;

            this.valueSet.Experimental = true;

            this.valueSet.Status = PublicationStatus.Active;
            this.codeSystem.Status = PublicationStatus.Active;

            this.valueSet.Date = Hl7.Fhir.Model.Date.Today().Value;
            this.codeSystem.Date = Hl7.Fhir.Model.Date.Today().Value;

            this.valueSet.Publisher = "Patients First Ltd";
            this.codeSystem.Publisher = "nzulm.org.nz";

            this.valueSet.Copyright = new Markdown("© 2010+ New Zealand Crown Copyright");
            this.codeSystem.Copyright = new Markdown("© 2010+ New Zealand Crown Copyright");

            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "peter.jordan@patientsfirst.org.nz" };
            ContactDetail cd = new ContactDetail();
            cd.Telecom.Add(cp);
            this.valueSet.Contact.Add(cd);
            this.codeSystem.Contact.Add(cd);

            ValueSet.ConceptSetComponent cs = new ValueSet.ConceptSetComponent();
            ValueSet.ExpansionComponent es = new ValueSet.ExpansionComponent();

            cs.System = this.codeSystem.Url;
            cs.Version = this.codeSystem.Version;

            if (!string.IsNullOrEmpty(vsIdentifier))
            {
                cs.Filter.Add(new ValueSet.FilterComponent { Property = "TermType", Op = FilterOperator.Equal, Value = vsIdentifier });
            }

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
                        codeVals = NzUlmSearch.GetNzmtCombinedByCode(code);
                        if (codeVals.Count > 0 || termOp == TerminologyOperation.validate_code)
                        {
                            // create filter as need to subsequently check that it belongs in the passed Value Set
                            filter = codeVals[0].Display;
                        }
                    }

                    if (termOp == TerminologyOperation.expand || termOp == TerminologyOperation.validate_code)
                    {
                        if (vsId.StartsWith("NZULM-PT-"))
                        {
                            codeVals = NzUlmSearch.GetNZULMPrescribingTerms(filter, vsId.Replace("NZULM-PT-", "").ToLower());
                        }
                        else if (vsId == "NZULM-CTPP")
                        {
                            codeVals = NzUlmSearch.GetContaineredTradeProductPackByTerm(filter);
                        }
                        else if (vsId == "NZULM-MP")
                        {
                            codeVals = NzUlmSearch.GetMedicinalProductByTerm(filter);
                        }
                        else if (vsId == "NZULM-MPP")
                        {
                            codeVals = NzUlmSearch.GetMedicinalProductPackByTerm(filter);
                        }
                        else if (vsId == "NZULM-MPUU")
                        {
                            codeVals = NzUlmSearch.GetMedicinalProductUnitOfUseByTerm(filter);
                        }
                        else if (vsId == "NZULM-TP")
                        {
                            codeVals = NzUlmSearch.GetTradeProductByTerm(filter);
                        }
                        else if (vsId == "NZULM-TPP")
                        {
                            codeVals = NzUlmSearch.GetTradeProductPackByTerm(filter);
                        }
                        else if (vsId == "NZULM-TPUU")
                        {
                            codeVals = NzUlmSearch.GetTradeProductUnitOfUseByTerm(filter);
                        }
                        else
                        {
                            throw new Exception(TerminologyValueSet.UNFOUND_VALUESET);
                        }
                    }

                    // filtering performed at DB Layer, so add all returned concepts
                    foreach (Coding codeVal in codeVals)
                    {
                        ValueSet.DesignationComponent desig = new ValueSet.DesignationComponent {  ElementId = "NZULM", Value = codeVal.Version };
                        cs.Concept.Add(new ValueSet.ConceptReferenceComponent { Code = codeVal.Code, Display = codeVal.Display });
                        es.Contains.Add(new ValueSet.ContainsComponent { Code = codeVal.Code, Display = codeVal.Display, System = cs.System });
                        this.codeSystem.Concept.Add(new CodeSystem.ConceptDefinitionComponent { Code = codeVal.Code, Display = codeVal.Display, Definition = codeVal.Version, ElementId = codeVal.ElementId});
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

        internal void GetVsProperties(string vsIdentifier)
        {
            if (vsIdentifier == "NZULM_Containered_Trade_Product_Pack" || vsIdentifier == "http://itp.patientsfirst.org.nz/ValueSet/NzulmCtpp")
            {
                this.vsId = "NZULM-CTPP";
                this.vsTitle = "NZULM_Containered_Trade_Product_Pack";
                this.vsDescription = "NZULM Containered Trade Product Pack";
                this.vsURL = "http://itp.patientsfirst.org.nz/ValueSet/NzulmCtpp";
            }
            else if (vsIdentifier == "NZULM_Medicinal_Product" || vsIdentifier == "http://itp.patientsfirst.org.nz/ValueSet/NzulmMp")
            {
                this.vsId = "NZULM-MP";
                this.vsTitle = "NZULM_Medicinal_Product";
                this.vsDescription = "NZULM Medicinal Product";
                this.vsURL = "http://itp.patientsfirst.org.nz/ValueSet/NzulmMp";
            }
            else if (vsIdentifier == "NZULM_Medicinal_Product_Pack" || vsIdentifier == "http://itp.patientsfirst.org.nz/ValueSet/NzulmMpp")
            {
                this.vsId = "NZULM-MPP";
                this.vsTitle = "NZULM_Medicinal_Product_Pack";
                this.vsDescription = "NZULM Medicinal Product Pack";
                this.vsURL = "http://itp.patientsfirst.org.nz/ValueSet/NzulmMpp";
            }
            else if (vsIdentifier == "NZULM_Medicinal_Product_Unit_Of_Use" || vsIdentifier == "http://itp.patientsfirst.org.nz/ValueSet/NzulmMpuu")
            {
                this.vsId = "NZULM-MPUU";
                this.vsTitle = "NZULM_Medicinal_Product_Unit_Of_Use";
                this.vsDescription = "NZULM Medicinal Product Unit Of Use";
                this.vsURL = "http://itp.patientsfirst.org.nz/ValueSet/NzulmMpuu";
            }
            else if (vsIdentifier == "NZULM_Trade_Product" || vsIdentifier == "http://itp.patientsfirst.org.nz/ValueSet/NzulmTp")
            {
                this.vsId = "NZULM-TP";
                this.vsTitle = "NZULM_Trade_Product";
                this.vsDescription = "NZULM Trade Product";
                this.vsURL = "http://itp.patientsfirst.org.nz/ValueSet/NzulmTp";
            }
            else if (vsIdentifier == "NZULM_Trade_Product_Pack" || vsIdentifier == "http://itp.patientsfirst.org.nz/ValueSet/NzulmTpp")
            {
                this.vsId = "NZULM-TPP";
                this.vsTitle = "NZULM_Trade_Product_Pack";
                this.vsDescription = "NZULM Trade Product Pack";
                this.vsURL = "http://itp.patientsfirst.org.nz/ValueSet/NzulmTpp";
            }
            else if (vsIdentifier == "NZULM_Trade_Product_Unit_Of_Use" || vsIdentifier == "http://itp.patientsfirst.org.nz/ValueSet/NzulmTpuu")
            {
                this.vsId = "NZULM-TPUU";
                this.vsTitle = "NZULM_Trade_Product_Unit_Of_Use";
                this.vsDescription = "NZULM Trade Product Unit Of Use";
                this.vsURL = "http://itp.patientsfirst.org.nz/ValueSet/NzulmTpuu";
            }
            else if (vsIdentifier == "NZULM_Prescribing_Terms" || vsIdentifier == "http://itp.patientsfirst.org.nz/ValueSet/NzulmPrescribingTerms")
            {
                this.vsId = "NZULM-PT-ALL";
                this.vsTitle = "NZULM_Prescribing_Terms";
                this.vsDescription = "ALL Prescribing Terms from the NZ Universal List of Medicines";
                this.vsURL = "http://itp.patientsfirst.org.nz/ValueSet/NzulmPrescribingTerms";
            }
            else if (vsIdentifier == "NZULM_Prescribing_Terms_Generic" || vsIdentifier == "http://itp.patientsfirst.org.nz/ValueSet/NzulmPrescribingTermsGeneric")
            {
                this.vsId = "NZULM-PT-GENERIC";
                this.vsTitle = "NZULM_Prescribing_Terms_Generic";
                this.vsDescription = "Generic Prescribing Terms from the NZ Universal List of Medicines";
                this.vsURL = "http://itp.patientsfirst.org.nz/ValueSet/NzulmPrescribingTermsGeneric";
            }
            else if (vsIdentifier == "NZULM_Prescribing_Terms_Trade" || vsIdentifier == "http://itp.patientsfirst.org.nz/ValueSet/NzulmPrescribingTermsTrade")
            {
                this.vsId = "NZULM-PT-TRADE";
                this.vsTitle = "NZULM_Prescribing_Terms_Trade";
                this.vsDescription = "Trade Prescribing Terms from the NZ Universal List of Medicines";
                this.vsURL = "http://itp.patientsfirst.org.nz/ValueSet/NzulmPrescribingTermsTrade";
            }

        }

        internal static TerminologyCapabilities.CodeSystemComponent GetCapabilities()
        {
            TerminologyCapabilities.CodeSystemComponent csc = new TerminologyCapabilities.CodeSystemComponent
            {
                Uri = NzMt.URI
            };

            TerminologyCapabilities.VersionComponent vc = new TerminologyCapabilities.VersionComponent
            {
                Code = NzMt.CURRENT_VERSION,
                IsDefault = true,
                Compositional = false
            };

            vc.LanguageElement.Add(new Code("en-NZ"));

            TerminologyCapabilities.FilterComponent filt_1 = new TerminologyCapabilities.FilterComponent
            {
                CodeElement = new Code("TermType")
            };

            filt_1.OpElement.Add(new Code("ValueSet-expand"));
            vc.Filter.Add(filt_1);

            vc.PropertyElement.Add(new Code("code"));
            vc.PropertyElement.Add(new Code("system"));
            vc.PropertyElement.Add(new Code("version"));
            vc.PropertyElement.Add(new Code("definition"));
            vc.PropertyElement.Add(new Code("designation"));
            vc.PropertyElement.Add(new Code("substance"));
           
            csc.Version.Add(vc);

            return csc;
        }

    }
}

