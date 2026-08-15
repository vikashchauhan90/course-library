using CourseLibrary.Gateway.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddCourseLibraryServices();

var app = builder.Build();

app.UseCourseLibraryPipeline();

app.Run();