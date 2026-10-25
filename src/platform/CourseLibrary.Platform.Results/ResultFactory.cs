namespace CourseLibrary.Platform.Results;

public static class ResultFactory
{
    public static Result Ok()
        => Result.Success();

    public static Result<T> Ok<T>(T value)
        => Result<T>.Success(value);

    public static Result Fail(Error error)
        => Result.Failure(error);

    public static Result Fail()
      => Result.Failure();

    public static Result<T> Fail<T>(Error error)
        => Result<T>.Failure(error);

    public static Result<T> Fail<T>()
       => Result<T>.Failure();
}
