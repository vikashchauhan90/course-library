namespace CourseLibrary.Gateway.Configuration.Exceptions;

public static class ExceptionHandlingExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<GatewayExceptionHandlerMiddleware>();
    }
}
