namespace Vintage.AppServices.DataAccessClasses
{
    using Hl7.Fhir.Model;
    using System.Collections.Generic;
    using System.Linq;
    using Vintage.AppServices.BusinessClasses;

    public static class SnomedCtSearch
    {

        public static List<Coding> GetAttributes(string concept)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetAllAttributesByCodeResult> attributes = new List<GetAllAttributesByCodeResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                attributes = dc.GetAllAttributesByCode(concept).OrderBy(att => att.relationshipGroup).ToList();
            }

            foreach (GetAllAttributesByCodeResult result in attributes)
            {
                codeVals.Add(new Coding { Code = result.AttributeTypeID.Trim(), Display = result.AttributeValueID.Trim(), System = result.DefinitionStatus });
            }

            return codeVals;
        }

        public static List<Coding> GetAttributeExpressions(string concept, bool fullDisplay)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetAllAttributesByCodeResult> attributes = new List<GetAllAttributesByCodeResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                attributes = dc.GetAllAttributesByCode(concept).OrderBy(att => att.relationshipGroup).ThenByDescending(att => att.AttributeValueDefinitionStatus).ToList();
            }

            foreach (GetAllAttributesByCodeResult result in attributes)
            {
                
                // indicate to expression builder that an attribute values is fully defined and must be expanded
                string eq = "=";

                if (result.AttributeValueDefinitionStatus == "Fully Defined")
                {
                    eq = "=*";
                }

                string expression = result.AttributeTypeID.Trim() + ((fullDisplay) ? "|" + result.AttributeTypeTerm + "|" : "")
                    + eq + result.AttributeValueID.Trim() + ((fullDisplay) ? "|" + result.AttributeValueTerm + "|" : "");

                codeVals.Add(new Coding {  Code = result.relationshipGroup.Trim(), Display = expression});
            }

            return codeVals;
        }

        public static List<Coding> GetProximalPrimitives(string concept, bool fullDisplay)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetProximalPrimitivesResult> proxPrim = new List<GetProximalPrimitivesResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                proxPrim = dc.GetProximalPrimitives(concept).ToList();
            }

            foreach (GetProximalPrimitivesResult result in proxPrim)
            {
                string expression = result.conceptId.Trim() + ( (fullDisplay) ? "|" + result.term + "|" : "");
                codeVals.Add(new Coding { Code = result.conceptId.Trim(), Display = expression});
            }

            return codeVals;
        }

        public static List<Coding> GetCompositionMatch(string[] attType, string[] attValue, string[] pp, int attCount)           
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetCompositionMatchResult> compMatch = new List<GetCompositionMatchResult>();

            // nb cannot use table-values parameters in LINQ to SQL...unfortunately

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                compMatch = dc.GetCompositionMatch(
                  attType[0], attValue[0], attType[1], attValue[1], attType[2],  attValue[2], attType[3], attValue[3], 
                  attType[4],  attValue[4], attType[5],  attValue[5], attType[6], attValue[6], attType[7], attValue[7], 
                  attType[8],  attValue[8], attType[9], attValue[9], attType[10], attValue[10],
                  attType[11], attValue[11], attType[12], attValue[12], attType[13], attValue[13], attType[14], attValue[14],
                  attType[15], attValue[15], attType[16], attValue[16], attType[17], attValue[17],
                  attType[18], attValue[18], attType[19], attValue[19], attType[20], attValue[20],
                  pp[0], pp[1], pp[2], pp[3], pp[4], attCount).ToList();
            }

            foreach (GetCompositionMatchResult match in compMatch)
            {
              codeVals.Add(new Coding { Code = match.ConceptID.Trim(), Display = match.PreferredTerm });
            }

            return codeVals;
        }

        public static List<Coding> GetChildCodes(string concept)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetChildConceptsResult> childConcepts = new List<GetChildConceptsResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                childConcepts = dc.GetChildConcepts(concept).ToList();
            }

            foreach (GetChildConceptsResult result in childConcepts)
            {
                codeVals.Add(new Coding { Code = result.ConceptID.Trim(), Display = result.Term});
            }

            return codeVals;
        }

        public static List<Coding> GetParentCodes(string concept)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetParentConceptsResult> parentConcepts = new List<GetParentConceptsResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                parentConcepts = dc.GetParentConcepts(concept).ToList();
            }

            foreach (GetParentConceptsResult result in parentConcepts)
            {
                codeVals.Add(new Coding { Code = result.ConceptID.Trim(), Display = result.Term });
            }

            return codeVals;
        }

        public static List<Coding> GetSubsumedCodes(string superTypeConcept, bool plusSelf)
        {
            List<Coding> codeVals = new List<Coding>();

            if (plusSelf)
            {
                codeVals = SnomedCtSearch.GetConceptByCode(superTypeConcept);
            }

            List<GetAllSubTypeConceptsResult> subTypeConcepts = new List<GetAllSubTypeConceptsResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                subTypeConcepts = dc.GetAllSubTypeConcepts(superTypeConcept).ToList();
            }

            foreach (GetAllSubTypeConceptsResult result in subTypeConcepts)
            {
                codeVals.Add(new Coding { Code = result.subType.Trim(), Display = result.Term });
            }

            return codeVals;
        }

        public static bool? IsSubsumedBy(string subTypeConcept, string superTypeConcept)
        {
            bool? subsumedBy = false;

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                 subsumedBy = dc.IsSubsumedBy(subTypeConcept,superTypeConcept);
            }

            return subsumedBy;
        }

        public static List<Coding> GetSuperTypes(string subTypeConcept)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetAllSuperTypeConceptsResult> superTypeConcepts = new List<GetAllSuperTypeConceptsResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                superTypeConcepts = dc.GetAllSuperTypeConcepts(subTypeConcept).ToList();
            }

            foreach (GetAllSuperTypeConceptsResult result in superTypeConcepts)
            {
                codeVals.Add(new Coding { Code = result.superType.Trim(), Display = result.Term });
            }


            return codeVals;
        }

        public static List<Coding> GetConceptByCode(string code)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetConceptByCodeResult> concepts = new List<GetConceptByCodeResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                concepts = dc.GetConceptByCode(code).ToList();
            }

            foreach (GetConceptByCodeResult result in concepts)
            {
                codeVals.Add(new Coding { Code = result.Concept_ID.Trim(), Display = result.Term });
            }

            return codeVals;
        }

        public static List<Coding> GetConceptPropertiesByCode(string code)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetConceptPropertiesByCodeResult> props = new List<GetConceptPropertiesByCodeResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                props = dc.GetConceptPropertiesByCode(code).ToList();
            }

            foreach (GetConceptPropertiesByCodeResult result in props)
            {
                codeVals.Add(new Coding { Code = "inactive", Display = (!result.active).ToString() });
                codeVals.Add(new Coding { Code = "sufficientlyDefined", Display = (result.Primitive==0).ToString()});
                codeVals.Add(new Coding { Code = "moduleId", Display = result.moduleId.Trim() });
            }

            return codeVals;
        }

        public static List<Coding> GetConceptsByAttributeNameValuePair(string attributeTypeID, bool reverseAttribute, string attributeValueID, bool attributeTypeAncestors, bool attributeTypeDescendants, bool attributeValueAncestors, bool attributeValueDescendants)
        {
            List<Coding> codeVals = new List<Coding>();

            if (reverseAttribute)
            {
                codeVals = GetConceptsByAttributeNameValuePairReverse(attributeTypeID, attributeValueID, attributeTypeAncestors, attributeTypeDescendants, attributeValueAncestors, attributeValueDescendants);
            }
            else
            {
                List<GetConceptsByAttributeValuePairResult> concepts = new List<GetConceptsByAttributeValuePairResult>();

                using (SnomedCtDataContext dc = new SnomedCtDataContext())
                {
                    concepts = dc.GetConceptsByAttributeValuePair(attributeTypeID, attributeValueID, attributeTypeAncestors, attributeTypeDescendants, attributeValueAncestors, attributeValueDescendants).ToList();
                }

                foreach (GetConceptsByAttributeValuePairResult result in concepts)
                {
                    codeVals.Add(new Coding { Code = result.Concept_ID.Trim(), Display = result.Term });
                }
            }
            return codeVals;
        }

        public static List<Coding> GetConceptsByAttributeNameValuePairReverse(string attributeTypeID, string attributeValueID, bool attributeTypeAncestors, bool attributeTypeDescendants, bool attributeValueAncestors, bool attributeValueDescendants)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetConceptsByAttributeValuePairReverseResult> concepts = new List<GetConceptsByAttributeValuePairReverseResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                concepts = dc.GetConceptsByAttributeValuePairReverse(attributeTypeID, attributeValueID, attributeTypeAncestors, attributeTypeDescendants, attributeValueAncestors, attributeValueDescendants).ToList();
            }

            foreach (GetConceptsByAttributeValuePairReverseResult result in concepts)
            {
                codeVals.Add(new Coding { Code = result.Concept_ID.Trim(), Display = result.Term });
            }

            return codeVals;
        }

        public static List<Coding> GetAttributesByFocusTypeValue(string focusID, bool focusDescendants,string attributeTypeID, bool attributeTypeDescendants, string attributeValueID) 
        {
            List<Coding> codeVals = new List<Coding>();

                List<GetAttributesByFocusTypeValueResult> concepts = new List<GetAttributesByFocusTypeValueResult>();

                using (SnomedCtDataContext dc = new SnomedCtDataContext())
                {
                    concepts = dc.GetAttributesByFocusTypeValue(focusID, focusDescendants, attributeTypeID, attributeTypeDescendants, attributeValueID).ToList();
                }

                foreach (GetAttributesByFocusTypeValueResult result in concepts)
                {
                    codeVals.Add(new Coding { Code = result.AssocAttributeValueID.Trim(), Display = result.AssocAttributeValueTerm });
                }
            
            return codeVals;
        }

        public static List<Coding> GetConceptDesignationsByCode(string code)
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetConceptDesignationsByCodeResult> concepts = new List<GetConceptDesignationsByCodeResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                concepts = dc.GetConceptDesignationsByCode(code).ToList();
            }

            foreach (GetConceptDesignationsByCodeResult result in concepts)
            {
                codeVals.Add(new Coding { Code = result.Concept_ID.Trim(), Display = result.Term, System = result.Designation, Version = result.CSI.Trim() });
            }

            return codeVals;
        }

        public static List<Coding> GetRefSet(string refSetCode, string filter)
        {
            string filt = filter.Trim().ToLower();

            List<Coding> codeVals = new List<Coding>();

            List<GetRefSet_NZResult> concepts = new List<GetRefSet_NZResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                concepts = dc.GetRefSet_NZ(refSetCode).ToList();
            }

            foreach (GetRefSet_NZResult result in concepts.OrderBy(xx => xx.term))
            {
                if (string.IsNullOrEmpty(filter) || result.term.Trim().ToLower().Contains(filt))
                {
                    codeVals.Add(new Coding { Code = result.conceptId.Trim(), Display = result.term });
                }

            }

            return codeVals;
        }

        public static List<Coding> GetNzRefSets()
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetNzReferenceSetsResult> concepts = new List<GetNzReferenceSetsResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                concepts = dc.GetNzReferenceSets().ToList();
            }

            foreach (GetNzReferenceSetsResult result in concepts.OrderBy(xx => xx.Term))
            {
               codeVals.Add(new Coding { Code = result.ConceptID.Trim(), Display = result.Term });
            }

            return codeVals;
        }

        public static List<Coding> GetNzEnPatientFriendlyTerms()
        {
            List<Coding> codeVals = new List<Coding>();

            List<GetNzEnPatientFriendlyTermsResult> concepts = new List<GetNzEnPatientFriendlyTermsResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                concepts = dc.GetNzEnPatientFriendlyTerms().ToList();
            }

            foreach (GetNzEnPatientFriendlyTermsResult result in concepts.OrderBy(xx => xx.term))
            {
                codeVals.Add(new Coding { Code = result.conceptId.Trim(), Display = result.term });
            }

            return codeVals;
        }

        public static List<Coding> GetConceptsByTerm(string term)
        {
            string formattedTerm = term.Replace(" ", "*' AND '");
            formattedTerm = "'" + formattedTerm + "*'";
            formattedTerm = formattedTerm.Replace('\'', '\"');

            List<Coding> codeVals = new List<Coding>();

            List<GetDescriptionsByTermResult> concepts = new List<GetDescriptionsByTermResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                concepts = dc.GetDescriptionsByTerm(formattedTerm).ToList();
            }

            foreach (GetDescriptionsByTermResult result in concepts.OrderBy(xx => xx.Rank))
            {
                string cid = result.ConceptID.Trim();
                if (!codeVals.Exists(x => x.Code == cid))
                {
                    codeVals.Add(new Coding { Code = cid, Display = result.Term });
                }
            }

            return codeVals;
        }

        public static List<Coding> GetConceptsFromHierarchyByTerm(string superTypeCode, string term)
        {
            string formattedTerm = term.Replace(" ", "*' AND '");
            formattedTerm = "'" + formattedTerm + "*'";
            formattedTerm = formattedTerm.Replace('\'', '\"');

            List<Coding> codeVals = new List<Coding>();

            List<GetDescriptionsFromHierarchyByTermResult> concepts = new List<GetDescriptionsFromHierarchyByTermResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                concepts = dc.GetDescriptionsFromHierarchyByTerm(superTypeCode,formattedTerm).ToList();
            }

            foreach (GetDescriptionsFromHierarchyByTermResult result in concepts.OrderBy(xx => xx.Rank))
            {
                string cid = result.ConceptID.Trim();
                if (!codeVals.Exists(x => x.Code == cid))
                {
                    codeVals.Add(new Coding { Code = cid, Display = result.Term });
                }
            }

            return codeVals;
        }

        public static List<Coding> GetConceptMap_NZ(string refsetId, string conceptId)
        {

            List<Coding> codeVals = new List<Coding>();

            List<GetConceptMap_NZResult> concepts = new List<GetConceptMap_NZResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                concepts = dc.GetConceptMap_NZ(refsetId, conceptId).ToList();
            }

            foreach (GetConceptMap_NZResult result in concepts)
            {
                codeVals.Add(new Coding { Version = result.SourceCode.Trim(), System = result.SourceTerm.Trim(), Code = result.TargetCode.Trim(), Display = result.TargetTerm.Trim() });
            }

            return codeVals;
        }

        public static List<HpiFacility> GetLocations(string locationIdentifier, string locationName, string locationAddress, string locationType)
        {
            List<HpiFacility> locations = new List<HpiFacility>();
            List<GetHealthcareFacilitiesResult> facilities = new List<GetHealthcareFacilitiesResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                facilities = dc.GetHealthcareFacilities(locationIdentifier, locationName, locationAddress, locationType).ToList();
            }

            foreach (GetHealthcareFacilitiesResult fac in facilities)
            {
                locations.Add(new HpiFacility
                {
                    FacilityId = fac.FacilityHPI,
                    FacilityName = fac.FacilityName,
                    FacilityAddress = fac.FacilityAddress,
                    FacilityTypeCode = fac.FacilityType,
                    FacilityTypeName = fac.FacilityType_Name,
                    OrganisationId = fac.OrganisationHPI.Trim(),
                });
            }

            return locations;
        }

        public static List<HpiOrganisation> GetOrganisations(string organisationIdentifier, string organisationName, string organisationAddress, string organisationType)
        {
            List<HpiOrganisation> organisations = new List<HpiOrganisation>();
            List<GetHealthcareOrganisationsResult> orgs = new List<GetHealthcareOrganisationsResult>();

            using (SnomedCtDataContext dc = new SnomedCtDataContext())
            {
                orgs = dc.GetHealthcareOrganisations(organisationIdentifier, organisationName, organisationAddress, organisationType).ToList();
            }

            foreach (GetHealthcareOrganisationsResult org in orgs)
            {
                organisations.Add(new HpiOrganisation
                {
                    OrganisationId = org.OrganisationHPI,
                    OrganisationName = org.OrganisationName,
                    OrganisationAddress = org.OrganisationAddress,
                    OrganisationTypeCode = org.OrganisationType,
                    OrganisationTypeName = org.OrganisationType
                });
            }

            return organisations;
        }

    }
}
