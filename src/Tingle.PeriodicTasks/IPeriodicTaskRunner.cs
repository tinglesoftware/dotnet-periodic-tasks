namespace Tingle.PeriodicTasks;

/// <summary>A periodic task runner.</summary>
public interface IPeriodicTaskRunner
{
    /// <summary>Run the execution continuously based on the provided settings.</summary>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RunAsync(string name, CancellationToken cancellationToken);

    /// <summary>Execute the task once.</summary>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <remarks>
    /// Use this method if your application needs to execute the task
    /// before the next schedule is reached or it the task is disabled.
    /// </remarks>
    Task ExecuteAsync(string name, CancellationToken cancellationToken);
}

/// <summary>A periodic task runner tasks of <typeparamref name="TTask"/> type.</summary>
/// <typeparam name="TTask">The type of task to be executed.</typeparam>
public interface IPeriodicTaskRunner<TTask> : IPeriodicTaskRunner
{

}
