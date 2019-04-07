namespace Vintage.AppServices.DataAccessClasses
{
    using Hl7.Fhir.Model;
    using System.Collections.Generic;
    using System.Linq;

    public static class RxNormSearch
    {

        public static List<Coding> GetConceptByCode(string code)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetDescriptionByRxNormCodeResult> concepts = new List<GetDescriptionByRxNormCodeResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                concepts = dc.GetDescriptionByRxNormCode(code).ToList();
            }

            foreach (GetDescriptionByRxNormCodeResult result in concepts)
            {
                codeVals.Add(new Coding { Code = result.RXCUI.Trim(), Display = result.Description});
            }

            return codeVals;
        }

        public static List<Coding> GetConceptsByTerm(string term)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetDescriptionsByRxNormTermResult> concepts = new List<GetDescriptionsByRxNormTermResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                concepts = dc.GetDescriptionsByRxNormTerm(term).ToList();
            }

            foreach (GetDescriptionsByRxNormTermResult result in concepts.OrderBy(xx => xx.Description.Length))
            {
                codeVals.Add(new Coding { Code = result.RXCUI.Trim(), Display = result.Description});
            }

            return codeVals;
        }

        public static List<Coding> GetPropertiesByCode(string code)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetPropertiesByRxNormCodeResult> concepts = new List<GetPropertiesByRxNormCodeResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                concepts = dc.GetPropertiesByRxNormCode(code).ToList();
            }

            foreach (GetPropertiesByRxNormCodeResult result in concepts)
            {
                codeVals.Add(new Coding { Code = result.RXCUI.Trim(), Display = result.Description, Version = result.TTY });
            }

            return codeVals;
        }
    }
}
