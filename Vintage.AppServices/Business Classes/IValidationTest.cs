namespace Vintage.AppServices.BusinessClasses
{
    using System.Collections.Generic;

    /// <summary>
    /// Validation Test Abstract Factory Interface
    /// </summary>
    public interface IValidationTest
    {
        SpecificationType specificationType { get; set; }
        string specificationTypeVersion { get; set; }
        FormatType transportFormatType { get; set; }
        string transportFormatVersion { get; set; }
        FormatType payloadFormatType { get; set; }
        string payloadFormatVersion { get; set; }
        string testFileExtension { get; set; }
        string testFileName { get; set; }
        string documentTemplate { get; set; }

        List<string> ValidateTestInstance(string testFileName, string encryptionKey);
    }
}
