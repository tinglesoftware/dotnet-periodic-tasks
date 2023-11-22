namespace Tingle.PeriodicTasks.Stores;

/// <summary>
/// An implementation of <see cref="IPeriodicTaskExecutionAttemptsStore"/> that does not perform any operations.
/// This is useful for scenarios where you have many executions and do not need to store any attempts, not even in memory.
/// </summary>
public class NoOpPeriodicTaskExecutionAttemptsStore : IPeriodicTaskExecutionAttemptsStore
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<PeriodicTaskExecutionAttempt>> GetAttemptsAsync(int? count = null, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<PeriodicTaskExecutionAttempt>>([]);

    /// <inheritdoc/>
    public Task<IReadOnlyList<PeriodicTaskExecutionAttempt>> GetSuccessfulAttemptsAsync(int? count = null, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<PeriodicTaskExecutionAttempt>>([]);

    /// <inheritdoc/>
    public Task<IReadOnlyList<PeriodicTaskExecutionAttempt>> GetAttemptsAsync(string name, int? count = null, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<PeriodicTaskExecutionAttempt>>([]);

    /// <inheritdoc/>
    public Task<IReadOnlyList<PeriodicTaskExecutionAttempt>> GetSuccessfulAttemptsAsync(string name, int? count = null, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<PeriodicTaskExecutionAttempt>>([]);

    /// <inheritdoc/>
    public Task<PeriodicTaskExecutionAttempt?> GetLastAttemptAsync(string name, CancellationToken cancellationToken = default)
        => Task.FromResult<PeriodicTaskExecutionAttempt?>(default);

    /// <inheritdoc/>
    public Task<PeriodicTaskExecutionAttempt?> GetLastSuccessfulAttemptAsync(string name, CancellationToken cancellationToken = default)
        => Task.FromResult<PeriodicTaskExecutionAttempt?>(default);

    /// <inheritdoc/>
    public Task AddAsync(PeriodicTaskExecutionAttempt attempt, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
