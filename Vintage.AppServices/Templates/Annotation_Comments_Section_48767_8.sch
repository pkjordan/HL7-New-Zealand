<!-- Annotation_Comments_Section_48767_8.sch -->
<!-- Schematron for HISO 10043.3 CDA Templates: Annotation Comments Section -->
<!-- Peter Jordan: 20 June 2012 -->
<!-- Copyright (c) 2012 Patients First -->

<schema  xmlns="http://www.ascc.net/xml/schematron">	
  <title>Schematron schema for validating conformance to NZ CDA Templates: Annotation Comments Section</title>
  <ns prefix="cda" uri="urn:hl7-org:v3"/>
  <ns prefix="xsi" uri="http://www.w3.org/2001/XMLSchema-instance"/>

  <!-- errors -->
  
  <pattern id='annotationCommentErrors'>
    <title>Annotation Comments Section - error validation phase</title>
    <rule context='*[cda:templateId/@root="2.16.840.1.113883.2.18.7.2.25"]'>
      <assert test='self::cda:section'>The root of the Annotation Comments section shall be 'section' in the HL7 namespace.</assert>
      <assert test='count(cda:text)=1'>The Annotation Comments section SHALL contain a single narrative block.</assert>
      <assert test='count(cda:code)=1'>The Annotation Comments section SHALL contain a single Section / code.</assert>
      <assert test='cda:code[@code="48767-8"]'>The Annotation Comments section SHALL contain Section / code = 48767-8.</assert>
      <assert test='cda:code[@codeSystem="2.16.840.1.113883.6.1"]'>The Annotation Comments section / code SHALL come from LOINC.</assert>
      <assert test='count(cda:title)=1'>The Annotation Comments section SHALL contain a single Section / title.</assert>
    </rule>
  </pattern>

  <!-- warning -->

  <pattern id='annotationCommentWarnings'>
    <title>Annotation Comments section - warning validation phase</title>
    <rule context='*[cda:templateId/@root="2.16.840.1.113883.2.18.7.2.25"]'>
      <assert test="contains(translate(cda:title,'QWERTYUIOPASDFGHJKLZXCVBNM','qwertyuiopasdfghjklzxcvbnm'),'annotation comment')">The Annotation Comments section / title SHOULD be valued with a case-insensitive language-insensitive text string containing "annotation comment".</assert>
    </rule>
  </pattern>

  <!-- manual -->

  <pattern id='annotationCommentManual'>
    <title>Annotation Comments section - manual validation phase</title>
    <rule context='*[cda:templateId/@root="2.16.840.1.113883.2.18.7.2.25"]'>
      <report test='.'></report>
    </rule>
  </pattern>


</schema>