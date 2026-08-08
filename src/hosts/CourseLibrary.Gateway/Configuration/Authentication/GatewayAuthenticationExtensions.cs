using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CourseLibrary.Gateway.Configuration.Authentication;

internal static class GatewayAuthenticationConstants
{
    public const string JwtScheme = JwtBearerDefaults.AuthenticationScheme;
    public const string M2MScheme = "M2M";
}

internal static class GatewayAuthenticationExtensions
{
    public static WebApplicationBuilder AddGatewayAuthentication(
        this WebApplicationBuilder builder)
    {
        var authSection = builder.Configuration.GetSection("Authentication");
        var jwtSection = authSection.GetSection("Jwt");
        var m2mSection = authSection.GetSection("M2M");

        var jwtAuthority = jwtSection["Authority"];
        var jwtAudience = jwtSection["Audience"];
        var m2mAuthority = m2mSection["Authority"];
        var m2mAudience = m2mSection["Audience"];

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = GatewayAuthenticationConstants.JwtScheme;
            options.DefaultChallengeScheme = GatewayAuthenticationConstants.JwtScheme;
        })
        .AddJwtBearer(GatewayAuthenticationConstants.JwtScheme, options =>
        {
            options.Authority = jwtAuthority;
            options.Audience = jwtAudience;
            options.RequireHttpsMetadata = !string.IsNullOrWhiteSpace(jwtAuthority) &&
                                           jwtAuthority.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = !string.IsNullOrWhiteSpace(jwtAuthority),
                ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
                RoleClaimType = "roles"
            };
        })
        .AddJwtBearer(GatewayAuthenticationConstants.M2MScheme, options =>
        {
            options.Authority = m2mAuthority;
            options.Audience = m2mAudience;
            options.RequireHttpsMetadata = !string.IsNullOrWhiteSpace(m2mAuthority) &&
                                           m2mAuthority.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = !string.IsNullOrWhiteSpace(m2mAuthority),
                ValidateAudience = !string.IsNullOrWhiteSpace(m2mAudience),
                RoleClaimType = "roles"
            };
        });

        return builder;
    }
}
