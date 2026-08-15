using CourseLibrary.Api.Configuration.Exceptions.Middlewares;

namespace CourseLibrary.Api.Configuration.Exceptions;

public static class ExceptionHandlingExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}