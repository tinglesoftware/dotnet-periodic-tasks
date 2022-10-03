namespace Tingle.PeriodicTasks.Internal;

///
public class PeriodicTaskException : Exception
{
    ///
    public PeriodicTaskException(string message) : base(message)
    {
    }

    ///
    public PeriodicTaskException(string message, Exception innerException) : base(message, innerException)
    {
    }

    ///
    public Type? TaskType { get; init; }

    ///
    public PeriodicTaskExecutionAttempt? Attempt { get; init; }
}
