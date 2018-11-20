namespace Vintage.AppServices.DataAccessClasses
{
    using System.Collections.Generic;
    using System.Linq;
    using Vintage.AppServices.BusinessClasses;

    public static class PracticeSearch
    {
        public static List<GeneralPractice> GetPractices(string vendorCode, string practiceName, string practiceAddress, string phoName, string dhbName, string ediAddress)
        {
            List<GeneralPractice> practiceList = new List<GeneralPractice>();

            // format SQL UDF parameters correctly...
            string pracName = string.IsNullOrEmpty(practiceName) ? "ALL" : practiceName;
            string pracAddress = string.IsNullOrEmpty(practiceAddress) ? "ALL" : practiceAddress;
            string pho = string.IsNullOrEmpty(phoName) ? "ALL" : phoName;
            string dhb = string.IsNullOrEmpty(dhbName) ? "ALL" : dhbName;
            string edi = string.IsNullOrEmpty(ediAddress) ? "ALL" : ediAddress;
            string vendor = "ALL";
            bool active = true;
         
            List<GetPractices_ByNamePhoResult> practices = new List<GetPractices_ByNamePhoResult>();

            using (PatientsFirstDataContext dc = new PatientsFirstDataContext())
            {
                practices = dc.GetPractices_ByNamePho(pracName,pracAddress, pho, dhb, vendor, edi, active).ToList();
            }

            foreach(GetPractices_ByNamePhoResult gp in practices)
            {
                GeneralPractice gpItem = new GeneralPractice{ PracticeName = gp.PracticeName, 
                                                              PracticeAddress = gp.PracticeAddress,
                                                              HpiFacilityId = gp.PracticeId,
                                                              EDI = gp.EDI.Trim(),
                                                              PHO = gp.PhoName,
                                                              DHB = gp.DHB,
                                                              Region = gp.Region,
                                                              Rural = (gp.Rural ? "Yes" : "No")};
                practiceList.Add(gpItem);
            }

            return practiceList;
        }
    }
}
