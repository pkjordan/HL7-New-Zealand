namespace Vintage.AppServices.BusinessClasses.FHIR
{
    using Hl7.Fhir.Model;
    using System;
    using System.Xml.Linq;
    using Vintage.AppServices.BusinessClasses.FHIR.CodeSystems;

    public static class TerminologyCapability
    {
        public static Resource GetStatement(bool inBundle)
        {
            TerminologyCapabilities tsCapabilityStatement = new TerminologyCapabilities
            {
                Url = ServerCapability.TERMINZ_CANONICAL + "/TerminologyCapabilities/Terminz-Example",
                Id = "Terminz-Example",
                Description = new Markdown("HL7© FHIR© terminology services for use in New Zealand."),
                Name = "Patients First Terminology Server (Terminz)",
                Purpose = new Markdown("Exemplar of terminology services approach for New Zealand."),
                Publisher = "Patients First Ltd",
                Version = "3.5.0",
                Status = PublicationStatus.Draft,
                Date = "2018-11-18",
                Experimental = true,
                Copyright = new Markdown("© 2018+ Patients First Ltd"),
                LockedDate = false
            };

            ContactDetail cd = new ContactDetail { Name = "Peter Jordan" };
            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "pkjordan@xtra.co.nz" };
            cd.Telecom.Add(cp);
            tsCapabilityStatement.Contact.Add(cd);
           
            CodeableConcept cc = new CodeableConcept("http://hl7.org/fhir/ValueSet/jurisdiction", "NZL", "New Zealand", "New Zealand");
            tsCapabilityStatement.Jurisdiction.Add(cc);

            tsCapabilityStatement.CodeSystem.Add(FhirSnomed.GetCapabilities());
            tsCapabilityStatement.CodeSystem.Add(FhirLoinc.GetCapabilities());
            tsCapabilityStatement.CodeSystem.Add(NzMt.GetCapabilities());

            tsCapabilityStatement.Expansion = new TerminologyCapabilities.ExpansionComponent
            {
                Hierarchical = false,
                Paging = true,
                Incomplete = false,
                TextFilter = new Markdown("Results include synonyms, not just preferred terms.")
            };

            tsCapabilityStatement.CodeSearch = TerminologyCapabilities.CodeSearchSupport.All;

            tsCapabilityStatement.ValidateCode = new TerminologyCapabilities.ValidateCodeComponent()
            {
                Translations = false
            };

            tsCapabilityStatement.Translation = new TerminologyCapabilities.TranslationComponent()
            {
                NeedsMap = true
            };

            tsCapabilityStatement.Closure = new TerminologyCapabilities.ClosureComponent()
            {
                Translation = false
            };

            // text element

            XNamespace ns = "http://www.w3.org/1999/xhtml";

            var summary = new XElement(ns + "div",
                new XElement(ns + "h2", tsCapabilityStatement.Name),
                new XElement(ns + "p", tsCapabilityStatement.Description),
                new XElement(ns + "table",
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Purpose"),
                    new XElement(ns + "td", tsCapabilityStatement.Purpose.ToString())
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Publisher"),
                    new XElement(ns + "td", tsCapabilityStatement.Publisher)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Version"),
                    new XElement(ns + "td", tsCapabilityStatement.Version)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", "Date"),
                    new XElement(ns + "td", tsCapabilityStatement.Date)
                    )
                 ),
                new XElement(ns + "table",
                    new XElement(ns + "tr",
                    new XElement(ns + "th", "Code System"),
                    new XElement(ns + "th", "Description"),
                    new XElement(ns + "th", "URI"),
                    new XElement(ns + "th", "Version")
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", FhirSnomed.NAME),
                    new XElement(ns + "td", FhirSnomed.DESCRIPTION),
                    new XElement(ns + "td", FhirSnomed.URI),
                    new XElement(ns + "td", FhirSnomed.CURRENT_VERSION)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", FhirLoinc.NAME),
                    new XElement(ns + "td", FhirLoinc.DESCRIPTION),
                    new XElement(ns + "td", FhirLoinc.URI),
                    new XElement(ns + "td", FhirLoinc.CURRENT_VERSION)
                    ),
                    new XElement(ns + "tr",
                    new XElement(ns + "td", NzMt.NAME),
                    new XElement(ns + "td", NzMt.DESCRIPTION),
                    new XElement(ns + "td", NzMt.URI),
                    new XElement(ns + "td", NzMt.CURRENT_VERSION)
                    )
                 )
              );

            tsCapabilityStatement.Text = new Narrative
            {
                Status = Narrative.NarrativeStatus.Generated,
                Div = summary.ToString()
            };

            // place in a bundle

            if (inBundle)
            {
                Bundle tcsBundle = new Bundle
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = Bundle.BundleType.Searchset
                };

                tcsBundle.Link.Add(new Bundle.LinkComponent { Url = ServerCapability.TERMINZ_CANONICAL + "/TerminologyCapabilities", Relation = "self" });
                tcsBundle.AddResourceEntry(tsCapabilityStatement, tsCapabilityStatement.Url);
                tcsBundle.Total = tcsBundle.Entry.Count;

                return tcsBundle;
            }

            return tsCapabilityStatement;

        }


    }

}
