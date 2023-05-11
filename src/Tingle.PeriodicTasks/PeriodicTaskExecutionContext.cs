namespace Tingle.PeriodicTasks;

/// <summary>Represents the context for the current execution of a period task.</summary>
public class PeriodicTaskExecutionContext
{
    /// <summary>
    /// Creates and instance of <see cref="PeriodicTaskExecutionContext"/>.
    /// </summary>
    /// <param name="name">
    /// Name of the periodic task.
    /// This value is different for tasks of the same type but multiple registrations.
    /// </param>
    /// <param name="executionId">Unique identifier of the execution.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public PeriodicTaskExecutionContext(string name, string executionId)
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
    public Type? TaskType { get; init; }
}
