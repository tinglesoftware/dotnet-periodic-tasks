namespace Tingle.PeriodicTasks;

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
    public string? Id { get; internal set; }

    ///
    public string? Name { get; internal set; }

    ///
    public Type? TaskType { get; init; }
}
