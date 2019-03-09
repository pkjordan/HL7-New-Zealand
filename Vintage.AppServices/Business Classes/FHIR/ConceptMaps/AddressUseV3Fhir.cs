namespace Vintage.AppServices.BusinessClasses.FHIR.ConceptMaps
{
    using System;
    using System.Collections.Generic;
    using Hl7.Fhir.Model;

    /// <summary>
    ///  Test Concept Map of V3 to FHIR Address Types
    /// </summary>

    public class AddressUseV3Fhir
    {

        public ConceptMap conceptMap { get; set; }

        public AddressUseV3Fhir()
        {
            this.FillValues(string.Empty);
        }

        public AddressUseV3Fhir(string version)
        {
            this.FillValues(version);
        }

        private void FillValues(string version)
        {
            this.conceptMap = new ConceptMap();

            this.conceptMap.Id = "101";
            this.conceptMap.Url = "http://hl7.org/fhir/ConceptMap/101";
            this.conceptMap.Identifier = new Identifier { System = "urn:ietf:rfc:3986", Value = "urn:uuid:53cd62ee-033e-414c-9f58-3ca97b5ffc3b" };

            this.conceptMap.Name = this.conceptMap.Id;
            this.conceptMap.Title = "FHIR/v3 Address Use Mapping";
            this.conceptMap.Description = new Markdown("A mapping between the FHIR and HL7 v3 AddressUse Code systems");
            this.conceptMap.Version = "C20";
            this.conceptMap.Status = PublicationStatus.Draft;
            this.conceptMap.Experimental = true;
            this.conceptMap.Publisher = "HL7, Inc";
            this.conceptMap.Date = new FhirDateTime(2012, 6, 13).Value;                
            this.conceptMap.Purpose = new Markdown("To help implementers map from HL7 v3/CDA to FHIR");
            this.conceptMap.Copyright = new Markdown("Creative Commons 0");

            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Other, Value = "http://hl7.org/fhir" };
            ContactDetail cd = new ContactDetail();
            cd.Telecom.Add(cp);
            cd.Name = "FHIR project team (example)";
            this.conceptMap.Contact.Add(cd);

            string sourceUri = @"http://hl7.org/fhir/ValueSet/address-use";
            string targetUri = @"http://terminology.hl7.org/ValueSet/v3-AddressUse";

            this.conceptMap.Source = new FhirUri(sourceUri);  // Value Set
            this.conceptMap.Target = new FhirUri(targetUri);  // Value Set

            Dictionary<string, string> mapVals = new Dictionary<string, string>();
            mapVals.Add("home", "H");
            mapVals.Add("work", "WP");
            mapVals.Add("temp", "TMP");
            mapVals.Add("old", "BAD");

            if (string.IsNullOrEmpty(version) || version == this.conceptMap.Version)
            {

                ConceptMap.GroupComponent gc = new ConceptMap.GroupComponent();
                gc.Source = "http://hl7.org/fhir/address-use";   // Code System
                gc.Target = "http://terminology.hl7.org/CodeSystem/v3-AddressUse"; // Code System

                foreach (KeyValuePair<string, string> mapVal in mapVals)
                {

                    ConceptMap.ConceptMapEquivalence cme = ConceptMap.ConceptMapEquivalence.Equivalent;

                    string comments = "";
                    string display = mapVal.Value.Replace("H", "home address").Replace("WP", "work place").Replace("TMP", "temporary address");
                    if (mapVal.Key == "old")
                    {
                        cme = ConceptMap.ConceptMapEquivalence.Disjoint;
                        comments = "In the HL7 v3 AD, old is handled by the usablePeriod element, but you have to provide a time, there's no simple equivalent of flagging an address as old";
                        display = "bad address";
                    }

                    ConceptMap.SourceElementComponent sec = new ConceptMap.SourceElementComponent { Code = mapVal.Key, Display = mapVal.Key };
                    ConceptMap.TargetElementComponent tec = new ConceptMap.TargetElementComponent { Code = mapVal.Value, Display = display, Equivalence = cme, Comment = comments};

                    sec.Target.Add(tec);
                    gc.Element.Add(sec);                    
                }

                this.conceptMap.Group.Add(gc);
            }
            else
            {
                throw new Exception(TerminologyConceptMap.UNSUPPORTED_VERSION);
            }
            
        }

    }
}

