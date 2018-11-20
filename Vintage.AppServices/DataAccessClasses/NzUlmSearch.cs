namespace Vintage.AppServices.DataAccessClasses
{
    using System.Collections.Generic;
    using System.Linq;
    using Vintage.AppServices.BusinessClasses;
    using Hl7.Fhir.Model;

    public static class NzUlmSearch
    {

        public static List<Coding> GetNZULMPrescribingTerms(string term, string termType)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetNZULM_PrescribingTermResult> concepts = new List<GetNZULM_PrescribingTermResult>();

            using (NzUlmDataContext dc = new NzUlmDataContext())
            {
                concepts = dc.GetNZULM_PrescribingTerm(term, termType).ToList();
            }

            foreach (GetNZULM_PrescribingTermResult result in concepts)
            {
                codeVals.Add(new Coding { Code = result.concept_id.Trim(), Display = result.prescribing_term, System = result.term_type });
            }

            return codeVals;
        }

        public static List<Coding> GetConceptSubstanceDataByCode(string code, string nzulmType)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetMedicinalProduct_Substance_ByProductCodeResult> concepts = new List<GetMedicinalProduct_Substance_ByProductCodeResult>();

            using (NzUlmDataContext dc = new NzUlmDataContext())
            {
                concepts = dc.GetMedicinalProduct_Substance_ByProductCode(code).ToList();
            }

            foreach (GetMedicinalProduct_Substance_ByProductCodeResult result in concepts)
            {
                codeVals.Add(new Coding { System = "NZULM Type", Display = nzulmType });

                if (nzulmType != "MP")
                {
                    codeVals.Add(new Coding { System = "Medicinal Product", Display = result.PreferredTerm, Code = result.Product_Code });
                }                
                
                codeVals.Add(new Coding { System = "Is Base Substance?", Display = result.BaseSubstance_.ToString() });

                if (!string.IsNullOrEmpty(result.Ingredient))
                {
                    codeVals.Add(new Coding { System = "Ingredient", Display = result.Ingredient});
                }

                if (!string.IsNullOrEmpty(result.LessModifiedIngredient))
                {
                    codeVals.Add(new Coding { System = "Less Modified Ingredient", Display = result.LessModifiedIngredient});
                }

            }

            return codeVals;
        }

        public static List<Coding> GetNzmtCombinedByCode(string code)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetCttp_Related_IDs_ByCodeResult> concepts = new List<GetCttp_Related_IDs_ByCodeResult>();

            using (NzUlmDataContext dc = new NzUlmDataContext())
            {
                concepts = dc.GetCttp_Related_IDs_ByCode(code).ToList();
            }

            foreach (GetCttp_Related_IDs_ByCodeResult result in concepts)
            {
                if (code == result.cttp_id)
                {
                    codeVals.Add(new Coding { Code = result.cttp_id.Trim(), Display = result.cttp_pt, Version = "CTTP"});
                }
                else if (code == result.mp_id)
                {
                    codeVals.Add(new Coding { Code = result.mp_id.Trim(), Display = result.mp_pt, Version = "MP" });
                }
                else if (code == result.mpp_id)
                {
                    codeVals.Add(new Coding { Code = result.mpp_id.Trim(), Display = result.mpp_pt, Version = "MPP" });
                }
                else if (code == result.mpuu_id)
                {
                    codeVals.Add(new Coding { Code = result.mpuu_id.Trim(), Display = result.mpuu_pt, Version = "MPUU" });
                }
                else if (code == result.tp_id)
                {
                    codeVals.Add(new Coding { Code = result.tp_id.Trim(), Display = result.tp_pt, Version = "TP" });
                }
                else if (code == result.tpp_id)
                {
                    codeVals.Add(new Coding { Code = result.tpp_id.Trim(), Display = result.tpp_pt, Version = "TPP" });
                }
                else if (code == result.tpuu_id)
                {
                    codeVals.Add(new Coding { Code = result.tpuu_id.Trim(), Display = result.tpuu_pt, Version = "TPUU",  });
                }

                if (codeVals.Count > 0 && !string.IsNullOrEmpty(result.mpp_id))
                {
                    codeVals[0].ElementId = result.mp_id;
                }

                break;
            }

            return codeVals;
        }

        public static List<Coding> GetMedicinalProductByTerm(string term)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetMedicinalProduct_ByTermResult> concepts = new List<GetMedicinalProduct_ByTermResult>();

            using (NzUlmDataContext dc = new NzUlmDataContext())
            {
                concepts = dc.GetMedicinalProduct_ByTerm(term).ToList();
            }

            foreach (GetMedicinalProduct_ByTermResult result in concepts)
            {
                codeVals.Add(new Coding { Code = result.Product_Code.Trim(), Display = result.Preferred_Term });
            }

            return codeVals;
        }

        public static List<Coding> GetMedicinalProductPackByTerm(string term)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetMedicinalProductPack_ByTermResult> concepts = new List<GetMedicinalProductPack_ByTermResult>();

            using (NzUlmDataContext dc = new NzUlmDataContext())
            {
                concepts = dc.GetMedicinalProductPack_ByTerm(term).ToList();
            }

            foreach (GetMedicinalProductPack_ByTermResult result in concepts)
            {
                codeVals.Add(new Coding { Code = result.Pack_Code.Trim(), Display = result.Preferred_Term });
            }

            return codeVals;
        }

        public static List<Coding> GetMedicinalProductUnitOfUseByTerm(string term)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetMedicinalProductUnitOfUse_ByTermResult> concepts = new List<GetMedicinalProductUnitOfUse_ByTermResult>();

            using (NzUlmDataContext dc = new NzUlmDataContext())
            {
                concepts = dc.GetMedicinalProductUnitOfUse_ByTerm(term).ToList();
            }

            foreach (GetMedicinalProductUnitOfUse_ByTermResult result in concepts)
            {
                codeVals.Add(new Coding { Code = result.Unit_of_Use_Code, Display = result.Preferred_Term });
            }

            return codeVals;
        }

        public static List<Coding> GetTradeProductByTerm(string term)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetTradeProduct_ByTermResult> concepts = new List<GetTradeProduct_ByTermResult>();

            using (NzUlmDataContext dc = new NzUlmDataContext())
            {
                concepts = dc.GetTradeProduct_ByTerm(term).ToList();
            }

            foreach (GetTradeProduct_ByTermResult result in concepts)
            {
                codeVals.Add(new Coding { Code = result.Product_Code.Trim(), Display = result.Preferred_Term });
            }

            return codeVals;
        }

        public static List<Coding> GetTradeProductPackByTerm(string term)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetTradeProductPack_ByTermResult> concepts = new List<GetTradeProductPack_ByTermResult>();

            using (NzUlmDataContext dc = new NzUlmDataContext())
            {
                concepts = dc.GetTradeProductPack_ByTerm(term).ToList();
            }

            foreach (GetTradeProductPack_ByTermResult result in concepts)
            {
                codeVals.Add(new Coding { Code = result.Product_Pack_Code.Trim(), Display = result.Preferred_Term });
            } 
            
            return codeVals;
        }

        public static List<Coding> GetTradeProductUnitOfUseByTerm(string term)
        {
            List<Coding> codeVals = new List<Coding>();
            List<GetTradeProductUnitOfUse_ByTermResult> concepts = new List<GetTradeProductUnitOfUse_ByTermResult>();

            using (NzUlmDataContext dc = new NzUlmDataContext())
            {
                concepts = dc.GetTradeProductUnitOfUse_ByTerm(term).ToList();
            }

            foreach (GetTradeProductUnitOfUse_ByTermResult result in concepts)
            {
                codeVals.Add(new Coding { Code = result.Unit_of_Use_Code.Trim(), Display = result.Preferred_Term });
            }

            return codeVals;
        }

        public static List<Coding> GetContaineredTradeProductPackByTerm(string term)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetContaineredTradeProductPack_ByTermResult> concepts = new List<GetContaineredTradeProductPack_ByTermResult>();

            using (NzUlmDataContext dc = new NzUlmDataContext())
            {
                concepts = dc.GetContaineredTradeProductPack_ByTerm(term).ToList();
            }

            foreach (GetContaineredTradeProductPack_ByTermResult result in concepts)
            {
                codeVals.Add(new Coding { Code = result.Containered_Pack_Code.Trim(), Display = result.Preferred_Term });
            }

            return codeVals;
        }

    }
}
