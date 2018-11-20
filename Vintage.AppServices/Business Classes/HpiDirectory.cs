namespace Vintage.AppServices.BusinessClasses
{
    public class HimDirectory
    {
        public string HpiFacilityId { get; set; }
        public string EDI { get; set; }
        public bool? HimOnLine { get; set; }
    }

    public class HpiFacility
    {
        public string FacilityId { get; set; }
        public string FacilityName { get; set; }
        public string FacilityAddress { get; set; }
        public string FacilityTypeCode { get; set; }
        public string FacilityTypeName { get; set; }
        public string OrganisationId { get; set; }
    }

    public class HpiOrganisation
    {
        public string OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public string OrganisationAddress { get; set; }
        public string OrganisationTypeCode { get; set; }
        public string OrganisationTypeName { get; set; }
    }

}
