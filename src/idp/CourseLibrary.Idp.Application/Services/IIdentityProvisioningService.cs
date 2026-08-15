namespace CourseLibrary.Idp.Application.Services;

public interface IIdentityProvisioningService
{
    Task EnsureSeedDataAsync(CancellationToken cancellationToken = default);
}
