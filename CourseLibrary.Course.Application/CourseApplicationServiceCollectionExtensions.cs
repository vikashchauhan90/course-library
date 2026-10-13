using CourseLibrary.Course.Application.Operations.Courses;
using CourseLibrary.Course.Application.Operations.Courses.Create;
using CourseLibrary.Course.Application.Operations.Courses.Delete;
using CourseLibrary.Course.Application.Operations.Courses.Get;
using CourseLibrary.Course.Application.Operations.Courses.Retire;
using CourseLibrary.Course.Application.Operations.Courses.Search;
using CourseLibrary.Course.Application.Operations.Courses.Update;
using CourseLibrary.Course.Application.Operations.Comments.Create;
using CourseLibrary.Course.Application.Operations.Comments.Delete;
using CourseLibrary.Course.Application.Operations.Comments.Get;
using CourseLibrary.Course.Application.Operations.Comments.GetByCourse;
using CourseLibrary.Course.Application.Operations.Comments.Update;
using CourseLibrary.Course.Application.Operations.Discussions.Create;
using CourseLibrary.Course.Application.Operations.Discussions.Delete;
using CourseLibrary.Course.Application.Operations.Discussions.Get;
using CourseLibrary.Course.Application.Operations.Discussions.GetByCourse;
using CourseLibrary.Course.Application.Operations.Discussions.Update;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Course.Application;

public static class CourseApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddCourseApplicationOperations(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IValidator<GetCourseRequest>, GetCourseValidator>();
        services.AddSingleton<IValidator<SearchCoursesRequest>, SearchCoursesValidator>();
        services.AddSingleton<IValidator<CreateCourseOperationRequest>, CreateCourseValidator>();
        services.AddSingleton<IValidator<UpdateCourseOperationRequest>, UpdateCourseValidator>();
        services.AddSingleton<IValidator<RetireCourseRequest>, RetireCourseValidator>();
        services.AddSingleton<IValidator<DeleteCourseRequest>, DeleteCourseValidator>();
        services.AddSingleton<IValidator<GetCommentRequest>, GetCommentValidator>();
        services.AddSingleton<IValidator<GetCommentsByCourseRequest>, GetCommentsByCourseValidator>();
        services.AddSingleton<IValidator<CreateCommentOperationRequest>, CreateCommentValidator>();
        services.AddSingleton<IValidator<UpdateCommentRequest>, UpdateCommentValidator>();
        services.AddSingleton<IValidator<DeleteCommentRequest>, DeleteCommentValidator>();
        services.AddSingleton<IValidator<GetDiscussionRequest>, GetDiscussionValidator>();
        services.AddSingleton<IValidator<GetDiscussionsByCourseRequest>, GetDiscussionsByCourseValidator>();
        services.AddSingleton<IValidator<CreateDiscussionRequest>, CreateDiscussionValidator>();
        services.AddSingleton<IValidator<UpdateDiscussionRequest>, UpdateDiscussionValidator>();
        services.AddSingleton<IValidator<DeleteDiscussionRequest>, DeleteDiscussionValidator>();
        services.AddScoped<CourseEventPublisher>();
        services.AddScoped<IGetCourseOperation, GetCourseOperation>();
        services.AddScoped<ISearchCoursesOperation, SearchCoursesOperation>();
        services.AddScoped<ICreateCourseOperation, CreateCourseOperation>();
        services.AddScoped<IUpdateCourseOperation, UpdateCourseOperation>();
        services.AddScoped<IRetireCourseOperation, RetireCourseOperation>();
        services.AddScoped<IDeleteCourseOperation, DeleteCourseOperation>();
        services.AddScoped<IGetCommentOperation, GetCommentOperation>();
        services.AddScoped<IGetCommentsByCourseOperation, GetCommentsByCourseOperation>();
        services.AddScoped<ICreateCommentOperation, CreateCommentOperation>();
        services.AddScoped<IUpdateCommentOperation, UpdateCommentOperation>();
        services.AddScoped<IDeleteCommentOperation, DeleteCommentOperation>();
        services.AddScoped<IGetDiscussionOperation, GetDiscussionOperation>();
        services.AddScoped<IGetDiscussionsByCourseOperation, GetDiscussionsByCourseOperation>();
        services.AddScoped<ICreateDiscussionOperation, CreateDiscussionOperation>();
        services.AddScoped<IUpdateDiscussionOperation, UpdateDiscussionOperation>();
        services.AddScoped<IDeleteDiscussionOperation, DeleteDiscussionOperation>();

        return services;
    }
}
