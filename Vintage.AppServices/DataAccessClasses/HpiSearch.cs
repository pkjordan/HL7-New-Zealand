namespace Vintage.AppServices.DataAccessClasses
{
    using System.Collections.Generic;
    using System.Linq;
    using Vintage.AppServices.BusinessClasses;

    public static class HimSearch
    {
        public static List<HimDirectory> GetFacilities()           
        {
            List<HimDirectory> facilityList = new List<HimDirectory>();

            List<GetHpiListingResult> facilities = new List<GetHpiListingResult>();

            using (PatientsFirstDataContext dc = new PatientsFirstDataContext())
            {
                facilities = dc.GetHpiListing().ToList();
            }

            foreach (GetHpiListingResult fac in facilities)
            {
                HimDirectory listing = new HimDirectory
                {
                    HpiFacilityId = fac.HPI.Trim(),
                    EDI = fac.EDI.Trim(),
                    HimOnLine = fac.HPI_OnLine
                };

                facilityList.Add(listing);
            }

            return facilityList;
        }

    }
}
