namespace Tingle.PeriodicTasks;

/// <summary>Contract for a period task.</summary>
public interface IPeriodicTask
{
    /// <summary>Execute the task.</summary>
    /// <param name="name">
    /// Name of the periodic task.
    /// This value is only different for tasks of the same type but multiple registrations.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    /// This method is invoked in a distributed lock context
    /// Implementations do not need to handle locking to ensure only one is called.
    /// </remarks>
    Task ExecuteAsync(string name, CancellationToken cancellationToken);
}
