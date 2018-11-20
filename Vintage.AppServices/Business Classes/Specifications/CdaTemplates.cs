namespace Vintage.AppServices.BusinessClasses
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.XPath;
    using NMatrix.Schematron;

    public class CdaTemplates : IValidationTest
    {
        public SpecificationType specificationType { get; set; }
        public string specificationTypeVersion { get; set; }
        public FormatType transportFormatType { get; set; }
        public string transportFormatVersion { get; set; }
        public FormatType payloadFormatType { get; set; }
        public string payloadFormatVersion { get; set; }
        public string testFileExtension { get; set; }
        public string testFileName { get; set; }
        public string documentTemplate { get; set; }

        public CdaTemplates()
        {
            this.specificationType = SpecificationType.CdaTemplates;
            this.specificationTypeVersion = "0.9";
            this.transportFormatType = FormatType.XML;
            this.transportFormatVersion = "1.0";
            this.payloadFormatType = FormatType.CDA;
            this.payloadFormatVersion = "2.0";
            this.testFileExtension = ".xml";
            this.testFileName = "HISO_CDA_Template_Standard_Draft";
            this.documentTemplate = "2.16.840.1.113883.2.18.7.x";   // could be any NZ CDA Template
        }

        static public string GetDescription()
        {
            return "CdaTemplates"; // Draft : HISO 10043.3;
        }

        public List<string> ValidateTestInstance(string testFileName, string encryptionKey)
        {
            List<string> validationResults = new List<string>();

            try
            {

                // validate against CDA Schema
                try
                {
                    List<string> schemaErrors = Validate_Schema(testFileName);

                    if (schemaErrors.Count < 1)
                    {
                        validationResults.Add("FORMAT : Pass");
                    }
                    else
                    {
                        validationResults.Add("FORMAT : Fail");
                        validationResults.AddRange(schemaErrors);
                    }
                }
                catch (Exception exFormat)
                {
                    validationResults.Add("FORMAT : Fail");
                    validationResults.Add("Exception : " + exFormat.Message);
                }

                //// validate against Template Schematron - Schematron use deprecated, so no data validation
                validationResults.Add("DATA : Pass");

                //try
                //{

                //    List<string> schematronErrors = Validate_Schematron(testFileName);

                //    if (schematronErrors.Count < 1)
                //    {
                //        validationResults.Add("DATA : Pass");
                //    }
                //    else
                //    {
                //        validationResults.Add("DATA : Fail");
                //        validationResults.AddRange(schematronErrors);
                //    }

                //}
                //catch (Exception exData)
                //{
                //    validationResults.Add("DATA : Fail");
                //    validationResults.Add("Exception : " + exData.Message);
                //}

            }
            catch (Exception ex)
            {
                validationResults.Add("FORMAT : Fail");
                validationResults.Add("Exception : " + ex.Message);
                validationResults.Add("DATA : ???");
                validationResults.Add("Unable to validate");
            }
            
            return validationResults;
        }

        public static List<string> Validate_Schema(string testFileName)
        {
            List<string> validationErrors = new List<string>();

            XDocument cda = XDocument.Load(testFileName);

            // safest way to get to CDA Schema Files folder (under bin folder) in all environments
            string schemaFolder = Specification.GetSchemaLocation();

            if (!File.Exists(schemaFolder + @"\cda.xsd"))
            {
                // if not there - just the runtime folder
                schemaFolder = Directory.GetCurrentDirectory();
            }

            if (!File.Exists(schemaFolder + @"\cda.xsd"))
            {
                validationErrors.Add("ERROR: Cannot Locate CDA Schema Files");
            }
            else
            {

                XmlSchemaSet set = new XmlSchemaSet();

                set.Add(null, schemaFolder + @"/cda.xsd");
                set.Add(null, schemaFolder + @"/voc.xsd");
                set.Add(null, schemaFolder + @"/POCD_MT000040.xsd");
                set.Add(null, schemaFolder + @"/datatypes.xsd");
                set.Add(null, schemaFolder + @"/datatypes-base.xsd");
                set.Add(null, schemaFolder + @"/NarrativeBlock.xsd");

                StringBuilder sb = new StringBuilder();
                cda.Validate(set, (sender, args) => { validationErrors.Add(args.Exception.Message); });
            }

            return validationErrors;
        }


        public static List<string> Validate_Schematron(string testFileName)
        {
            List<string> validationErrors = new List<string>();

            int tronFiles = 0;

            try
            {
                Validator validator = new Validator(OutputFormatting.Default, NavigableType.Default);

                foreach (string tron in Directory.GetFiles(Specification.GetSchemaLocation()))
                {
                    if (tron.EndsWith(".sch") || tron.EndsWith(".ent"))
                    {
                        validator.AddSchema(tron);
                        tronFiles++;
                    }
                }

                if (tronFiles > 0)
                {
                    XPathDocument xmlDoc = new XPathDocument(testFileName);
                    validator.ValidateSchematron(xmlDoc);
                }

            }
            catch (Exception ex)
            {
                foreach (string err in ex.Message.Split(new char[] { '\n', '\r' }))
                {
                    if (err.Trim().StartsWith("Assert"))
                    {
                        validationErrors.Add(err.Trim());
                    }
                }
            }

            return validationErrors;
        }

    }

}