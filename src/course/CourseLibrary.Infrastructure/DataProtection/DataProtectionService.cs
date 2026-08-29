using Microsoft.AspNetCore.DataProtection;

namespace CourseLibrary.Infrastructure.DataProtection;

internal sealed class DataProtectionService(
    IDataProtectionProvider dataProtectionProvider)
    : IDataProtectionService
{
    private const string Purpose = "CourseLibrary.DataProtection";

    private readonly IDataProtector _protector =
        dataProtectionProvider.CreateProtector(Purpose);

    public string Protect(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return _protector.Protect(value);
    }

    public string Unprotect(string protectedValue)
    {
        ArgumentNullException.ThrowIfNull(protectedValue);

        return _protector.Unprotect(protectedValue);
    }

    public byte[] Protect(byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return _protector.Protect(value);
    }

    public byte[] Unprotect(byte[] protectedValue)
    {
        ArgumentNullException.ThrowIfNull(protectedValue);

        return _protector.Unprotect(protectedValue);
    }
}