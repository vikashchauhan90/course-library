namespace CourseLibrary.Domain;

public static class EventIds
{
    public static class Authors
    {
        public const int CreateAuthor = 1000;
        public const int GetAuthor = 1100;
        public const int GetAuthors = 1200;
        public const int UpdateAuthor = 1300;
        public const int DeleteAuthor = 1400;
    }

    public static class Courses
    {
        public const int CreateCourse = 2000;
        public const int GetCourse = 2100;
        public const int GetCourses = 2200;
        public const int UpdateCourse = 2300;
        public const int DeleteCourse = 2400;
    }

    public static class Infrastructure
    {
        public const int Cosmos = 4000;
        public const int Cache = 4500;
        public const int Messaging = 5000;
    }

    public static class Api
    {
        public const int Requests = 8000;
        public const int Exceptions = 8100;
    }
}