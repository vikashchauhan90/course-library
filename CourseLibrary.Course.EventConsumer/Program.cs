using CourseLibrary.Course.EventConsumer.Observability;
using CourseLibrary.Course.EventConsumer.Observability.Metrics;
using CourseLibrary.Course.Persistence;
using CourseLibrary.Platform.Messaging;
using CourseLibrary.Platform.Serialization;
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

builder.Services.AddCourseLibrarySerializers();
builder.Services.AddCourseLibraryServiceBus(builder.Configuration);
builder.Services.AddCosmosDatabase(builder.Configuration);
builder.Services.AddRepositories();

builder.Build().Run();
