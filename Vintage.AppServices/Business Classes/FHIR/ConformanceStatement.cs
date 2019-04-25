namespace Vintage.AppServices.BusinessClasses.FHIR
{
    using Hl7.Fhir.Model;
    using System.Collections.Specialized;
    using System.Xml.Linq;

    public static class ServerCapability
    {
        public const string HL7_FHIR_CANONICAL = "http://hl7.org/fhir";
        public const string TERMINZ_CANONICAL = "http://its.patientsfirst.org.nz/RestService.svc/Terminz";
        public const string TERMINZ_DESCRIPTION = "HL7© FHIR© terminology services for use in New Zealand.";
        public static Resource GetStatement(NameValueCollection queryParam)
        {
            string modeVal = Utilities.GetQueryValue("mode", queryParam);
            if (modeVal.ToLower() == "terminology")
            {
                return TerminologyCapability.GetStatement(false);
            }

            CapabilityStatement capabilityStatement = new CapabilityStatement
            {
                Url = TERMINZ_CANONICAL,
                Id = "capability-fhir-server-patients-first",
                Description = new Markdown(TERMINZ_DESCRIPTION),
                Name = "Patients First Terminology Server (Terminz)",
                Publisher = "Patients First Ltd",
                Date = "2019-04-25",
                Version = "4.0.0",
                FhirVersion = FHIRVersion.N4_0_0,
                //Language = "en-NZ",  // review with next version of library - this needs to be a coded element!
                Status = PublicationStatus.Draft,
                //capabilityStatement.AcceptUnknown = CapabilityStatement.UnknownContentCode.Both;
                Experimental = true,
                Format = new string[] { "json", "xml" },
                Software = new CapabilityStatement.SoftwareComponent { Name = "Health Intelligence Platform", Version = "6.9.3.0", ReleaseDate = "2019-04-25" },
                Kind = CapabilityStatementKind.Instance,
                Implementation = new CapabilityStatement.ImplementationComponent { Description = TERMINZ_DESCRIPTION, Url = TERMINZ_CANONICAL }
            };

            ContactDetail cd = new ContactDetail { Name = "Peter Jordan" };
            ContactPoint cp = new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = "pkjordan@xtra.co.nz" };
            cd.Telecom.Add(cp);
            capabilityStatement.Contact.Add(cd);

            // Add Extensions for supported Code Systems
            capabilityStatement.AddExtension(HL7_FHIR_CANONICAL + "/StructureDefinition/capabilitystatement-supported-system", new FhirUri("http://snomed.info/sct"));
            capabilityStatement.AddExtension(HL7_FHIR_CANONICAL + "/StructureDefinition/capabilitystatement-supported-system", new FhirUri("http://loinc.org"));
            capabilityStatement.AddExtension(HL7_FHIR_CANONICAL + "/StructureDefinition/capabilitystatement-supported-system", new FhirUri("http://nzmt.org.nz"));

            // create server object

            var server = new CapabilityStatement.RestComponent
            {
                Mode = CapabilityStatement.RestfulCapabilityMode.Server,
                Documentation = new Markdown("RESTful Terminology Server")
            };

            server.Interaction.Add(new CapabilityStatement.SystemInteractionComponent
                { Code = CapabilityStatement.SystemRestfulInteraction.Batch,
                  Documentation = new Markdown("Supported for validate-code and translate operations.") });              

            // Add CORS security
            server.Security = new CapabilityStatement.SecurityComponent{ Cors = true };

            // create resource objects and add them to server object....

            // Capability Statement - only if /CapabilityStatement is supported where multiple CS exist

            // TerminologyCapabilities

            var resourceTerminologyCapabilities = new CapabilityStatement.ResourceComponent
            {
                Type = ResourceType.TerminologyCapabilities,
                ReadHistory = false,
                UpdateCreate = false,
                Versioning = CapabilityStatement.ResourceVersionPolicy.NoVersion
            };

            // add interactions
            var confReadcs = new CapabilityStatement.ResourceInteractionComponent
            {
                Code = CapabilityStatement.TypeRestfulInteraction.Read,
                Documentation = new Markdown("Read allows clients to get the logical definition of the Terminology Capability Statement")
            };

            resourceTerminologyCapabilities.Interaction.Add(confReadcs);

            // Operation Definition

            var resourceOperationDefinition = new CapabilityStatement.ResourceComponent
            {
                Type = ResourceType.OperationDefinition,
                ReadHistory = false,
                UpdateCreate = false,
                Versioning = CapabilityStatement.ResourceVersionPolicy.NoVersion
            };

            // add interactions
            var confReadOd = new CapabilityStatement.ResourceInteractionComponent
            {
                Code = CapabilityStatement.TypeRestfulInteraction.Read,
                Documentation = new Markdown("Read allows clients to get the logical definition of the Operation Definition")
            };

            resourceOperationDefinition.Interaction.Add(confReadOd);

            var confSearchOd = new CapabilityStatement.ResourceInteractionComponent
            {
                Code = CapabilityStatement.TypeRestfulInteraction.SearchType,
                Documentation = new Markdown("Search allows clients to find Operation Definitions on the server")
            };

            resourceOperationDefinition.Interaction.Add(confSearchOd);

            // add search parameters

            var spOd0 = new CapabilityStatement.SearchParamComponent { Name = "identifier", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/OperationDefinition-identifier" };
            resourceOperationDefinition.SearchParam.Add(spOd0);

            var spOd1 = new CapabilityStatement.SearchParamComponent { Name = "url", Type = SearchParamType.Uri, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/OperationDefinition-url" };
            resourceOperationDefinition.SearchParam.Add(spOd1);

            // CodeSystem

            var resourceCodeSystem = new CapabilityStatement.ResourceComponent
            {
                Type = ResourceType.CodeSystem,
                ReadHistory = false,
                UpdateCreate = false,
                Versioning = CapabilityStatement.ResourceVersionPolicy.NoVersion
            };

            // add interactions
            var confRead0 = new CapabilityStatement.ResourceInteractionComponent
            {
                Code = CapabilityStatement.TypeRestfulInteraction.Read,
                Documentation = new Markdown("Read allows clients to get the logical definitions of Code Systems")
            };
            resourceCodeSystem.Interaction.Add(confRead0);

            var confSearch0 = new CapabilityStatement.ResourceInteractionComponent
            {
                Code = CapabilityStatement.TypeRestfulInteraction.SearchType,
                Documentation = new Markdown("Search allows clients to find Code Systems on the server")
            };
            resourceCodeSystem.Interaction.Add(confSearch0);

            // add search parameters
            var sp1 = new CapabilityStatement.SearchParamComponent { Name = "code", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/CodeSystem-code" };
            resourceCodeSystem.SearchParam.Add(sp1);

            var sp2 = new CapabilityStatement.SearchParamComponent { Name = "date", Type = SearchParamType.Date, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/CodeSystem-date" };
            //resourceCodeSystem.SearchParam.Add(sp2);

            var sp3 = new CapabilityStatement.SearchParamComponent { Name = "name", Type = SearchParamType.String, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/CodeSystem-name" };
            //resourceCodeSystem.SearchParam.Add(sp3);

            var sp4 = new CapabilityStatement.SearchParamComponent { Name = "reference", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/CodeSystem-reference" };
            //resourceCodeSystem.SearchParam.Add(sp4);

            var sp5 = new CapabilityStatement.SearchParamComponent { Name = "status", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/CodeSystem-status" };
            //resourceCodeSystem.SearchParam.Add(sp5);

            var sp6 = new CapabilityStatement.SearchParamComponent { Name = "system", Type = SearchParamType.Uri, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/CodeSystem-system" };
            resourceCodeSystem.SearchParam.Add(sp6);

            var sp7 = new CapabilityStatement.SearchParamComponent { Name = "url", Type = SearchParamType.Uri, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/CodeSystem-url" };
            resourceCodeSystem.SearchParam.Add(sp7);

            var sp8 = new CapabilityStatement.SearchParamComponent { Name = "version", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/CodeSystem-version" };
            resourceCodeSystem.SearchParam.Add(sp8);

            var sp9 = new CapabilityStatement.SearchParamComponent { Name = "identifier", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/CodeSystem-identifier" };
            resourceCodeSystem.SearchParam.Add(sp9);

            var sp1a = new CapabilityStatement.SearchParamComponent { Name = "content-mode", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/CodeSystem-content-mode" };
            resourceCodeSystem.SearchParam.Add(sp1a);

            var sp1b = new CapabilityStatement.SearchParamComponent { Name = "supplements", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/CodeSystem-supplements" };
            resourceCodeSystem.SearchParam.Add(sp1b);
            
            // ValueSet

            var resourceValueSet = new CapabilityStatement.ResourceComponent
            {
                Type = ResourceType.ValueSet,
                ReadHistory = false,
                UpdateCreate = false,
                Versioning = CapabilityStatement.ResourceVersionPolicy.NoVersion
            };

            // add interactions
            var confRead = new CapabilityStatement.ResourceInteractionComponent
            {
                Code = CapabilityStatement.TypeRestfulInteraction.Read,
                Documentation = new Markdown("Read allows clients to get the logical definitions of Value Sets")
            };
            resourceValueSet.Interaction.Add(confRead);

            var confSearch = new CapabilityStatement.ResourceInteractionComponent
            {
                Code = CapabilityStatement.TypeRestfulInteraction.SearchType,
                Documentation = new Markdown("Search allows clients to find Value Sets on the server")
            };
            resourceValueSet.Interaction.Add(confSearch);

            // add search parameters
            var sp11 = new CapabilityStatement.SearchParamComponent 
                {Name = "code", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ValueSet-code"} ;
            resourceValueSet.SearchParam.Add(sp11);

            var sp12 = new CapabilityStatement.SearchParamComponent 
                { Name = "date", Type = SearchParamType.Date, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ValueSet-date" };
            //resourceValueSet.SearchParam.Add(sp12);

            var sp13 = new CapabilityStatement.SearchParamComponent 
                { Name = "name", Type = SearchParamType.String, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ValueSet-name" };
            //resourceValueSet.SearchParam.Add(sp13);

            var sp14 = new CapabilityStatement.SearchParamComponent 
                { Name = "reference", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ValueSet-reference" };
            //resourceValueSet.SearchParam.Add(sp14);

            var sp15 = new CapabilityStatement.SearchParamComponent 
                { Name = "status", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ValueSet-status" };
            //resourceValueSet.SearchParam.Add(sp15);

            var sp16 = new CapabilityStatement.SearchParamComponent 
                { Name = "system", Type = SearchParamType.Uri, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ValueSet-system" };
            resourceValueSet.SearchParam.Add(sp16);

            var sp17 = new CapabilityStatement.SearchParamComponent 
                { Name = "url", Type = SearchParamType.Uri, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ValueSet-url" };
            resourceValueSet.SearchParam.Add(sp17);

            var sp18 = new CapabilityStatement.SearchParamComponent 
                { Name = "version", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ValueSet-version" };
            resourceValueSet.SearchParam.Add(sp18);

            var sp19 = new CapabilityStatement.SearchParamComponent
            { Name = "identifier", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ValueSet-identifier" };
            resourceValueSet.SearchParam.Add(sp19);

            // ConceptMap 

            var resourceConceptMap = new CapabilityStatement.ResourceComponent
            {
                Type = ResourceType.ConceptMap,
                ReadHistory = false,
                UpdateCreate = false,
                Versioning = CapabilityStatement.ResourceVersionPolicy.NoVersion
            };

            // add interactions
            var confRead2 = new CapabilityStatement.ResourceInteractionComponent
            {
                Code = CapabilityStatement.TypeRestfulInteraction.Read,
                Documentation = new Markdown("Read allows clients to get the logical definitions of Concept Maps")
            };
            resourceConceptMap.Interaction.Add(confRead2);

            var confSearch2 = new CapabilityStatement.ResourceInteractionComponent
            {
                Code = CapabilityStatement.TypeRestfulInteraction.SearchType,
                Documentation = new Markdown("Search allows clients to find Concept Maps on the server")
            };
            resourceConceptMap.Interaction.Add(confSearch2);

            // add search parameters

            var sp21 = new CapabilityStatement.SearchParamComponent 
                { Name = "date", Type = SearchParamType.Date, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ConceptMap-date" };
            //resourceConceptMap.SearchParam.Add(sp21);

            var sp22 = new CapabilityStatement.SearchParamComponent 
                { Name = "name", Type = SearchParamType.String, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ConceptMap-name" };
            //resourceConceptMap.SearchParam.Add(sp22);

            var sp23 = new CapabilityStatement.SearchParamComponent 
                { Name = "status", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ConceptMap-status" };
            //resourceConceptMap.SearchParam.Add(sp23);

            var sp24 = new CapabilityStatement.SearchParamComponent 
                { Name = "source", Type = SearchParamType.Uri, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ConceptMap-source" };
            resourceConceptMap.SearchParam.Add(sp24);

            var sp25 = new CapabilityStatement.SearchParamComponent 
                { Name = "target", Type = SearchParamType.Uri, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ConceptMap-target" };
            resourceConceptMap.SearchParam.Add(sp25);

            var sp26 = new CapabilityStatement.SearchParamComponent 
                { Name = "url", Type = SearchParamType.Uri, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ConceptMap-url" };
            resourceConceptMap.SearchParam.Add(sp26);

            var sp27 = new CapabilityStatement.SearchParamComponent 
                { Name = "version", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ConceptMap-version" };
            resourceConceptMap.SearchParam.Add(sp27);

            var sp28 = new CapabilityStatement.SearchParamComponent
            { Name = "identifier", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/ConceptMap-identifier" };
            resourceConceptMap.SearchParam.Add(sp28);

            // Naming System

            var resourceNamingSystem = new CapabilityStatement.ResourceComponent
            {
                Type = ResourceType.NamingSystem,
                ReadHistory = false,
                UpdateCreate = false,
                Versioning = CapabilityStatement.ResourceVersionPolicy.NoVersion
            };

            // add interactions
            var confReadNs = new CapabilityStatement.ResourceInteractionComponent
            {
                Code = CapabilityStatement.TypeRestfulInteraction.Read,
                Documentation = new Markdown("Read allows clients to get the logical definitions of Naming Systems")
            };
            resourceNamingSystem.Interaction.Add(confReadNs);

            var confSearchNs = new CapabilityStatement.ResourceInteractionComponent
            {
                Code = CapabilityStatement.TypeRestfulInteraction.SearchType,
                Documentation = new Markdown("Search allows clients to find Naming Systems on the server")
            };
            resourceNamingSystem.Interaction.Add(confSearchNs);

            // add search parameters

            var spNs = new CapabilityStatement.SearchParamComponent
            { Name = "identifier", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/NamingSystem-identifier" };
            resourceNamingSystem.SearchParam.Add(spNs);

            var spNs2 = new CapabilityStatement.SearchParamComponent
            { Name = "url", Type = SearchParamType.Uri, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/NamingSystem-url" };
            resourceNamingSystem.SearchParam.Add(spNs2);

            var spNs3 = new CapabilityStatement.SearchParamComponent
            { Name = "value", Type = SearchParamType.String, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/NamingSystem-value" };
            resourceNamingSystem.SearchParam.Add(spNs3);

            var spNs4 = new CapabilityStatement.SearchParamComponent
            { Name = "kind", Type = SearchParamType.String, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/NamingSystem-kind" };
            resourceNamingSystem.SearchParam.Add(spNs4);

            // Location

            var resourceLocation = new CapabilityStatement.ResourceComponent
            {
                Type = ResourceType.Location,
                UpdateCreate = false,
                Versioning = CapabilityStatement.ResourceVersionPolicy.NoVersion
            };

            // add interactions
            //var confRead3 = new CapabilityStatement.ResourceInteractionComponent();
            //confRead3.Code = CapabilityStatement.TypeRestfulInteraction.Read;
            //confRead3.Documentation = "Read allows clients to get the logical definitions of the locations";
            //resourceLocation.Interaction.Add(confRead3);

            var confSearch3 = new CapabilityStatement.ResourceInteractionComponent
            {
                Code = CapabilityStatement.TypeRestfulInteraction.SearchType,
                Documentation = new Markdown("Search allows clients to find Locations (NZ Healthcare Facilities) on the server")
            };
            resourceLocation.Interaction.Add(confSearch3);

            // add search parameters

            var sp31 = new CapabilityStatement.SearchParamComponent
            { Name = "address", Type = SearchParamType.Date, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/Location-address" };
            resourceLocation.SearchParam.Add(sp31);

            var sp32 = new CapabilityStatement.SearchParamComponent
            { Name = "address-city", Type = SearchParamType.String, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/Location-address-city" };
            resourceLocation.SearchParam.Add(sp32);

            var sp33 = new CapabilityStatement.SearchParamComponent
            { Name = "address-postalcode", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/Location-address-postalcode" };
            resourceLocation.SearchParam.Add(sp33);

            var sp34 = new CapabilityStatement.SearchParamComponent
            { Name = "identifier", Type = SearchParamType.Uri, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/Location-identifier" };
            resourceLocation.SearchParam.Add(sp34);

            var sp35 = new CapabilityStatement.SearchParamComponent
            { Name = "name", Type = SearchParamType.Uri, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/Location-name" };
            resourceLocation.SearchParam.Add(sp35);

            var sp36 = new CapabilityStatement.SearchParamComponent
            { Name = "type", Type = SearchParamType.Uri, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/Location-type" };
            resourceLocation.SearchParam.Add(sp36);

            // Organization

            var resourceOrganization = new CapabilityStatement.ResourceComponent
            {
                Type = ResourceType.Organization,
                ReadHistory = false,
                UpdateCreate = false,
                Versioning = CapabilityStatement.ResourceVersionPolicy.NoVersion
            };

            // add interactions
            //var confRead4 = new CapabilityStatement.ResourceInteractionComponent();
            //confRead4.Code = CapabilityStatement.TypeRestfulInteraction.Read;
            //confRead4.Documentation = "Read allows clients to get the logical definitions of the locations";
            //resourceOrganization.Interaction.Add(confRead4);

            var confSearch4 = new CapabilityStatement.ResourceInteractionComponent
            {
                Code = CapabilityStatement.TypeRestfulInteraction.SearchType,
                Documentation = new Markdown("Search allows clients to find Organizations on the server")
            };
            resourceOrganization.Interaction.Add(confSearch4);

            // add search parameters

            var sp41 = new CapabilityStatement.SearchParamComponent
            { Name = "address", Type = SearchParamType.Date, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/Organization-address" };
            resourceOrganization.SearchParam.Add(sp41);

            var sp42 = new CapabilityStatement.SearchParamComponent
            { Name = "address-city", Type = SearchParamType.String, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/Organization-address-city" };
            resourceOrganization.SearchParam.Add(sp42);

            var sp43 = new CapabilityStatement.SearchParamComponent
            { Name = "address-postalcode", Type = SearchParamType.Token, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/Organization-address-postalcode" };
            resourceOrganization.SearchParam.Add(sp43);

            var sp44 = new CapabilityStatement.SearchParamComponent
            { Name = "identifier", Type = SearchParamType.Uri, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/Organization-identifier" };
            resourceOrganization.SearchParam.Add(sp44);

            var sp45 = new CapabilityStatement.SearchParamComponent
            { Name = "name", Type = SearchParamType.Uri, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/Organization-name" };
            resourceOrganization.SearchParam.Add(sp45);

            var sp46 = new CapabilityStatement.SearchParamComponent
            { Name = "type", Type = SearchParamType.Uri, Definition = HL7_FHIR_CANONICAL + "/SearchParameter/Organization-type" };
            resourceOrganization.SearchParam.Add(sp46);

            // add resources to server object

            //server.Resource.Add(resourceCapabilityStatement);
            
            server.Resource.Add(resourceTerminologyCapabilities);
            server.Resource.Add(resourceOperationDefinition);
            server.Resource.Add(resourceCodeSystem);
            server.Resource.Add(resourceValueSet);
            server.Resource.Add(resourceConceptMap);
            server.Resource.Add(resourceNamingSystem);
            server.Resource.Add(resourceLocation);
            server.Resource.Add(resourceOrganization);

            // add operations to server object

            var opCom1 = new CapabilityStatement.OperationComponent { Name = "lookup", Definition = TERMINZ_CANONICAL + "/OperationDefinition/CodeSystem-lookup" };
            server.Operation.Add(opCom1);
            
            var opCom1a = new CapabilityStatement.OperationComponent { Name = "validate-code", Definition = TERMINZ_CANONICAL + "/OperationDefinition/CodeSystem-validate-code" };
            //opCom5.FhirCommentsElement.Add(new FhirString("Supported in batch request"));
            server.Operation.Add(opCom1a);
   
            var opCom2 = new CapabilityStatement.OperationComponent { Name = "subsumes", Definition = TERMINZ_CANONICAL + "/OperationDefinition/CodeSystem-subsumes" };
            server.Operation.Add(opCom2);

            var opCom3 = new CapabilityStatement.OperationComponent { Name = "find-matches", Definition = HL7_FHIR_CANONICAL + "/OperationDefinition/CodeSystem-find-matches" };
            server.Operation.Add(opCom3);

            var opCom4 = new CapabilityStatement.OperationComponent { Name = "expand", Definition = TERMINZ_CANONICAL + "/OperationDefinition/ValueSet-expand" };
            server.Operation.Add(opCom4);

            var opCom5 = new CapabilityStatement.OperationComponent { Name = "validate-code", Definition = TERMINZ_CANONICAL + "/OperationDefinition/ValueSet-validate-code" };
            //opCom5.FhirCommentsElement.Add(new FhirString("Supported in batch request"));
            server.Operation.Add(opCom5);

            var opCom6 = new CapabilityStatement.OperationComponent { Name = "translate", Definition = TERMINZ_CANONICAL + "/OperationDefinition/ConceptMap-translate" };
            //opCom6.FhirCommentsElement.Add(new FhirString("Supported in batch request"));
            server.Operation.Add(opCom6);

            var opCom7 = new CapabilityStatement.OperationComponent { Name = "closure", Definition = HL7_FHIR_CANONICAL + "/OperationDefinition/ConceptMap-closure" };
            server.Operation.Add(opCom7);

            var opCom8 = new CapabilityStatement.OperationComponent { Name = "preferred-id", Definition = HL7_FHIR_CANONICAL + "/OperationDefinition/NamingSystem-preferred-id" };
            server.Operation.Add(opCom8);

            var opCom9 = new CapabilityStatement.OperationComponent { Name = "versions", Definition = HL7_FHIR_CANONICAL + "/OperationDefinition/CapabilityStatement-versions" };
            server.Operation.Add(opCom9);

            // add server to capability statement object
            capabilityStatement.Rest.Add(server);

            // create text
            AddNarrative(capabilityStatement,server);

            return capabilityStatement;            
        }

        internal static CapabilityStatement AddNarrative(CapabilityStatement capabilityStatement, CapabilityStatement.RestComponent server)
        {
            // create display text for Location Resource
            string textString = string.Empty;

            try
            {

                XNamespace ns = "http://www.w3.org/1999/xhtml";

                var summary = new XElement(ns + "div",
                    new XElement(ns + "h2", capabilityStatement.Name),
                    new XElement(ns + "p", capabilityStatement.Description),
                    new XElement(ns + "table",
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Mode"),
                        new XElement(ns + "td", server.Mode.ToString())
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Description"),
                        new XElement(ns + "td", server.Documentation)
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "FHIR Version"),
                        new XElement(ns + "td", capabilityStatement.FhirVersion)
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Date"),
                        new XElement(ns + "td", capabilityStatement.Date)
                        )
                     ),
                    new XElement(ns + "table",
                        new XElement(ns + "tr",
                        new XElement(ns + "th", "Resource Type"),
                        new XElement(ns + "th", "Read"),
                        new XElement(ns + "th", "V-Read"),
                        new XElement(ns + "th", "Search"),
                        new XElement(ns + "th", "Update"),
                        new XElement(ns + "th", "Updates"),
                        new XElement(ns + "th", "Create"),
                        new XElement(ns + "th", "Delete"),
                        new XElement(ns + "th", "History")
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "TerminologyCapabilities"),
                        new XElement(ns + "td", "y"),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", "")
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "OperationDefinition"),
                        new XElement(ns + "td", "y"),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", "y"),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", "")
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "CodeSystem"),
                        new XElement(ns + "td", "y"),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", "y"),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", "")
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "ValueSet"),
                        new XElement(ns + "td", "y"),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", "y"),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", "")
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "ConceptMap"),
                        new XElement(ns + "td", "y"),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", "y"),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", "")
                        ),
                         new XElement(ns + "tr",
                        new XElement(ns + "td", "NamingSystem"),
                        new XElement(ns + "td", "y"),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", "y"),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", "")
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Location"),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", "y"),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", "")
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "Organization"),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", "y"),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", ""),
                        new XElement(ns + "td", "")
                        )),
                   new XElement(ns + "table",
                        new XElement(ns + "tr",
                        new XElement(ns + "th", "Operation"),
                        new XElement(ns + "th", "")
                        ),
                         new XElement(ns + "tr",
                        new XElement(ns + "td", "CodeSystem-lookup"),
                        new XElement(ns + "td", "y")
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "CodeSystem-validate-code"),
                        new XElement(ns + "td", "y")
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "CodeSystem-subsumes"),
                        new XElement(ns + "td", "y")
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "CodeSystem-find-matches"),
                        new XElement(ns + "td", "y")
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "ValueSet-expand"),
                        new XElement(ns + "td", "y")
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "ValueSet-validate-code"),
                        new XElement(ns + "td", "y")
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "ConceptMap-translate"),
                        new XElement(ns + "td", "y")
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "ConceptMap-closure"),
                        new XElement(ns + "td", "y")
                        ),
                        new XElement(ns + "tr",
                        new XElement(ns + "td", "NamingSystem-preferred-id"),
                        new XElement(ns + "td", "y")
                        )
                     )
                  //,
                  //new XElement(ns + "table",
                  //   new XElement(ns + "tr",
                  //   new XElement(ns + "th", "Supported Code Systems")
                  //   ),
                  //   new XElement(ns + "tr",
                  //   new XElement(ns + "td", "http://snomed.info/sct")
                  //   ),
                  //   new XElement(ns + "tr",
                  //   new XElement(ns + "td", "http://loinc.org")
                  //   ),
                  //   new XElement(ns + "tr",
                  //   new XElement(ns + "td", "http://nzmt.org.nz")
                  //   )
                  //)
                  );
                textString = summary.ToString();
            }
            catch
            { }

            capabilityStatement.Text = new Narrative
            {
                Status = Narrative.NarrativeStatus.Generated,
                Div = textString
            };
            
            return capabilityStatement;
        }

        public static Resource GetVersions()
        {
            Parameters param = new Parameters();
            param.Add("version", new FhirString("4.0"));
            param.Add("default", new FhirString("4.0"));
            return param;
        }

    }

}
