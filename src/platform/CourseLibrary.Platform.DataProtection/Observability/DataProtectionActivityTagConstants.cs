namespace CourseLibrary.Platform.DataProtection.Observability;

internal static class DataProtectionActivityTagConstants
{
    public const string Operation = "dataprotection.operation";
    public const string PayloadType = "dataprotection.payload_type";
    public const string OperationSuccess = "dataprotection.operation.success";
    public const string OperationError = "dataprotection.operation.error";

    public const string ProtectOperation = "protect";
    public const string UnprotectOperation = "unprotect";
    public const string StringPayload = "string";
    public const string ByteArrayPayload = "byte-array";
}