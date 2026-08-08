using Microsoft.AspNetCore.Authorization;

namespace CourseLibrary.Gateway.Configuration.Authorization;


internal sealed class UserTokenRequirement
    : IAuthorizationRequirement
{
}