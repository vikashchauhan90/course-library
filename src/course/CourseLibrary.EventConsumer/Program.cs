using CourseLibrary.EventConsumer.Configuration.Observability;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using CourseLibrary.Infrastructure.Configuration;

var builder = FunctionsApplication.CreateBuilder(args);

// W3C distributed tracing format.
Activity.DefaultIdFormat = ActivityIdFormat.W3C;
Activity.ForceDefaultIdFormat = true;

builder.Services.AddOptions();
builder.Services.AddHttpClient();

// Infrastructure services.
builder.Services.AddCourseLibraryInfrastructure(builder.Configuration);

builder.AddObservability();
var host = builder.Build();
