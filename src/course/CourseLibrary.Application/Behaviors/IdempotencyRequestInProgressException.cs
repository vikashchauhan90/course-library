namespace CourseLibrary.Application.Behaviors
{
    [Serializable]
    internal class IdempotencyRequestInProgressException : Exception
    {
        public IdempotencyRequestInProgressException()
        {
        }

        public IdempotencyRequestInProgressException(string? message) : base(message)
        {
        }

        public IdempotencyRequestInProgressException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}