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
                options.MapInboundClaims = false;
                if (!string.IsNullOrWhiteSpace(jwt.MetadataAddress)) { options.MetadataAddress = jwt.MetadataAddress; }
                options.Audience = jwt.Audience;

                options.RequireHttpsMetadata = jwt.RequireHttpsMetadata;
                options.RefreshOnIssuerKeyNotFound = true;
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidIssuer = jwt.Authority,
                        ValidAudience = jwt.Audience,
                        ValidateIssuer = jwt.ValidateIssuer,
                        ValidateAudience = jwt.ValidateAudience,
                        ValidateLifetime = jwt.ValidateLifetime,
                        ValidateIssuerSigningKey = jwt.ValidateIssuerSigningKey,
                        ClockSkew =TimeSpan.Zero
                    };

                options.EventsType =
                    typeof(GatewayJwtBearerEvents);
            });

        return builder;
    }
}
