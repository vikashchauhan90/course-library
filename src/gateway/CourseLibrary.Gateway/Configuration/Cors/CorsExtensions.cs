namespace CourseLibrary.Gateway.Configuration.Cors;

internal static class GatewayCorsConfiguration
{
    public static WebApplicationBuilder AddGatewayCors(
    this WebApplicationBuilder builder)
    {
        var corsConfig = builder.Configuration.GetSection("Cors");

        builder.Services.AddCors(options =>
        {
            var policies = corsConfig.GetSection("Policies");

            foreach (var policySection in policies.GetChildren())
            {
                var policyName = policySection.Key;

                var allowedOrigins =
                    policySection.GetSection("AllowedOrigins")
                        .Get<string[]>() ?? Array.Empty<string>();

                var allowedMethods =
                    policySection.GetSection("AllowedMethods")
                        .Get<string[]>() ?? Array.Empty<string>();

                var allowedHeaders =
                    policySection.GetSection("AllowedHeaders")
                        .Get<string[]>() ?? Array.Empty<string>();

                var exposedHeaders =
                    policySection.GetSection("ExposedHeaders")
                        .Get<string[]>();

                var allowCredentials =
                    policySection.GetValue<bool>("AllowCredentials");

                var maxAge =
                    policySection.GetValue<int?>("MaxAge");

                options.AddPolicy(policyName, policy =>
                {
                    if (allowedOrigins.Contains("*"))
                    {
                        policy.AllowAnyOrigin();
                    }
                    else if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins);
                    }

                    if (allowedMethods.Contains("*"))
                    {
                        policy.AllowAnyMethod();
                    }
                    else if (allowedMethods.Length > 0)
                    {
                        policy.WithMethods(allowedMethods);
                    }

                    if (allowedHeaders.Contains("*"))
                    {
                        policy.AllowAnyHeader();
                    }
                    else if (allowedHeaders.Length > 0)
                    {
                        policy.WithHeaders(allowedHeaders);
                    }

                    if (exposedHeaders?.Length > 0)
                    {
                        policy.WithExposedHeaders(exposedHeaders);
                    }

                    if (allowCredentials)
                    {
                        policy.AllowCredentials();
                    }

                    if (maxAge.HasValue)
                    {
                        policy.SetPreflightMaxAge(
                            TimeSpan.FromSeconds(maxAge.Value));
                    }
                });
            }

            var defaultPolicyName =
                corsConfig.GetValue<string>("DefaultPolicy");

            if (!string.IsNullOrWhiteSpace(defaultPolicyName))
            {
                options.DefaultPolicyName = defaultPolicyName;
            }
        });

        return builder;
    }
}