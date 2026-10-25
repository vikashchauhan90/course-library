namespace CourseLibrary.Platform.Results;

public sealed class Result<T> : Result
{
    private Result(
        T? value,
        bool isSuccess,
        Error? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value)
        => new(
            value,
            true,
            null);

    public new static Result<T> Failure(Error error)
        => new(
            default,
            false,
            error);

    public new static Result<T> Failure()
        => new(
            default,
            false,
            null);
}