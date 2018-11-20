namespace Vintage.AppServices.BusinessClasses.FHIR.ConceptMaps
{
    using Hl7.Fhir.Model;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using Vintage.AppServices.BusinessClasses.FHIR.CodeSystems;

    /// <summary>
    ///  NZ Ethnicity Code Mappings
    /// </summary>

    public class NZEthnicityLevels
    {

        public ConceptMap conceptMap { get; set; }

        public NZEthnicityLevels()
        {
            this.FillValues(string.Empty, string.Empty, string.Empty, string.Empty);
        }

        public NZEthnicityLevels(string version, string ethCode, string sourceSystem, string targetSystem)
        {
            this.FillValues(version, ethCode, sourceSystem, targetSystem);
        }

        private void FillValues(string version, string ethcode, string sourceSystem, string targetSystem)
        {
            this.conceptMap = new ConceptMap();

            this.conceptMap.Id = "NZ_ETHNICITY";
            this.conceptMap.Url = ServerCapability.TERMINZ_CANONICAL + "/ConceptMap/NzEthnicityLevels";

            this.conceptMap.Name = "NZ Ethnicity Level (2-4) Mappings";
            this.conceptMap.Description = new Markdown("Mappings between NZ Ethnicity Levels 2, 3 and 4.");
            this.conceptMap.Version = "20161209";
            this.conceptMap.Status = PublicationStatus.Draft;
            this.conceptMap.Experimental = true;
            this.conceptMap.Publisher = "Ministry of Health";
            this.conceptMap.Date = new FhirDateTime(2016, 10, 26).Value;
            this.conceptMap.Purpose = new Markdown("To aid conversions of Ethnicity Codes held at different levels.");
            this.conceptMap.Copyright = new Markdown("© 2010+ New Zealand Crown Copyright");

            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "peter.jordan@patientsfirst.org.nz" };
            ContactDetail cd = new ContactDetail();
            cd.Telecom.Add(cp);
            cd.Name = "Patients First Ltd";
            this.conceptMap.Contact.Add(cd);

            this.conceptMap.Source = new FhirUri(sourceSystem);
            this.conceptMap.Target = new FhirUri(targetSystem);

            // determine which maps have been requested or all
            bool allMaps = true;
            string mapping = string.Empty;
            string levels = "234";
            string sourceLevel =  sourceSystem.Substring(sourceSystem.Length-1, 1);
            string targetLevel =  targetSystem.Substring(targetSystem.Length-1, 1);

            if (levels.Contains(sourceLevel) && levels.Contains(targetLevel))
            {
                mapping = sourceLevel + targetLevel;
                allMaps = false;
            }

            // get ValueSets for Levels 2-4
            NameValueCollection queryParams = new NameValueCollection();
            ValueSet vsL2 = (ValueSet)TerminologyValueSet.PerformOperation("NzEthnicityL2", "$expand", queryParams);
            ValueSet vsL3 = (ValueSet)TerminologyValueSet.PerformOperation("NzEthnicityL3", "$expand", queryParams);
            ValueSet vsL4 = (ValueSet)TerminologyValueSet.PerformOperation("NzEthnicityL4", "$expand", queryParams);

            // Level 2-3 mappings

            Dictionary<string, string> mapVals23 = new Dictionary<string, string>();
            mapVals23.Add("10", "100");
            mapVals23.Add("11", "111");
            mapVals23.Add("12", "129");
            mapVals23.Add("21", "211");
            mapVals23.Add("30", "300");
            mapVals23.Add("31", "311");
            mapVals23.Add("32", "321");
            mapVals23.Add("33", "331");
            mapVals23.Add("34", "341");
            mapVals23.Add("35", "351");
            mapVals23.Add("36", "361");
            mapVals23.Add("37", "371");
            mapVals23.Add("40", "400");
            mapVals23.Add("41", "414");
            mapVals23.Add("42", "421");
            mapVals23.Add("43", "431");
            mapVals23.Add("44", "444");
            mapVals23.Add("51", "511");
            mapVals23.Add("52", "521");
            mapVals23.Add("53", "531");
            mapVals23.Add("61", "611");
            mapVals23.Add("94", "944");
            mapVals23.Add("95", "955");
            mapVals23.Add("96", "966");
            mapVals23.Add("97", "977");
            mapVals23.Add("98", "988");
            mapVals23.Add("99", "999");

            ConceptMap.GroupComponent gc23 = new ConceptMap.GroupComponent();
            gc23.Source = NzEthnicityL2.URI;
            gc23.Target = NzEthnicityL3.URI;

            foreach (KeyValuePair<string, string> mapVal in mapVals23)
            {
                ConceptMap.ConceptMapEquivalence cme = ConceptMap.ConceptMapEquivalence.Narrower;
                ConceptMap.SourceElementComponent sec = new ConceptMap.SourceElementComponent { Code = mapVal.Key, Display = GetCodeDisplay(vsL2,mapVal.Key) };
                ConceptMap.TargetElementComponent tec = new ConceptMap.TargetElementComponent { Code = mapVal.Value, Equivalence = cme, Display = GetCodeDisplay(vsL3, mapVal.Value) };
                sec.Target.Add(tec);
                gc23.Element.Add(sec);
            }

            if (allMaps || mapping == "23")
            {
                this.conceptMap.Group.Add(gc23);
            }

            // Level 2-4 mappings
            Dictionary<string, string> mapVals24 = new Dictionary<string, string>();            
            mapVals24.Add("10", "10000");
            mapVals24.Add("11", "11111");
            mapVals24.Add("12", "12999");
            mapVals24.Add("21", "21111");
            mapVals24.Add("30", "30000");
            mapVals24.Add("31", "31111");
            mapVals24.Add("32", "32100");
            mapVals24.Add("33", "33111");
            mapVals24.Add("34", "34111");
            mapVals24.Add("35", "35111");
            mapVals24.Add("36", "36111");
            mapVals24.Add("37", "37199");
            mapVals24.Add("40", "40000");
            mapVals24.Add("41", "41499");
            mapVals24.Add("42", "42199");
            mapVals24.Add("43", "43199");
            mapVals24.Add("44", "44499");
            mapVals24.Add("51", "51199");
            mapVals24.Add("52", "52199");
            mapVals24.Add("53", "53199");
            mapVals24.Add("61", "61199");
            mapVals24.Add("94", "94444");
            mapVals24.Add("95", "95555");
            mapVals24.Add("96", "96666");
            mapVals24.Add("97", "97777");
            mapVals24.Add("98", "98888");
            mapVals24.Add("99", "99999");

            ConceptMap.GroupComponent gc24 = new ConceptMap.GroupComponent();
            gc24.Source = NzEthnicityL2.URI;
            gc24.Target = NzEthnicityL4.URI;

            foreach (KeyValuePair<string, string> mapVal in mapVals24)
            {
                ConceptMap.ConceptMapEquivalence cme = ConceptMap.ConceptMapEquivalence.Narrower;
                ConceptMap.SourceElementComponent sec = new ConceptMap.SourceElementComponent { Code = mapVal.Key, Display = GetCodeDisplay(vsL2, mapVal.Key) };
                ConceptMap.TargetElementComponent tec = new ConceptMap.TargetElementComponent { Code = mapVal.Value, Equivalence = cme, Display = GetCodeDisplay(vsL4, mapVal.Value) };
                sec.Target.Add(tec);
                gc24.Element.Add(sec);
            }

            if (allMaps || mapping == "24")
            {
                this.conceptMap.Group.Add(gc24);
            }

            // Level 3 to 4
            ConceptMap.GroupComponent gc34 = new ConceptMap.GroupComponent();
            gc34.Source = NzEthnicityL3.URI;
            gc34.Target = NzEthnicityL4.URI;

            foreach (ValueSet.ContainsComponent ec in vsL3.Expansion.Contains)
            {
                ConceptMap.ConceptMapEquivalence cme = ConceptMap.ConceptMapEquivalence.Narrower;
                string targetCode = ec.Code + ec.Code.Substring(2, 1) + ec.Code.Substring(2, 1);
                ConceptMap.SourceElementComponent sec = new ConceptMap.SourceElementComponent { Code = ec.Code, Display = ec.Display };
                ConceptMap.TargetElementComponent tec = new ConceptMap.TargetElementComponent { Code = targetCode, Equivalence = cme, Display = GetCodeDisplay(vsL4, targetCode) };
                sec.Target.Add(tec);
                gc34.Element.Add(sec);
            }

            if (allMaps || mapping == "34")
            {
                this.conceptMap.Group.Add(gc34);
            }

            // Level 3 to 2
            ConceptMap.GroupComponent gc32 = new ConceptMap.GroupComponent();
            gc32.Source = NzEthnicityL3.URI;
            gc32.Target = NzEthnicityL2.URI;

            foreach (ValueSet.ContainsComponent ec in vsL3.Expansion.Contains)
            {
                ConceptMap.ConceptMapEquivalence cme = ConceptMap.ConceptMapEquivalence.Wider;
                ConceptMap.SourceElementComponent sec = new ConceptMap.SourceElementComponent { Code = ec.Code, Display = ec.Display };
                ConceptMap.TargetElementComponent tec = new ConceptMap.TargetElementComponent { Code = ec.Code.Substring(0, 2), Equivalence = cme, Display = GetCodeDisplay(vsL2, ec.Code.Substring(0, 2)) };
                sec.Target.Add(tec);
                gc32.Element.Add(sec);
            }

            if (allMaps || mapping == "32")
            {
                this.conceptMap.Group.Add(gc32);
            }

            // Level 4 to 3

            ConceptMap.GroupComponent gc43 = new ConceptMap.GroupComponent();
            gc43.Source = NzEthnicityL4.URI;
            gc43.Target = NzEthnicityL3.URI;

            foreach (ValueSet.ContainsComponent ec in vsL4.Expansion.Contains)
            {
                string targetCode = (ec.Code == "61118")  ? "111" : ec.Code.Substring(0, 3);
                ConceptMap.ConceptMapEquivalence cme = ConceptMap.ConceptMapEquivalence.Wider;
                ConceptMap.SourceElementComponent sec = new ConceptMap.SourceElementComponent { Code = ec.Code, Display = ec.Display };
                ConceptMap.TargetElementComponent tec = new ConceptMap.TargetElementComponent { Code = targetCode, Equivalence = cme, Display = GetCodeDisplay(vsL3, targetCode) };
                sec.Target.Add(tec);
                gc43.Element.Add(sec);                
            }

            if (allMaps || mapping == "43")
            {
                this.conceptMap.Group.Add(gc43);
            }

            // Level 4 to 2

            ConceptMap.GroupComponent gc42 = new ConceptMap.GroupComponent();
            gc42.Source = NzEthnicityL4.URI;
            gc42.Target = NzEthnicityL2.URI;

            foreach (ValueSet.ContainsComponent ec in vsL4.Expansion.Contains)
            {
                string targetCode = (ec.Code == "61118") ? "11" : ec.Code.Substring(0, 2);
                ConceptMap.ConceptMapEquivalence cme = ConceptMap.ConceptMapEquivalence.Wider;
                ConceptMap.SourceElementComponent sec = new ConceptMap.SourceElementComponent { Code = ec.Code, Display = ec.Display };
                ConceptMap.TargetElementComponent tec = new ConceptMap.TargetElementComponent { Code = targetCode, Equivalence = cme, Display = GetCodeDisplay(vsL2, targetCode) };
                sec.Target.Add(tec);
                gc42.Element.Add(sec);
            }

            if (allMaps || mapping == "42")
            {
                this.conceptMap.Group.Add(gc42);
            }

        }

        private string GetCodeDisplay(ValueSet vs, string code)
        {
            string retValue = string.Empty;
            foreach (ValueSet.ContainsComponent ec in vs.Expansion.Contains)
            {
                if (ec.Code == code)
                {
                    retValue = ec.Display;
                    break;
                }
            }
            return retValue;
        }
    }
}

