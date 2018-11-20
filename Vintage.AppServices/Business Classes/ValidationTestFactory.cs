namespace Vintage.AppServices.BusinessClasses
{

    /// <summary>
    /// Validation Test Factory Class
    /// </summary>
    public class ValidationTestFactory
    {

        /// <summary>
        /// Create and return Validation Test Object of the passed-in Type. 
        /// </summary>
        /// <param name="specType">Specification Type</param>
        /// <returns>Instance of the passed class that implements the IValidationTest Interface</returns>
        /// <remarks>Implements Abstract Factory Pattern</remarks>
        public static IValidationTest Create(SpecificationType specType)
        {
            IValidationTest testInstance = null;

            switch (specType)
            {
                case SpecificationType.GP2GP:
                    testInstance = new Gp2Gp();
                    break;
                case SpecificationType.ePrescribing:
                    testInstance = new ePrescription();
                    break;
                case SpecificationType.eDischargeSummary:
                    testInstance = new eDischargeSummary();
                    break;
                case SpecificationType.PharmacyHealthSummary:
                    testInstance = new PharmacyHealthSummary();
                    break;
                case SpecificationType.CdaTemplates:
                    testInstance = new CdaTemplates();
                    break;
                case SpecificationType.InterRaiCommunityHealth:
                    testInstance = new InterRaiCommunityHealth();
                    break;
                case SpecificationType.InterRaiHomeCare:
                    testInstance = new InterRaiHomeCare();
                    break;
                case SpecificationType.InterRaiLongTermCareFacility:
                    testInstance = new InterRaiLongTermCareFacility();
                    break;
                case SpecificationType.InterRaiContact:
                    testInstance = new InterRaiContact();
                    break;
                case SpecificationType.GP2GPV2:
                    testInstance = new Gp2Gpv2();
                    break;
                default:
                    testInstance = null;
                    break;
            }

            return testInstance;
        }

    }
}
