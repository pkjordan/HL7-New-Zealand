namespace Vintage.AppServices.BusinessClasses
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using NZeHR.CDA;
    using NZeHR.DataTypes;

    /// <summary>
    ///  Specification Format Types
    /// </summary>
    public enum FormatType
    {
        Unknown = 0,
        HL7v2 = 1,
        HL7v3 = 10,
        CDA = 20,
        FlatFile = 30,
        XML = 40,
        XML_NZEPS = 50,
    };

    /// <summary>
    ///  Specification Types
    /// </summary>
    public enum SpecificationType
    {
        Unknown = 0,
        GP2GP = 1,
        ePrescribing = 2,
        eDischargeSummary = 3,
        PharmacyHealthSummary = 4,
        GP2GPV2 = 5,
        InterRaiCommunityHealth = 10,
        InterRaiHomeCare = 11,
        InterRaiLongTermCareFacility = 12,
        InterRaiContact = 13,
        CdaTemplates = 100
    };

    /// <summary>
    /// Class for supplying static Specification-related methods
    /// </summary>
    static internal class Specification
    {

        static internal List<string> GetAvailableSpecificationList()
        {
            List<string> specifications = new List<string>();
            specifications.Add(Gp2Gp.GetDescription());
            specifications.Add(ePrescription.GetDescription());
            specifications.Add(eDischargeSummary.GetDescription());
            specifications.Add(PharmacyHealthSummary.GetDescription());
            specifications.Add(InterRaiCommunityHealth.GetDescription());
            specifications.Add(InterRaiHomeCare.GetDescription());
            specifications.Add(InterRaiLongTermCareFacility.GetDescription());
            specifications.Add(InterRaiContact.GetDescription());
            specifications.Add(Gp2Gpv2.GetDescription());
            specifications.Add(CdaTemplates.GetDescription());
            return specifications;
        }

        static internal SpecificationType GetSpecificationType(string specification)
        {
            SpecificationType specType = SpecificationType.Unknown;

            if (specification == Gp2Gp.GetDescription())
            {
                specType = SpecificationType.GP2GP;
            }
            else if (specification == ePrescription.GetDescription())
            {
                specType = SpecificationType.ePrescribing;
            }
            else if (specification == eDischargeSummary.GetDescription())
            {
                specType = SpecificationType.eDischargeSummary;
            }
            else if (specification == CdaTemplates.GetDescription())
            {
                specType = SpecificationType.CdaTemplates;
            }
            else if (specification == PharmacyHealthSummary.GetDescription())
            {
                specType = SpecificationType.PharmacyHealthSummary;
            }
            else if (specification == InterRaiCommunityHealth.GetDescription())
            {
                specType = SpecificationType.InterRaiCommunityHealth;
            }
            else if (specification == InterRaiHomeCare.GetDescription())
            {
                specType = SpecificationType.InterRaiHomeCare;
            }
            else if (specification == InterRaiLongTermCareFacility.GetDescription())
            {
                specType = SpecificationType.InterRaiLongTermCareFacility;
            }
            else if (specification == InterRaiContact.GetDescription())
            {
                specType = SpecificationType.InterRaiContact;
            }
            else if (specification == Gp2Gpv2.GetDescription())
            {
                specType = SpecificationType.GP2GPV2;
            }

            return specType;
        }

        static internal string GetSpecificationInstance(SpecificationType specType)
        {
            string specInstance = string.Empty;

            // determine name of Test Instance File & check that it exists
            IValidationTest testInstance = ValidationTestFactory.Create(specType);

            // safest way to get to Test Files folder (under bin folder) in all environments
            string fileFullName = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) + @"\Test Files\" + testInstance.testFileName + testInstance.testFileExtension;

            if (!File.Exists(fileFullName))
            {
                throw new Exception("Test Instance (" + testInstance.testFileName + ") Not Found");
            }
            
            // read the file into a string
            using (StreamReader sr = File.OpenText(fileFullName))
            {
                specInstance = sr.ReadToEnd();
            }

            return specInstance;
        }

        static internal List<string> Validate(SpecificationType specType, string testData)
        {
            List<string> validationResults = new List<string>();

            string encryptionKey = "GP2GPzENCRYPTION";
            string fileFullName = string.Empty;
            
            // create Test Instance
            IValidationTest testInstance = ValidationTestFactory.Create(specType);

            try
            {

                // validate transport format
                string fileHeader = testData.Substring(0, 10).ToUpper();
                bool validTransport = true;

                if (testInstance.transportFormatType == FormatType.XML || testInstance.transportFormatType == FormatType.XML_NZEPS)
                {
                    validTransport = fileHeader.StartsWith(@"<?XML ");
                }
                else if (testInstance.transportFormatType == FormatType.HL7v2)
                {
                    validTransport = fileHeader.StartsWith(@"MSH|^~\&|");
                }

                if (!validTransport)
                {
                    validationResults.Add("TRANSPORT : Fail");
                    validationResults.Add("Invalid Message File Format : specification requires " + testInstance.transportFormatType.ToString());
                    validationResults.Add("FORMAT : ???");
                    validationResults.Add("Unable to validate");
                    validationResults.Add("DATA : ???");
                    validationResults.Add("Unable to validate");
                }
                else
                {
                    validationResults.Add("TRANSPORT : Pass");
                    // create temporary test file (this is supported from Windows Azure Role)
                    fileFullName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + testInstance.testFileExtension;

                    // write the test data string to the temporary test file (required for Toolkit message consumption)
                    using (StreamWriter sw = new StreamWriter(fileFullName))
                    {
                        sw.Write(testData);
                    }

                    // call test instance validate method for Format and Data checks
                    validationResults.AddRange(testInstance.ValidateTestInstance(fileFullName, encryptionKey));

                }
            }
            finally
            {
                // delete temporary test file
                if (File.Exists(fileFullName))
                {
                    File.Delete(fileFullName);
                }
            }

            return validationResults;
        }

        static internal List<string> ValidateNZeHR(CdaDocument testInstance, string docTemplate)
        {
            List<string> validationResults = new List<string>();
            List<string> formatValidationResults = new List<string>();
            List<string> headerValidationResults = new List<string>();
            List<string> dataValidationResults = new List<string>();
            List<Code> clinicalCodeList = new List<Code>();

            string templateID = string.Empty;
            int dataErrors = 0;

            // validate CDA - add any errors to list
            try
            {
                foreach (CdaTemplate tmpl in testInstance.templates)
                {
                    templateID = tmpl.id;
                    if (templateID == docTemplate)
                    {
                        break;
                    }
                }

                if (templateID != docTemplate)
                {
                    formatValidationResults.Add("Invalid Document Template ID " + templateID + " : expected " + docTemplate);
                }
                else
                {
                    testInstance.ValidateAll(Specification.GetSchemaLocation());

                    // Schema Validation Results
                    formatValidationResults.AddRange(testInstance.schemaErrors);

                    // Header Validation Results
                    foreach (string de in testInstance.allErrors)
                    {
                        if (de.ToUpper().StartsWith("HEADER:") || de.ToUpper().StartsWith("PATIENT:"))
                        {
                            headerValidationResults.Add(de);
                        }
                    }

                    dataValidationResults.Add("Header: " + (headerValidationResults.Count < 1 ? "Pass" : "Fail"));
                    dataValidationResults.AddRange(headerValidationResults);
                    dataErrors += headerValidationResults.Count;

                    int populatedSections = 0;

                    // Section Validation Results
                    foreach (CdaSection section in testInstance.sections)
                    {
                        if (section.items.Count > 0)
                        {
                            dataValidationResults.Add(section.displayName + " Section: " + (section.allErrors.Count < 1 ? "Pass" : "Fail"));
                            dataValidationResults.AddRange(section.allErrors);
                            dataErrors += section.allErrors.Count;
                            populatedSections++;
                            clinicalCodeList.AddRange(GetClinicalCodes(section));
                        }
                    }

                    if (populatedSections == 0)
                    {
                        dataErrors++;
                        dataValidationResults.Add("No Section Entries could be imported.");
                    }

                }

            }
            catch (Exception exData)
            {
                dataErrors++;
                dataValidationResults.Add("Exception : " + exData.Message);
            }
            
            validationResults.Add("FORMAT : " + (formatValidationResults.Count < 1 ? "Pass" : "Fail"));
            validationResults.AddRange(formatValidationResults);

            validationResults.Add("DATA : " + (dataErrors < 1 ? "Pass" : "Fail"));
            validationResults.AddRange(dataValidationResults);

            validationResults.Add("CODING : (from clinical data values)");
            validationResults.AddRange(AnalyseClinicalCodes(clinicalCodeList));

            return validationResults;
        }

        static internal List<string> AnalyseClinicalCodes(List<Code> clinicalCodeList)
        {
            List<string> codeUsage = new List<string>();

            int codeTot = 0;
            int codeNull = 0;
            int codeCode = 0;
            int codeText = 0;

            int snomed = 0;
            int read = 0;
            int icd9 = 0;
            int icd10 = 0;
            int loinc = 0;
            int nzulm = 0;
            int mims = 0;
            int pharmac = 0;
            int encount = 0;
            int route = 0;
            int interaiAss = 0;
            int interaiOutScale = 0;
            int other = 0;


            foreach(Code cc in clinicalCodeList)
            {
                codeTot++;

                if (!string.IsNullOrEmpty(cc.nullFlavor))
                {
                    codeNull++;
                }
                else if (!string.IsNullOrEmpty(cc.code))
                {
                    codeCode++;

                    if (cc.codeSystem == CodeSystem.SNOMED_CT)
                    {
                        snomed++;
                    }
                    else if (cc.codeSystem == CodeSystem.NZ_READ)
                    {
                        read++;
                    }
                    else if (cc.codeSystem == CodeSystem.LOINC)
                    {
                        loinc++;
                    }
                    else if (cc.codeSystem == CodeSystem.NZ_MEDICINE)
                    {
                        nzulm++;
                    }
                    else if (cc.codeSystem == CodeSystem.MIMS)
                    {
                        mims++;
                    }
                    else if (cc.codeSystem == CodeSystem.PHARMAC)
                    {
                        pharmac++;
                    }
                    else if (cc.codeSystem == CodeSystem.ICD_9_CM || cc.codeSystem == CodeSystem.ICD_9)
                    {
                        icd9++;
                    }
                    else if (cc.codeSystem == CodeSystem.ICD_10_CM || cc.codeSystem == CodeSystem.ICD_10)
                    {
                        icd10++;
                    }
                    else if (cc.codeSystem == CodeSystem.MEDICATION_ADMINISTRATION_ROUTE)
                    {
                        route++;
                    }
                    else if (cc.codeSystem == CodeSystem.NZ_ENCOUNTER_TYPE)
                    {
                        encount++;
                    }
                    else if (cc.codeSystem == CodeSystem.INTERRAI_ASSESSMENT_TYPE)
                    {
                        interaiAss++;
                    }
                    else if (cc.codeSystem == CodeSystem.INTERRAI_OUTCOME_SCALE)
                    {
                        interaiOutScale++;
                    }
                    else
                    {
                        other++;
                    }
                }
                else if (!string.IsNullOrEmpty(cc.originalText))
                {
                    codeText++;
                }

            }

            if (codeTot > 0)
            {
                int nullPercent = (codeNull == 0) ? 0 : Convert.ToInt32(codeNull * 100.0 / codeTot);
                int textPercent = (codeText == 0) ? 0 : Convert.ToInt32(codeText * 100.0 / codeTot);
                int codePercent = (codeCode == 0) ? 0 : Convert.ToInt32(codeCode * 100.0 / codeTot);

                codeUsage.Add("Coded Properties = " + codeTot.ToString());
                codeUsage.Add("...Null values = " + codeNull.ToString() + " (" + nullPercent.ToString() + "%)");
                codeUsage.Add("...Text only values = " + codeText.ToString() + " (" + textPercent.ToString() + "%)");
                codeUsage.Add("...Coded values = " + codeCode.ToString() + " (" + codePercent.ToString() + "%)" );

                if (snomed > 0) { codeUsage.Add("......SNOMED = " + snomed.ToString()); }
                if (read > 0) { codeUsage.Add("......READ = " + read.ToString()); }
                if (icd9 > 0) { codeUsage.Add("......ICD9 = " + icd9.ToString()); }
                if (icd10 > 0) { codeUsage.Add("......ICD10 = " + icd10.ToString()); }
                if (loinc > 0) { codeUsage.Add("......LOINC = " + loinc.ToString()); }
                if (nzulm > 0) { codeUsage.Add("......NZ ULM = " + nzulm.ToString()); }
                if (mims > 0) { codeUsage.Add("......MIMS = " + mims.ToString()); }
                if (pharmac > 0) { codeUsage.Add("......PHARMAC = " + pharmac.ToString()); }
                if (route > 0) { codeUsage.Add("......MEDICATION ROUTE = " + route.ToString()); }
                if (encount > 0) { codeUsage.Add("......NZ ENCOUNTER TYPE = " + encount.ToString()); }
                if (interaiAss > 0) { codeUsage.Add("......INTERRAI ASSESSMENT = " + interaiAss.ToString()); }
                if (interaiOutScale > 0) { codeUsage.Add("......INTERRAI OUTCOME = " + interaiOutScale.ToString()); }
                if (other > 0) { codeUsage.Add("......other = " + other.ToString()); }

            }

            return codeUsage;
        }

        static internal List<Code> GetClinicalCodes(CdaSection section)
        {
            List<Code> codeList = new List<Code>();

            foreach (CdaEntry entry in section.items)
            {
                foreach (Code code in entry.clinicalCodes)
                {
                    codeList.Add(code);
                }
            }

            return codeList;
        }


        static internal string GetSchemaLocation()
        {
            return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) + @"\Schema";
        }

    }

}
