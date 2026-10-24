using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using CourseLibrary.Course.Application.UseCases.Courses;
using CourseLibrary.Course.Application.UseCases.Discussions.Create;
using CourseLibrary.Course.Application.UseCases.Discussions.Delete;
using CourseLibrary.Course.Application.UseCases.Discussions.Get;
using CourseLibrary.Course.Application.UseCases.Discussions.GetByCourse;
using CourseLibrary.Course.Application.UseCases.Discussions.Update;
using CourseLibrary.Course.Application.UseCases.Courses.Create;
using CourseLibrary.Course.Application.UseCases.Courses.Delete;
using CourseLibrary.Course.Application.UseCases.Courses.Get;
using CourseLibrary.Course.Application.UseCases.Courses.Retire;
using CourseLibrary.Course.Application.UseCases.Courses.Search;
using CourseLibrary.Course.Application.UseCases.Courses.Update;
using CourseLibrary.Course.Application.UseCases.Comments.Create;
using CourseLibrary.Course.Application.UseCases.Comments.Delete;
using CourseLibrary.Course.Application.UseCases.Comments.Get;
using CourseLibrary.Course.Application.UseCases.Comments.GetByCourse;
using CourseLibrary.Course.Application.UseCases.Comments.Update;

namespace CourseLibrary.Course.Application;

public static class CourseApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddCourseApplicationUseCases(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddValidatorsFromAssemblyContaining<GetCourseValidator>(
            ServiceLifetime.Singleton,
            includeInternalTypes: true);
        services.AddScoped<ICourseEventPublisher, CourseEventPublisher>();
        services.AddScoped<IGetCourseUseCase, GetCourseUseCase>();
        services.AddScoped<ISearchCoursesUseCase, SearchCoursesUseCase>();
        services.AddScoped<ICreateCourseUseCase, CreateCourseUseCase>();
        services.AddScoped<IUpdateCourseUseCase, UpdateCourseUseCase>();
        services.AddScoped<IRetireCourseUseCase, RetireCourseUseCase>();
        services.AddScoped<IDeleteCourseUseCase, DeleteCourseUseCase>();
        services.AddScoped<IGetCommentUseCase, GetCommentUseCase>();
        services.AddScoped<IGetCommentsByCourseUseCase, GetCommentsByCourseUseCase>();
        services.AddScoped<ICreateCommentUseCase, CreateCommentUseCase>();
        services.AddScoped<IUpdateCommentUseCase, UpdateCommentUseCase>();
        services.AddScoped<IDeleteCommentUseCase, DeleteCommentUseCase>();
        services.AddScoped<IGetDiscussionUseCase, GetDiscussionUseCase>();
        services.AddScoped<IGetDiscussionsByCourseUseCase, GetDiscussionsByCourseUseCase>();
        services.AddScoped<ICreateDiscussionUseCase, CreateDiscussionUseCase>();
        services.AddScoped<IUpdateDiscussionUseCase, UpdateDiscussionUseCase>();
        services.AddScoped<IDeleteDiscussionUseCase, DeleteDiscussionUseCase>();

        return services;
    }
}
