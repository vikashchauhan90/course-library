using CourseLibrary.Course.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.AddCourseLibraryApi();
var app = builder.Build();
app.UseCourseLibraryApi();
app.Run();
