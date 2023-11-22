namespace Tingle.PeriodicTasks;

/// <summary>A store for periodic task attempt</summary>
public interface IPeriodicTaskExecutionAttemptsStore
{
    /// <summary>Get all attempts for a task.</summary>
    /// <param name="count">The number of attempts to return.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyList<PeriodicTaskExecutionAttempt>> GetAttemptsAsync(int? count = null, CancellationToken cancellationToken = default);

    /// <summary>Get all successful attempts for a task.</summary>
    /// <param name="cancellationToken"></param>
    /// <param name="count">The number of attempts to return.</param>
    /// <returns></returns>
    Task<IReadOnlyList<PeriodicTaskExecutionAttempt>> GetSuccessfulAttemptsAsync(int? count = null, CancellationToken cancellationToken = default);

    /// <summary>Get all attempts for a task.</summary>
    /// <param name="name">Name of the task.</param>
    /// <param name="count">The number of attempts to return.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyList<PeriodicTaskExecutionAttempt>> GetAttemptsAsync(string name, int? count = null, CancellationToken cancellationToken = default);

    /// <summary>Get all successful attempts for a task.</summary>
    /// <param name="name">Name of the task.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="count">The number of attempts to return.</param>
    /// <returns></returns>
    Task<IReadOnlyList<PeriodicTaskExecutionAttempt>> GetSuccessfulAttemptsAsync(string name, int? count = null, CancellationToken cancellationToken = default);

    /// <summary>Get the last attempt for a task.</summary>
    /// <param name="name">Name of the task.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PeriodicTaskExecutionAttempt?> GetLastAttemptAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Get the last attempt for a task.</summary>
    /// <param name="name">Name of the task.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PeriodicTaskExecutionAttempt?> GetLastSuccessfulAttemptAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Add an attempt to the store.</summary>
    /// <param name="attempt">The attempt to add.</param>
    /// <param name="cancellationToken"></param>
    Task AddAsync(PeriodicTaskExecutionAttempt attempt, CancellationToken cancellationToken = default);
}
