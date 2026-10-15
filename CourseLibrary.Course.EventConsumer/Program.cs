using CourseLibrary.Course.EventConsumer.Configuration;
using CourseLibrary.Course.EventConsumer.Configuration.Observability;
using CourseLibrary.Course.EventConsumer.Configuration.Observability.Metrics;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

var builder = FunctionsApplication.CreateBuilder(args);

Activity.DefaultIdFormat = ActivityIdFormat.W3C;
Activity.ForceDefaultIdFormat = true;

builder.Services.AddOptions();
builder.Services.AddHttpClient();

builder.AddCourseLibraryEventConsumerObservability();
builder.UseMiddleware<EventConsumerFunctionMetricsMiddleware>();

builder.Services.AddCourseLibraryEventConsumer(builder.Configuration);

builder.Build().Run();
