using CourseLibrary.Client.Courses;
using CourseLibrary.Client.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using System.Net;

namespace CourseLibrary.Client.Configuration;

public static class CourseLibraryClientExtensions
{
    public static IServiceCollection AddCourseLibraryClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
           .AddOptions<CourseLibraryClientOptions>()
           .Bind(configuration.GetSection(CourseLibraryClientOptions.SectionName))
           .ValidateOnStart();

        services.ConfigureHttpClientDefaults(
             builder =>
             {
                 builder.AddStandardResilienceHandler(options =>
                 {
                     options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
                     options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
                     options.Retry.MaxRetryAttempts = 3;
                     options.Retry.ShouldRetryAfterHeader = true;
                     options.Retry.Delay = TimeSpan.FromSeconds(1);
                     options.Retry.BackoffType = DelayBackoffType.Exponential;
                     options.Retry.UseJitter = true;

                     options.Retry.ShouldHandle = static args =>
                     {
                         if (args.Outcome.Exception is HttpRequestException)
                         {
                             return new ValueTask<bool>(true);
                         }

                         if (args.Outcome.Result is HttpResponseMessage response)
                         {
                             return new ValueTask<bool>(
                                  response.StatusCode == HttpStatusCode.RequestTimeout ||
                                  response.StatusCode == HttpStatusCode.TooManyRequests ||
                                  (int)response.StatusCode >= (int)HttpStatusCode.InternalServerError);
                         }
                         return new ValueTask<bool>(false);
                     };

                     options.CircuitBreaker.FailureRatio = 0.20;
                     options.CircuitBreaker.MinimumThroughput = 20;
                     options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
                     options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(10);

                     options.CircuitBreaker.ShouldHandle = static args =>
                     {
                         if (args.Outcome.Exception is HttpRequestException)
                         {
                             return new ValueTask<bool>(true);
                         }

                         if (args.Outcome.Result is HttpResponseMessage response)
                         {
                             return new ValueTask<bool>(
                                  response.StatusCode == HttpStatusCode.RequestTimeout ||
                                  (int)response.StatusCode >= (int)HttpStatusCode.InternalServerError);
                         }
                         return new ValueTask<bool>(false);
                     };
                 });
             });

        services.AddTransient<AuthorizationDelegatingHandler>();
        services.AddTransient<CommonHeadersDelegatingHandler>();
        services.AddHttpClient<ICourseApiClient, CourseApiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<CourseLibraryClientOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        }).AddHttpMessageHandler<CommonHeadersDelegatingHandler>()
        .AddHttpMessageHandler<AuthorizationDelegatingHandler>();

        return services;
    }
}
