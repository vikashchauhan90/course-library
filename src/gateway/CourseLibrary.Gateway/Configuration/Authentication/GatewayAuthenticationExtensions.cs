using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace CourseLibrary.Gateway.Configuration.Authentication;

internal static class GatewayAuthenticationConstants
{
    public const string JwtScheme = JwtBearerDefaults.AuthenticationScheme;
}

internal static class GatewayAuthenticationExtensions
{
    public static WebApplicationBuilder AddGatewayAuthentication(
        this WebApplicationBuilder builder)
    {
        var jwt = builder.Configuration
            .GetSection("Authentication:Jwt")
            .Get<GatewayJwtOptions>()
            ?? throw new InvalidOperationException(
                "Authentication:Jwt configuration is missing.");

        builder.Services.AddScoped<GatewayJwtBearerEvents>();

        builder.Services.AddSingleton<ITokenIdentityService,
            TokenIdentityService>();


        builder.Services.AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = jwt.Authority;
                if (!string.IsNullOrWhiteSpace(jwt.MetadataAddress)) { options.MetadataAddress = jwt.MetadataAddress; }
                options.Audience = jwt.Audience;

                options.RequireHttpsMetadata = true;
                options.RefreshOnIssuerKeyNotFound = true;
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew =TimeSpan.Zero
                    };

                options.EventsType =
                    typeof(GatewayJwtBearerEvents);
            });

        return builder;
    }
}
