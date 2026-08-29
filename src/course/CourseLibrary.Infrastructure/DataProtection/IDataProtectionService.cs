namespace CourseLibrary.Infrastructure.DataProtection;

public interface IDataProtectionService
{
    string Protect(string value);

    string Unprotect(string protectedValue);

    byte[] Protect(byte[] value);

    byte[] Unprotect(byte[] protectedValue);
}
