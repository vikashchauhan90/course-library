using CourseLibrary.EventConsumer.Configuration;
using CourseLibrary.EventConsumer.Configuration.Observability;
using CourseLibrary.EventConsumer.Configuration.Observability.Metrics.Middlewares;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

var builder = FunctionsApplication.CreateBuilder(args);

// W3C distributed tracing format.
Activity.DefaultIdFormat = ActivityIdFormat.W3C;
Activity.ForceDefaultIdFormat = true;

builder.Services.AddOptions();
builder.Services.AddHttpClient();

// Observability.
builder.AddObservability();
builder.UseMiddleware<FunctionMetricsMiddleware>();

// Infrastructure services.
builder.Services.AddCourseLibraryInfrastructure(builder.Configuration);

// Application services
builder.Services.AddCourseLibraryApplication();

var host = builder.Build();

host.Run();