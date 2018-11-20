namespace Vintage.AppServices.BusinessClasses.FHIR.CodeSystems
{
    using System;
    using System.Collections.Generic;
    using Hl7.Fhir.Model;

    /// <summary>
    ///  NZ Ethnicity Codes - Level 4
    /// </summary>

    public class NzEthnicityL4
    {
        public const string URI = "https://standards.digital.health.nz/codesystem/ethnic-group-level-4";

        public CodeSystem codeSystem { get; set; }
        public ValueSet valueSet { get; set; }

        public NzEthnicityL4()
        {
            this.FillValues(TerminologyOperation.define_vs, string.Empty, string.Empty, string.Empty, -1, -1);
        }

        public NzEthnicityL4(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {
            this.FillValues(termOp, version, code, filter, offsetNo, countNo);
        }

        private void FillValues(TerminologyOperation termOp, string version, string code, string filter, int offsetNo, int countNo)
        {

            this.valueSet = new ValueSet();
            this.codeSystem = new CodeSystem();

            this.valueSet.Id = "NzEthnicityL4";
            this.codeSystem.Id = "NzEthnicityL4";

            this.codeSystem.CaseSensitive = true;
            this.codeSystem.Content = CodeSystem.CodeSystemContentMode.Complete;
            this.codeSystem.Experimental = true;
            this.codeSystem.Compositional = false;
            this.codeSystem.VersionNeeded = false;

            this.valueSet.Url = ServerCapability.TERMINZ_CANONICAL + "/ValueSet/NzEthnicityL4";
            this.codeSystem.Url = NzEthnicityL4.URI;

            this.codeSystem.ValueSet = this.valueSet.Url;

            this.valueSet.Name = "NZ Ethnicity Level 4";
            this.codeSystem.Name = "NZ Ethnicity Level 4";

            this.valueSet.Description = new Markdown("Value Set of all NZ Ethnicity Level 4 Codes");
            this.codeSystem.Description = new Markdown("NZ Ethnicity Level 4 Codes");

            this.valueSet.Version = "1.0.1";
            this.codeSystem.Version = "1.0.1";

            this.valueSet.Experimental = false;

            this.valueSet.Status = PublicationStatus.Active;
            this.codeSystem.Status = PublicationStatus.Active;

            this.valueSet.Date = Hl7.Fhir.Model.Date.Today().Value;
            this.codeSystem.Date = Hl7.Fhir.Model.Date.Today().Value;

            this.valueSet.Publisher = "Patients First Ltd";
            this.codeSystem.Publisher = "Ministry of Health";

            this.valueSet.Copyright = new Markdown("© 2010+ New Zealand Crown Copyright");
            this.codeSystem.Copyright = new Markdown("© 2010+ New Zealand Crown Copyright");

            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "peter.jordan@patientsfirst.org.nz" };
            ContactDetail cd = new ContactDetail();
            cd.Telecom.Add(cp);
            this.valueSet.Contact.Add(cd);
            this.codeSystem.Contact.Add(cd);

            ValueSet.ConceptSetComponent cs = new ValueSet.ConceptSetComponent();
            ValueSet.ExpansionComponent es = new ValueSet.ExpansionComponent();

            cs.System = this.codeSystem.Url;
            cs.Version = this.codeSystem.Version;

            string codeCode = string.Empty;
            string codeDisplay = string.Empty;
            string codeDefinition = string.Empty;

            if ((string.IsNullOrEmpty(version) || version == cs.Version))
            {
                Dictionary<string, string> codeVals = new Dictionary<string, string>();

                codeVals.Add("10000", "European NFD");
                codeVals.Add("11111", "New Zealand European");
                codeVals.Add("12100", "British NFD");
                codeVals.Add("12111", "Celtic");
                codeVals.Add("12112", "Channel Islander");
                codeVals.Add("12113", "Cornish");
                codeVals.Add("12114", "English");
                codeVals.Add("12115", "Gaelic");
                codeVals.Add("12116", "Irish");
                codeVals.Add("12117", "Manx");
                codeVals.Add("12118", "Orkney Islander");
                codeVals.Add("12119", "Scottish (Scots)");
                codeVals.Add("12120", "Shetland Islander");
                codeVals.Add("12121", "Welsh");
                codeVals.Add("12199", "British NEC");
                codeVals.Add("12211", "Dutch/Netherlands");
                codeVals.Add("12311", "Greek (including Greek Cypriot)");
                codeVals.Add("12411", "Polish");
                codeVals.Add("12500", "South Slav (formerly Yugoslav groups) NFD");
                codeVals.Add("12511", "Croat/Croatian");
                codeVals.Add("12512", "Dalmatian");
                codeVals.Add("12513", "Macedonian");
                codeVals.Add("12514", "Serb/Serbian");
                codeVals.Add("12515", "Slovene/Slovenian");
                codeVals.Add("12516", "Bosnian");
                codeVals.Add("12599", "South Slav (formerly Yugoslav groups) NEC");
                codeVals.Add("12611", "Italian");
                codeVals.Add("12711", "German");
                codeVals.Add("12811", "Australian");
                codeVals.Add("12911", "Albanian");
                codeVals.Add("12912", "Armenian");
                codeVals.Add("12913", "Austrian");
                codeVals.Add("12914", "Belgian");
                codeVals.Add("12915", "Bulgarian");
                codeVals.Add("12916", "Belorussian");
                codeVals.Add("12917", "Corsican");
                codeVals.Add("12918", "Cypriot Unspecified");
                codeVals.Add("12919", "Czech");
                codeVals.Add("12920", "Danish");
                codeVals.Add("12921", "Estonian");
                codeVals.Add("12922", "Finnish");
                codeVals.Add("12923", "Flemish");
                codeVals.Add("12924", "French");
                codeVals.Add("12925", "Greenlander");
                codeVals.Add("12926", "Hungarian");
                codeVals.Add("12927", "Icelander");
                codeVals.Add("12928", "Latvian");
                codeVals.Add("12929", "Lithuanian");
                codeVals.Add("12930", "Maltese");
                codeVals.Add("12931", "Norwegian");
                codeVals.Add("12932", "Portuguese");
                codeVals.Add("12933", "Romanian / Rumanian");
                codeVals.Add("12934", "Romany / Gypsy");
                codeVals.Add("12935", "Russian");
                codeVals.Add("12936", "Sardinian");
                codeVals.Add("12937", "Slavic / Slav");
                codeVals.Add("12938", "Slovak");
                codeVals.Add("12939", "Spanish");
                codeVals.Add("12940", "Swedish");
                codeVals.Add("12941", "Swiss");
                codeVals.Add("12942", "Ukrainian");
                codeVals.Add("12943", "American (US)");
                codeVals.Add("12944", "Burgher");
                codeVals.Add("12945", "Canadian");
                codeVals.Add("12946", "Falkland Islander / Kelper");
                codeVals.Add("12947", "New Caledonian");
                codeVals.Add("12948", "South African");
                codeVals.Add("12949", "Afrikaner");
                codeVals.Add("12950", "Zimbabwean");
                codeVals.Add("12999", "European NEC");
                codeVals.Add("21111", "Māori");
                codeVals.Add("30000", "Pacific peoples NFD");
                codeVals.Add("31111", "Samoan");
                codeVals.Add("32100", "Cook Island Māori NFD");
                codeVals.Add("32111", "Aitutaki Islander");
                codeVals.Add("32112", "Atiu Islander");
                codeVals.Add("32113", "Mangaia Islander");
                codeVals.Add("32114", "Manihiki Islander");
                codeVals.Add("32115", "Mauke Islander");
                codeVals.Add("32116", "Mitiaro Islander");
                codeVals.Add("32117", "Palmerston Islander");
                codeVals.Add("32118", "Penrhyn Islander");
                codeVals.Add("32119", "Pukapuka Islander");
                codeVals.Add("32120", "Rakahanga Islander");
                codeVals.Add("32121", "Rarotongan");
                codeVals.Add("33111", "Tongan");
                codeVals.Add("34111", "Niuean");
                codeVals.Add("35111", "Tokelauan");
                codeVals.Add("36111", "Fijian (except Fiji Indian / Indo-Fijian)");
                codeVals.Add("37111", "Admiralty Islander");
                codeVals.Add("37112", "Australian Aboriginal");
                codeVals.Add("37113", "Austral Islander");
                codeVals.Add("37114", "Belau / Palau Islander");
                codeVals.Add("37115", "Bismark Archipelagoan");
                codeVals.Add("37116", "Bougainvillean");
                codeVals.Add("37117", "Caroline Islander");
                codeVals.Add("37118", "Easter Islander");
                codeVals.Add("37119", "Gambier Islander");
                codeVals.Add("37120", "Guadalcanalian");
                codeVals.Add("37121", "Guam Islander / Chamorro");
                codeVals.Add("37122", "Hawaiian");
                codeVals.Add("37123", "Kanaka / Kanak");
                codeVals.Add("37124", "I-Kiribati / Gilbertese");
                codeVals.Add("37125", "Malaitian");
                codeVals.Add("37126", "Manus Islander");
                codeVals.Add("37127", "Marianas Islander");
                codeVals.Add("37128", "Marquesas Islander");
                codeVals.Add("37129", "Marshall Islander");
                codeVals.Add("37130", "Nauru Islander");
                codeVals.Add("37131", "New Britain Islander");
                codeVals.Add("37132", "New Georgian");
                codeVals.Add("37133", "New Irelander");
                codeVals.Add("37134", "Ocean Islander / Banaban");
                codeVals.Add("37135", "Papuan / New Guinean / Irian Jayan");
                codeVals.Add("37136", "Phoenix Islander");
                codeVals.Add("37137", "Pitcairn Islander");
                codeVals.Add("37138", "Rotuman / Rotuman Islander");
                codeVals.Add("37139", "Santa Cruz Islander");
                codeVals.Add("37140", "Society Islander (including Tahitian)");
                codeVals.Add("37141", "Solomon Islander");
                codeVals.Add("37142", "Torres Strait Islander / Thursday Islander");
                codeVals.Add("37143", "Tuamotu Islander");
                codeVals.Add("37144", "Tuvalu Islander / Ellice Islander");
                codeVals.Add("37145", "Vanuatu Islander / New Hebridean");
                codeVals.Add("37146", "Wake Islander");
                codeVals.Add("37147", "Wallis Islander");
                codeVals.Add("37148", "Yap Islander");
                codeVals.Add("37199", "Other Pacific peoples NEC");
                codeVals.Add("40000", "Asian NFD");
                codeVals.Add("41000", "Southeast Asian NFD");
                codeVals.Add("41111", "Filipino");
                codeVals.Add("41211", "Khmer / Kampuchean / Cambodian");
                codeVals.Add("41311", "Vietnamese");
                codeVals.Add("41411", "Burmese");
                codeVals.Add("41412", "Indonesian (including Javanese / Sundanese / Sumatran)");
                codeVals.Add("41413", "Lao / Laotian");
                codeVals.Add("41414", "Malay / Malayan");
                codeVals.Add("41415", "Thai / Tai / Siamese");
                codeVals.Add("41499", "Other Southeast Asian NEC");
                codeVals.Add("42100", "Chinese NFD");
                codeVals.Add("42111", "Hong Kong Chinese");
                codeVals.Add("42112", "Kampuchean Chinese");
                codeVals.Add("42113", "Malaysian Chinese");
                codeVals.Add("42114", "Singaporean Chinese");
                codeVals.Add("42115", "Vietnamese Chinese");
                codeVals.Add("42116", "Taiwanese Chinese");
                codeVals.Add("42199", "Chinese NEC");
                codeVals.Add("43100", "Indian NFD");
                codeVals.Add("43111", "Bengali");
                codeVals.Add("43112", "Fijian Indian / Indo-Fijian");
                codeVals.Add("43113", "Gujarati");
                codeVals.Add("43114", "Tamil");
                codeVals.Add("43115", "Punjabi");
                codeVals.Add("43116", "Sikh");
                codeVals.Add("43117", "Anglo Indian");
                codeVals.Add("43199", "Indian NEC");
                codeVals.Add("44100", "Sri Lankan NFD");
                codeVals.Add("44111", "Sinhalese");
                codeVals.Add("44112", "Sri Lankan Tamil");
                codeVals.Add("44199", "Sri Lankan NEC");
                codeVals.Add("44211", "Japanese");
                codeVals.Add("44311", "Korean");
                codeVals.Add("44411", "Afghani");
                codeVals.Add("44412", "Bangladeshi");
                codeVals.Add("44413", "Nepalese");
                codeVals.Add("44414", "Pakistani");
                codeVals.Add("44415", "Tibetan");
                codeVals.Add("44416", "Eurasian");
                codeVals.Add("44499", "Other Asian NEC");
                codeVals.Add("51100", "Middle Eastern NFD");
                codeVals.Add("51111", "Algerian");
                codeVals.Add("51112", "Arab");
                codeVals.Add("51113", "Assyrian");
                codeVals.Add("51114", "Egyptian");
                codeVals.Add("51115", "Iranian / Persian");
                codeVals.Add("51116", "Iraqi");
                codeVals.Add("51117", "Israeli / Jewish / Hebrew");
                codeVals.Add("51118", "Jordanian");
                codeVals.Add("51119", "Kurd");
                codeVals.Add("51120", "Lebanese");
                codeVals.Add("51121", "Libyan");
                codeVals.Add("51122", "Moroccan");
                codeVals.Add("51123", "Omani");
                codeVals.Add("51124", "Palestinian");
                codeVals.Add("51125", "Syrian");
                codeVals.Add("51126", "Tunisian");
                codeVals.Add("51127", "Turkish (including Turkish Cypriot)");
                codeVals.Add("51128", "Yemeni");
                codeVals.Add("51199", "Middle Eastern NEC");
                codeVals.Add("52100", "Latin American / Hispanic NFD");
                codeVals.Add("52111", "Argentinian");
                codeVals.Add("52112", "Bolivian");
                codeVals.Add("52113", "Brazilian");
                codeVals.Add("52114", "Chilean");
                codeVals.Add("52115", "Colombian");
                codeVals.Add("52116", "Costa Rican");
                codeVals.Add("52117", "Creole (Latin America)");
                codeVals.Add("52118", "Ecuadorian");
                codeVals.Add("52119", "Guatemalan");
                codeVals.Add("52120", "Guyanese");
                codeVals.Add("52121", "Honduran");
                codeVals.Add("52122", "Malvinian (Spanish-speaking Falkland Islander)");
                codeVals.Add("52123", "Mexican");
                codeVals.Add("52124", "Nicaraguan");
                codeVals.Add("52125", "Panamanian");
                codeVals.Add("52126", "Paraguayan");
                codeVals.Add("52127", "Peruvian");
                codeVals.Add("52128", "Puerto Rican");
                codeVals.Add("52129", "Uruguayan");
                codeVals.Add("52130", "Venezuelan");
                codeVals.Add("52199", "Latin American / Hispanic NEC");
                codeVals.Add("53100", "African NFD");
                codeVals.Add("53112", "Creole (US)");
                codeVals.Add("53113", "Jamaican");
                codeVals.Add("53114", "Kenyan");
                codeVals.Add("53115", "Nigerian");
                codeVals.Add("53116", "African American");
                codeVals.Add("53117", "Ugandan");
                codeVals.Add("53118", "West Indian / Caribbean");
                codeVals.Add("53119", "Somali");
                codeVals.Add("53120", "Eritrean");
                codeVals.Add("53121", "Ethiopian");
                codeVals.Add("53122", "Ghanaian");
                codeVals.Add("53199", "Other African NEC");
                codeVals.Add("61111", "Central American Indian");
                codeVals.Add("61112", "Inuit / Eskimo");
                codeVals.Add("61113", "North American Indian");
                codeVals.Add("61114", "South American Indian");
                codeVals.Add("61115", "Mauritian");
                codeVals.Add("61116", "Seychelles Islander");
                codeVals.Add("61117", "South African Coloured");
                codeVals.Add("61118", "New Zealander");
                codeVals.Add("61199", "Other NEC");
                codeVals.Add("94444", "Don't know");
                codeVals.Add("95555", "Refused to answer");
                codeVals.Add("96666", "Repeated Value");
                codeVals.Add("97777", "Response unidentifiable");
                codeVals.Add("98888", "Response Outside Scope");
                codeVals.Add("99999", "Not stated");

                foreach (KeyValuePair<string, string> codeVal in codeVals)
                {
                    if (TerminologyValueSet.MatchValue(codeVal.Key, codeVal.Value, code, filter))
                    {
                        cs.Concept.Add(new ValueSet.ConceptReferenceComponent { Code = codeVal.Key, Display = codeVal.Value });
                        es.Contains.Add(new ValueSet.ContainsComponent { Code = codeVal.Key, Display = codeVal.Value, System = cs.System });
                        this.codeSystem.Concept.Add(new CodeSystem.ConceptDefinitionComponent { Code = codeVal.Key, Display = codeVal.Value });
                    }
                }

                if (termOp == TerminologyOperation.expand || termOp == TerminologyOperation.validate_code)
                {
                    this.valueSet = TerminologyValueSet.AddExpansion(this.valueSet, es, offsetNo, countNo);
                }
                else if (termOp == TerminologyOperation.define_vs)
                {
                    this.valueSet.Compose = new ValueSet.ComposeComponent();
                    this.valueSet.Compose.Include.Add(cs);
                }
            }
        }
    }
}
