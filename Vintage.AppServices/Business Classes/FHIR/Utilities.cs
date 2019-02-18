namespace Vintage.AppServices.BusinessClasses.FHIR
{
    using Hl7.Fhir.Model;
    using System.Collections.Specialized;

    /// <summary>
    /// Terminology Operation Types
    /// </summary>
    public enum TerminologyOperation
    {
        expand,
        validate_code,
        lookup,
        subsumes,
        find_matches,
        translate,
        closure,
        define_vs,
        define_cs
    }

    internal static class Utilities
    {

        internal static string GetQueryValue(string paramKey, NameValueCollection queryParam)
        {
            string rv = string.Empty;

            if (queryParam != null)
            {
                foreach (string key in queryParam)
                {
                    if (key == paramKey || key.StartsWith(paramKey + ":"))
                    {
                        rv = queryParam[key];
                    }
                }
            }

            return rv;
        }
             
        internal static bool IsDigitsOnly(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }

            return true;
        }

        internal static string StripSemanticTag(string descFilter)
        {
            string descrip = descFilter;

            try
            {
                if (!string.IsNullOrEmpty(descFilter) && descFilter.EndsWith(")"))
                {
                    descrip = descFilter.Substring(0, descFilter.IndexOf("(")).Trim();
                }
            }
            catch { }

            return descrip;
        }

        internal static Address GetAddress(string address)
        {
            Address fhirAddress = new Address();
            fhirAddress.City = string.Empty;
            fhirAddress.PostalCode = string.Empty;
            fhirAddress.Use = Address.AddressUse.Work;
            fhirAddress.Type = Address.AddressType.Physical;

            try
            {
                if (address.EndsWith(","))
                {
                    address = address.Remove(address.LastIndexOf(","), 1);
                }

                string[] addressParts = address.Split(',');
                int lineNo = 1;
                foreach (string addrPart in addressParts)
                {
                    if (lineNo == addressParts.Length)
                    {
                        string[] cityZip = addrPart.Split(' ');
                        foreach (string czPart in cityZip)
                        {
                            int xx;
                            if (int.TryParse(czPart, out xx))
                            {
                                fhirAddress.PostalCode = czPart;
                            }
                            else
                            {
                                fhirAddress.City = czPart.Trim();
                            }
                        }
                    }
                    else
                    {
                        FhirString al = new FhirString(addrPart.Trim());
                        fhirAddress.LineElement.Add(al);
                    }
                    lineNo++;
                }
                fhirAddress.Text = address;
            }
            catch { }

            return fhirAddress;
        }

    }

}
