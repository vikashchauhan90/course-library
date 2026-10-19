namespace CourseLibrary.Course.Contracts.DTOs;

public sealed record UpdateCourseRequest(
   string Title,
   string Description);