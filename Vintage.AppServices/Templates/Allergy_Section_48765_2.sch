<!-- Allergy_Section_48765_2.sch -->
<!-- Schematron for HISO 10043.3 CDA Templates: Allergy Section -->
<!-- Peter Jordan: 29 June 2012 -->
<!-- Copyright (c) 2012 Patients First -->

<schema  xmlns="http://www.ascc.net/xml/schematron">	
  <title>Schematron schema for validating conformance to NZ CDA Templates: Allergy Section</title>
  <ns prefix="cda" uri="urn:hl7-org:v3"/>
  <ns prefix="xsi" uri="http://www.w3.org/2001/XMLSchema-instance"/>

  <!-- errors -->
  
  <pattern id='allergyErrors'>
    <title>Allergy Section - error validation phase</title>
    <rule context='*[cda:templateId/@root="2.16.840.1.113883.2.18.7.46"]'>
      <assert test='self::cda:section'>The root of the Allergy section shall be 'section' in the HL7 namespace.</assert>
      <assert test='count(cda:text)=1'>The Allergy section SHALL contain a single narrative block.</assert>
      <assert test='count(cda:code)=1'>The Allergy section SHALL contain a single Section / code.</assert>
      <assert test='cda:code[@code="48765-2"]'>The Allergy section SHALL contain Section / code = 48765-2.</assert>
      <assert test='cda:code[@codeSystem="2.16.840.1.113883.6.1"]'>The Allergy section / code SHALL come from LOINC.</assert>
      <assert test='count(cda:title)=1'>The Allergy section SHALL contain Section / title.</assert>
      <assert test="descendant::*[cda:templateId/@root='2.16.840.1.113883.2.18.7.46.1']">The Allergy section SHALL include one or more Problem Acts (templateId 2.16.840.1.113883.2.18.7.46.1).</assert>
    </rule>
    <rule context="*[cda:templateId/@root='2.16.840.1.113883.2.18.7.46.1'][ancestor::*[cda:templateId/@root='2.16.840.1.113883.2.18.7.46']]">
      <assert test="count(descendant::*[cda:templateId/@root='2.16.840.1.113883.2.18.7.46.2'])=1">An Allergy Section Problem Act SHALL include one Alert Observation (templateId 2.16.840.1.113883.2.18.7.46.2).</assert>
    </rule>
    <rule context="*[cda:templateId/@root='2.16.840.1.113883.2.18.7.46.2'][ancestor::*[cda:templateId/@root='2.16.840.1.113883.2.18.7.46.1']]">
      <assert test="count(descendant::*[cda:templateId/@root='2.16.840.1.113883.2.18.7.46.3'])=1">An Allergy Section Problem Act Alert Observation SHALL include one Status Observation (templateId 2.16.840.1.113883.2.18.7.46.3).</assert>
      <assert test="count(descendant::*[cda:templateId/@root='2.16.840.1.113883.2.18.7.47'])=1">An Allergy Section Problem Act Alert Observation SHALL include one Reaction Observation (templateId 2.16.840.1.113883.2.18.7.47).</assert>
      <assert test="count(descendant::*[cda:templateId/@root='2.16.840.1.113883.2.18.7.53'])=1">An Allergy Section Problem Act Alert Observation SHALL include one Severity Observation (templateId 2.16.840.1.113883.2.18.7.53).</assert>
    </rule>
  </pattern>

  <!-- warning -->

  <pattern id='allergyWarnings'>
    <title>Allergy section - warning validation phase</title>
    <rule context='*[cda:templateId/@root="2.16.840.1.113883.2.18.7.46"]'>
      <assert test="contains(translate(cda:title,'QWERTYUIOPASDFGHJKLZXCVBNM','qwertyuiopasdfghjklzxcvbnm'),'allergies and adverse reactions')">The Allergies Section / title SHOULD be valued with a case-insensitive language-insensitive text string containing "allergies and adverse reactions".</assert>
    </rule>
  </pattern>

  <!-- manual -->

  <pattern id='allegyManual'>
    <title>Allergy section - manual validation phase</title>
    <rule context='*[cda:templateId/@root="2.16.840.1.113883.2.18.7.46"]'>
      <report test='.'></report>
    </rule>
  </pattern>


</schema>