
namespace CourseLibrary.Platform.Observability.Logs.Redaction;

public static class DataClassificationCodes
{
	public const string SecurityCategory = "Security";
	public const string Secret = "Secret";
	public const string Sensitive = "Sensitive";

	public const string PersonalCategory = "Personal";
	public const string Email = "Email";
	public const string Phone = "Phone";
	public const string PersonalData = "General";

	public const string FinancialCategory = "Financial";
	public const string CreditCard = "CreditCard";
}
