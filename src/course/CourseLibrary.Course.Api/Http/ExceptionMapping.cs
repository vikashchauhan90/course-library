namespace CourseLibrary.Course.Api.Http;

public sealed record ExceptionMapping(
         int StatusCode,
         string Title,
         string Detail,
         string Type,
         object? Errors = null);