namespace Vintage.AppServices.DataAccessClasses
{
    using System.Collections.Generic;
    using System.Linq;
    using Vintage.AppServices.BusinessClasses;

    public static class Closure
    {
        public static bool AddClosureTable(string ClosureName, string CodeSystemID, string CodeSystemVersion)
        {
            bool retVal = true;

            try
            {
                using (SnomedCtDataContext dc = new SnomedCtDataContext())
                {
                    int spReturn = dc.ClientClosure_Insert(ClosureName, CodeSystemID, CodeSystemVersion);
                    retVal = (spReturn >= 0);
                }
            }
            catch
            {
                retVal = false;
            }

            return retVal;
        }

        public static int UpdateClosureTable(string ClosureName, string Concepts, string CodeSystemID, string CodeSystemVersion)
        {
            int retVal = 0;

            try
            {
                using (SnomedCtDataContext dc = new SnomedCtDataContext())
                {
                    int spReturn = dc.ClientClosure_Update(ClosureName, Concepts, CodeSystemID, CodeSystemVersion);
                    retVal = spReturn;
                }
            }
            catch
            {
                retVal = 0;
            }

            // 99 - reinitialise 
            return retVal;
        }

        public static string GetClosureConcepts(string ClosureName, short ClosureVersion, out short dbVer)
        {
            string concepts = string.Empty;
            dbVer = ClosureVersion;

            try
            {
                List<string> codeVals = new List<string>();

                List<GetClientClosuresResult> closures = new List<GetClientClosuresResult>();

                using (SnomedCtDataContext dc = new SnomedCtDataContext())
                {
                    closures = dc.GetClientClosures(ClosureName,ClosureVersion).ToList();
                }
                // will only return a single row
                foreach (GetClientClosuresResult result in closures)
                {
                    concepts = result.Concepts;
                    dbVer = result.Version;
                }
            }
            catch {  }

            return concepts;
        }

    }
}
