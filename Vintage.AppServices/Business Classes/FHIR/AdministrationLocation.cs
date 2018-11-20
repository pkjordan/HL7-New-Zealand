namespace Vintage.AppServices.BusinessClasses.FHIR
{
    using Hl7.Fhir.Model;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Xml.Linq;
    using Vintage.AppServices.DataAccessClasses;

    public static class AdministrationLocation
    {
        internal const string NAMING_SYSTEM_IDENTIFIER = "https://standards.digital.health.nz/id/hpi-facility";
   
        public static Resource GetRequest(string id, NameValueCollection queryParam)
        {

            Bundle locBundle = new Bundle
            {
                Id = Guid.NewGuid().ToString(),
                Type = Bundle.BundleType.Searchset
            };
            locBundle.Link.Add(new Bundle.LinkComponent { Url = ServerCapability.TERMINZ_CANONICAL + "/Location", Relation = "self" });

            Location location = new Location();
            Organization org = new Organization();

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

            //CodeableConcept hpiFac = new CodeableConcept { Text = "HPI-FAC" };

            try
            {

                List<HpiFacility> facilities = SnomedCtSearch.GetLocations(identifier, name, address, type);

                foreach (HpiFacility fac in facilities)
                {
                    bool addLocation = true;

                    Address locAddress = Utilities.GetAddress(fac.FacilityAddress.Trim());

                    if (!string.IsNullOrEmpty(address_city) && locAddress.City.ToUpper() != address_city.ToUpper())
                    {
                        addLocation = false;
                    }

                    if (!string.IsNullOrEmpty(address_postalcode) && locAddress.PostalCode.ToUpper() != address_postalcode.ToUpper())
                    {
                        addLocation = false;
                    }

                    if (addLocation)
                    {
                        bool addOrg = false;
                        org = new Organization();
                        location = new Location
                        {
                            Id = fac.FacilityId.Trim()
                        };
                        location.Identifier.Add(new Identifier { Value = fac.FacilityId.Trim(), System = NAMING_SYSTEM_IDENTIFIER });
                        location.Name = fac.FacilityName.Trim();
                        location.Status = Location.LocationStatus.Active;
                        location.Mode = Location.LocationMode.Instance;
                        location.Type.Add(new CodeableConcept { Text = fac.FacilityTypeName.Trim() });
                        location.Address = locAddress;
                        AddNarrative(location);

                        if (!string.IsNullOrEmpty(fac.OrganisationId))
                        {
                            try
                            {
                                org = (Organization)AdministrationOrganisation.GetRequest(fac.OrganisationId, null);
                                location.ManagingOrganization = new ResourceReference { Reference = fac.OrganisationId };
                                addOrg = true;
                            }
                            catch { }
                        }

                        locBundle.AddResourceEntry(location, ServerCapability.TERMINZ_CANONICAL + "/Location/" + fac.FacilityId.Trim());
                        matches++;

                        if (addOrg)
                        {
                            locBundle.AddResourceEntry(org, ServerCapability.TERMINZ_CANONICAL + "/Organization" + "/" + fac.OrganisationId.Trim());
                        }

                    }
                }

                if (matches == 0)
                {
                    return OperationOutcome.ForMessage("No Locations match search parameter values.", OperationOutcome.IssueType.NotFound, OperationOutcome.IssueSeverity.Information);
                }

                locBundle.Total = matches;
            }
            catch (Exception ex)
            {
                return OperationOutcome.ForMessage("Error: " + ex.Message, OperationOutcome.IssueType.Invalid, OperationOutcome.IssueSeverity.Error);
            }

            // always return bundle because of contained resources <TODO> implement _include so user can specify this

            return locBundle;
        }

        internal static Location AddNarrative(Location location)
        {
            // create display text for Location Resource
            string textString = string.Empty;

            try
            {

                XNamespace ns = "http://www.w3.org/1999/xhtml";

                var summary = new XElement(ns + "div",
                    new XElement(ns + "h2", location.Name),
                    new XElement(ns + "table",
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Identifier"),
                        new XElement(ns + "td", location.Identifier[0].Value)
                        ),
                        new XElement(ns + "tr",
                         new XElement(ns + "td", "Identifier System"),
                        new XElement(ns + "td", location.Identifier[0].System)
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Status"),
                        new XElement(ns + "td", location.Status.ToString())
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Mode"),
                        new XElement(ns + "td", location.Mode.ToString())
                        ),
                         new XElement(ns + "tr",
                        new XElement(ns + "td", "Type"),
                        new XElement(ns + "td", location.TypeName)
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Address"),
                        new XElement(ns + "td", location.Address.Text)
                        )
                     )
                  );

                textString = summary.ToString();
            }
            catch
            { }

            location.Text = new Narrative
            {
                Status = Narrative.NarrativeStatus.Generated,
                Div = textString
            };

            return location;
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