namespace Vintage.AppServices.DataAccessClasses
{
    public static class ValidationResult
    {
        public static void AddValidationResult(string applicationName, string applicationVersion, string specification, bool passedTransport, bool passedFormat, bool passedData, string reportTransport, string reportFormat, string reportData, string createdBy)
        {
            using (PatientsFirstDataContext dc = new PatientsFirstDataContext())
            {
                dc.ValidationResult_Insert(applicationName, applicationVersion, specification, passedTransport, passedFormat, passedData, reportTransport, reportFormat, reportData, createdBy);
            }
        }
    }
}
