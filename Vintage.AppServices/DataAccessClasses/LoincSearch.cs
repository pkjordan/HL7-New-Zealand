namespace Vintage.AppServices.DataAccessClasses
{
    using System.Collections.Generic;
    using System.Linq;
    using Vintage.AppServices.BusinessClasses;
    using Hl7.Fhir.Model;

    public static class LoincSearch
    {
        public static List<Coding> GetConceptsByProperty(string loincProperty, string loincValue)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetDescriptionByLoincPropertyResult> concepts = new List<GetDescriptionByLoincPropertyResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                concepts = dc.GetDescriptionByLoincProperty(loincProperty, loincValue).ToList();
            }

            foreach (GetDescriptionByLoincPropertyResult result in concepts)
            {
                codeVals.Add(new Coding { Code = result.id.Trim(), Display = result.long_common_name });
            }


            return codeVals;
        }

        public static List<Coding> GetConceptByCode(string code)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetDescriptionByLoincCodeResult> concepts = new List<GetDescriptionByLoincCodeResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                concepts = dc.GetDescriptionByLoincCode(code).ToList();
            }

            foreach (GetDescriptionByLoincCodeResult result in concepts)
            {
                codeVals.Add(new Coding { Code = result.id.Trim(), Display = result.long_common_name });
            }

            return codeVals;
        }

        public static List<Coding> GetPropertiesByCode(string code)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetPropertiesByLoincCodeResult> concepts = new List<GetPropertiesByLoincCodeResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                concepts = dc.GetPropertiesByLoincCode(code).ToList();
            }

            foreach (GetPropertiesByLoincCodeResult result in concepts)
            {
                codeVals.Add(new Coding { Code = "STATUS", Display = result.status.Trim() });
                codeVals.Add(new Coding { Code = "COMPONENT", Display = result.component});
                codeVals.Add(new Coding { Code = "PROPERTY", Display = result.property});
                codeVals.Add(new Coding { Code = "TIME_ASPCT", Display = result.time_aspct});
                codeVals.Add(new Coding { Code = "SYSTEM", Display = result.system});
                codeVals.Add(new Coding { Code = "SCALE_TYP", Display = result.scale_typ});
                codeVals.Add(new Coding { Code = "METHOD_TYP", Display = result.method_typ });
                codeVals.Add(new Coding { Code = "CLASS", Display = result.@class });
                codeVals.Add(new Coding { Code = "CLASSTYPE", Display = result.classtype.ToString() });
                codeVals.Add(new Coding { Code = "ORDER_OBS", Display = result.order_obs });
                codeVals.Add(new Coding { Code = "CONSUMER_NAME", Display = result.consumer_name });
            }

            return codeVals;
        }


        public static List<Coding> GetConceptsByTerm(string term)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetDescriptionsByLoincTermResult> concepts = new List<GetDescriptionsByLoincTermResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                concepts = dc.GetDescriptionsByLoincTerm(term).ToList();
            }

            foreach (GetDescriptionsByLoincTermResult result in concepts.OrderBy(xx => xx.long_common_name.Length))
            {
                codeVals.Add(new Coding { Code = result.id.Trim(), Display = result.long_common_name});
            }

            return codeVals;
        }

    }
}
