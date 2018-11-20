namespace Vintage.AppServices.DataAccessClasses
{
    public static class HimOnLine
    {
        public static void UpdateOnLineStatus(string HpiFacilityID, bool onLine)
        {
            using (PatientsFirstDataContext dc = new PatientsFirstDataContext())
            {
                dc.HpiOnLine_UpdateStatus(HpiFacilityID,onLine);
            }
        }

        public static void UpdateClientVersion(string HpiFacilityID, string himVersion)
        {
            using (PatientsFirstDataContext dc = new PatientsFirstDataContext())
            {
                dc.HpiOnLine_UpdateVersion(HpiFacilityID, himVersion);
            }
        }

    }
}