using Microsoft.Extensions.Compliance.Classification;

namespace CourseLibrary.Platform.Observability.Logs.Redaction;

public static class DataClassifications
{
    public static readonly DataClassification Secret =
        new(
            DataClassificationCodes.SecurityCategory,
            DataClassificationCodes.Secret);


    public static readonly DataClassification Email =
        new(
            DataClassificationCodes.PersonalCategory,
            DataClassificationCodes.Email);


    public static readonly DataClassification Phone =
        new(
            DataClassificationCodes.PersonalCategory,
            DataClassificationCodes.Phone);


    public static readonly DataClassification CreditCard =
        new(
            DataClassificationCodes.FinancialCategory,
            DataClassificationCodes.CreditCard);


    public static readonly DataClassification PersonalData =
        new(
            DataClassificationCodes.PersonalCategory,
            DataClassificationCodes.PersonalData);


    public static readonly DataClassification Sensitive =
      new(
                    DataClassificationCodes.SecurityCategory,
                    DataClassificationCodes.Sensitive);

}