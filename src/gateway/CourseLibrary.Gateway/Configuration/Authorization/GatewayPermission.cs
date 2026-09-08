namespace CourseLibrary.Gateway.Configuration.Authorization;

internal static class GatewayPermission
{
    public const string Course = "course";
    public const string Comment = "comment";
    public const string Discussion = "discussion";

    public const string Read = "read";
    public const string Write = "write";
    public const string Update = "update";
    public const string Delete = "delete";
    public const string All = "all";

    public const string ClaimType = "permission";
    public const string AdministratorRole = "Administrator";

    public static string For(string resource, string action)
        => $"{resource}.{action}";
}