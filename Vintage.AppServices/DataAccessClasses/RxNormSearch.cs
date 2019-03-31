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

        //public static List<Coding> GetConceptsByProperty(string loincProperty, string loincValue)
        //{
        //    List<Coding> codeVals = new List<Coding>();

        //    List<GetDescriptionByLoincPropertyResult> concepts = new List<GetDescriptionByLoincPropertyResult>();

        //    using (SnomedCtDataContext dc = new SnomedCtDataContext())
        //    {
        //        concepts = dc.GetDescriptionByLoincProperty(loincProperty, loincValue).ToList();
        //    }

        //    foreach (GetDescriptionByLoincPropertyResult result in concepts)
        //    {
        //        codeVals.Add(new Coding { Code = result.id.Trim(), Display = result.long_common_name, Version = result.consumer_name });
        //    }


        //    return codeVals;
        //}

        //public static List<Coding> GetConceptByPartCode(string partCode)
        //{
        //    List<Coding> codeVals = new List<Coding>();

        //    List<GetDescriptionByLoincPartCodeResult> concepts = new List<GetDescriptionByLoincPartCodeResult>();

        //    using (SnomedCtDataContext dc = new SnomedCtDataContext())
        //    {
        //        concepts = dc.GetDescriptionByLoincPartCode(partCode).ToList();
        //    }

        //    foreach (GetDescriptionByLoincPartCodeResult result in concepts)
        //    {
        //        codeVals.Add(new Coding { Code = result.PartNumber.Trim(), Display = result.PartDisplayName, Version = result.PartTypeName });
        //    }

        //    return codeVals;
        //}

        //public static List<Coding> GetPropertiesByCode(string code, string properties)
        //{
        //    List<Coding> codeVals = new List<Coding>();

        //    List<GetPropertiesByLoincCodeResult> concepts = new List<GetPropertiesByLoincCodeResult>();

        //    using (SnomedCtDataContext dc = new SnomedCtDataContext())
        //    {
        //        concepts = dc.GetPropertiesByLoincCode(code).ToList();
        //    }

        //    foreach (GetPropertiesByLoincCodeResult result in concepts)
        //    {
        //        if (properties == "ALL" || properties.Contains("STATUS"))
        //            codeVals.Add(new Coding { Code = "STATUS", Display = result.status.Trim() });

        //        if (properties == "ALL" || properties.Contains("COMPONENT"))
        //            codeVals.Add(new Coding { Code = "COMPONENT", Display = result.component });

        //        if (properties == "ALL" || properties.Contains("PROPERTY"))
        //            codeVals.Add(new Coding { Code = "PROPERTY", Display = result.property });

        //        if (properties == "ALL" || properties.Contains("TIME_ASPCT"))
        //            codeVals.Add(new Coding { Code = "TIME_ASPCT", Display = result.time_aspct });

        //        if (properties == "ALL" || properties.Contains("SYSTEM"))
        //            codeVals.Add(new Coding { Code = "SYSTEM", Display = result.system });

        //        if (properties == "ALL" || properties.Contains("SCALE_TYP"))
        //            codeVals.Add(new Coding { Code = "SCALE_TYP", Display = result.scale_typ });

        //        if (properties == "ALL" || properties.Contains("METHOD_TYP"))
        //            codeVals.Add(new Coding { Code = "METHOD_TYP", Display = result.method_typ });

        //        if (properties == "ALL" || properties.Contains("CLASS"))
        //            codeVals.Add(new Coding { Code = "CLASS", Display = result.@class });

        //        if (properties == "ALL" || properties.Contains("CLASSTYPE"))
        //            codeVals.Add(new Coding { Code = "CLASSTYPE", Display = result.classtype.ToString() });

        //        if (properties == "ALL" || properties.Contains("ORDER_OBS"))
        //            codeVals.Add(new Coding { Code = "ORDER_OBS", Display = result.order_obs });

        //        if (properties == "ALL" || properties.Contains("CONSUMER_NAME"))
        //            codeVals.Add(new Coding { Code = "CONSUMER_NAME", Display = result.consumer_name });
        //    }

        //    return codeVals;
        //}

        

    }
}
