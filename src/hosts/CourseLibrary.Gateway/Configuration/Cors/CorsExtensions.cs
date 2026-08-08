using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Gateway.Configuration.Cors;

internal static class GatewayCorsConstants
{
    public const string PolicyName = "GatewayCors";
}

internal static class GatewayCorsExtensions
{
    public static WebApplicationBuilder AddGatewayCors(
        this WebApplicationBuilder builder)
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(GatewayCorsConstants.PolicyName, policy =>
            {
                if (allowedOrigins is { Length: > 0 })
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
                else
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        return builder;
    }
}
