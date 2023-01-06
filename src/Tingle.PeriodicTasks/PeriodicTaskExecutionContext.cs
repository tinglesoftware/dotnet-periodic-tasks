namespace Tingle.PeriodicTasks;

/// <summary>Represents the context for the current execution of a period task.</summary>
public class PeriodicTaskExecutionContext
{
    ///
    internal PeriodicTaskExecutionContext(string name, string executionId)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ExecutionId = executionId ?? throw new ArgumentNullException(nameof(executionId));
    }

    /// <summary>
    /// Name of the periodic task.
    /// This value is different for tasks of the same type but multiple registrations.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Unique identifier of the execution.
    /// </summary>
    public string ExecutionId { get; }

    /// <summary>Periodic task type.</summary>
    public required Type TaskType { get; init; }
}
