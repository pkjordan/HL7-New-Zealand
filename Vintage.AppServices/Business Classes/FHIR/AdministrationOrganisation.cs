namespace Vintage.AppServices.BusinessClasses.FHIR
{
    using Hl7.Fhir.Model;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Xml.Linq;
    using Vintage.AppServices.DataAccessClasses;

    public static class AdministrationOrganisation
    {

        internal const string NAMING_SYSTEM_IDENTIFIER = "https://standards.digital.health.nz/id/hpi-organisation";
       
        public static Resource GetRequest(string id, NameValueCollection queryParam)
        {

            Bundle orgBundle = new Bundle
            {
                Id = Guid.NewGuid().ToString(),
                Type = Bundle.BundleType.Searchset
            };
            orgBundle.Link.Add(new Bundle.LinkComponent { Url = ServerCapability.TERMINZ_CANONICAL + "/Organization", Relation = "self" });

            Organization organization = new Organization();

            string identifier = GetIdentifier(id, queryParam);
            string address = Utilities.GetQueryValue("address", queryParam);
            string address_city = Utilities.GetQueryValue("address-city", queryParam);
            string address_postalcode = Utilities.GetQueryValue("address-postalcode", queryParam);
            string name = Utilities.GetQueryValue("name", queryParam);
            string type = Utilities.GetQueryValue("type", queryParam);
            bool idPassed = !string.IsNullOrEmpty(id);
            int matches = 0;

            // facilitate (more efficient) postcode and city filtering at DB layer
            if (!string.IsNullOrEmpty(address_postalcode) && string.IsNullOrEmpty(address))
            {
                address = address_postalcode;
            }

            if (!string.IsNullOrEmpty(address_city) && string.IsNullOrEmpty(address))
            {
                address = address_city;
            }

            if (string.IsNullOrEmpty(identifier) && string.IsNullOrEmpty(name) && string.IsNullOrEmpty(address) && string.IsNullOrEmpty(type))
            {
                return OperationOutcome.ForMessage("No valid search parameters.", OperationOutcome.IssueType.Invalid, OperationOutcome.IssueSeverity.Error);
            }

            //CodeableConcept hpiFac = new CodeableConcept { Text = "HPI-ORG" };

            List<HpiOrganisation> organisations = SnomedCtSearch.GetOrganisations(identifier, name, address, type);

            foreach (HpiOrganisation org in organisations)
            {
                bool addOrg = true;

                Address orgAddress = Utilities.GetAddress(org.OrganisationAddress.Trim());

                if (!string.IsNullOrEmpty(address_city) && orgAddress.City.ToUpper() != address_city.ToUpper())
                {
                    addOrg = false;
                }

                if (!string.IsNullOrEmpty(address_postalcode) && orgAddress.PostalCode.ToUpper() != address_postalcode.ToUpper())
                {
                    addOrg = false;
                }

                if (addOrg)
                {
                    organization = new Organization
                    {
                        Id = org.OrganisationId.Trim()
                    };
                    organization.Identifier.Add(new Identifier { Value = org.OrganisationId.Trim(), System = NAMING_SYSTEM_IDENTIFIER });                
                    organization.Name = org.OrganisationName.Trim();
                    organization.Active = true;
                    organization.Type.Add(new CodeableConcept { Text = org.OrganisationTypeName.Trim() });
                    organization.Address.Add(orgAddress);
                    AddNarrative(organization);
                    orgBundle.AddResourceEntry(organization, ServerCapability.TERMINZ_CANONICAL + "/Organization/ " + org.OrganisationId.Trim());
                    matches++;
                }
            }

            if (matches == 0)
            {
                return OperationOutcome.ForMessage("No Organisations match search parameter values.", OperationOutcome.IssueType.NotFound, OperationOutcome.IssueSeverity.Information);
            }
            else if (matches == 1 && idPassed)
            {
                return organization;
            }

            orgBundle.Total = matches;

            return orgBundle;
        }

        internal static Organization AddNarrative(Organization organisation)
        {
            // create display text for Organisation Resource
            string textString = string.Empty;

            try
            {

                XNamespace ns = "http://www.w3.org/1999/xhtml";

                var summary = new XElement(ns + "div",
                    new XElement(ns + "h2", organisation.Name),
                    new XElement(ns + "table",
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Identifier"),
                        new XElement(ns + "td", organisation.Identifier[0].Value)
                        ),
                        new XElement(ns + "tr",
                         new XElement(ns + "td", "Identifier System"),
                        new XElement(ns + "td", organisation.Identifier[0].System)
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Name"),
                        new XElement(ns + "td", organisation.Name.ToString())
                        ),
                         new XElement(ns + "tr",
                        new XElement(ns + "td", "Type"),
                        new XElement(ns + "td", organisation.Type[0].Text)
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Address"),
                        new XElement(ns + "td", organisation.Address[0].Text)
                        )
                     )
                  );

                textString = summary.ToString();
            }
            catch
            { }

            organisation.Text = new Narrative
            {
                Status = Narrative.NarrativeStatus.Generated,
                Div = textString
            };

            return organisation;
        }


        private static string GetIdentifier(string id, NameValueCollection queryParam)
        {
            string identifier = id;

            // get parameter values that might uniquely identify the Resource

            if (string.IsNullOrEmpty(id))
            {
                identifier = Utilities.GetQueryValue("_id", queryParam);
            }

            if (string.IsNullOrEmpty(identifier))
            {
                identifier = Utilities.GetQueryValue("identifier", queryParam);
            }

            return identifier;
        }
    }
}