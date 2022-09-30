namespace Tingle.PeriodicTasks;

/// <summary>
/// Contract to listening to period task events.
/// </summary>
public interface IPeriodicTaskEventSubscriber
{
    /// <summary>
    /// Method invoked on successful invocation of a period task.
    /// </summary>
    /// <param name="attempt"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task OnExecutedAsync(PeriodicTaskExecutionAttempt attempt, CancellationToken cancellationToken = default);
}
