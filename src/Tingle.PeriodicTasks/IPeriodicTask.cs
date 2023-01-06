namespace Tingle.PeriodicTasks;

/// <summary>Contract for a period task.</summary>
public interface IPeriodicTask
{
    /// <summary>Execute the task.</summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    /// This method is invoked in a distributed lock context
    /// Implementations do not need to handle locking to ensure only one is called.
    /// </remarks>
    Task ExecuteAsync(PeriodicTaskExecutionContext context, CancellationToken cancellationToken);
}
