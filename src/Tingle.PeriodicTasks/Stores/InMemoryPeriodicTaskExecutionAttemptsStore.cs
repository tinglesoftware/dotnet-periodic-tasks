using System.Collections.Concurrent;

namespace Tingle.PeriodicTasks.Stores;

/// <summary>
/// An implementation of <see cref="IPeriodicTaskExecutionAttemptsStore"/> that stores attempts in memory.
/// This is backed by <see cref="ConcurrentBag{T}"/>.
/// </summary>
public class InMemoryPeriodicTaskExecutionAttemptsStore : IPeriodicTaskExecutionAttemptsStore
{
    private readonly ConcurrentBag<PeriodicTaskExecutionAttempt> attempts = [];

    /// <inheritdoc/>
    public Task<IReadOnlyList<PeriodicTaskExecutionAttempt>> GetAttemptsAsync(int? count = null, CancellationToken cancellationToken = default)
    {
        var attempts = this.attempts.ToList();
        if (count is int limit) attempts = attempts.Take(limit).ToList();
        return Task.FromResult<IReadOnlyList<PeriodicTaskExecutionAttempt>>(attempts);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<PeriodicTaskExecutionAttempt>> GetSuccessfulAttemptsAsync(int? count = null, CancellationToken cancellationToken = default)
    {
        var attempts = this.attempts.ToList().Where(a => a.Successful);
        if (count is int limit) attempts = attempts.Take(limit);
        return Task.FromResult<IReadOnlyList<PeriodicTaskExecutionAttempt>>(attempts.ToList());
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<PeriodicTaskExecutionAttempt>> GetAttemptsAsync(string name, int? count = null, CancellationToken cancellationToken = default)
    {
        var attempts = this.attempts.ToList().Where(a => a.Name == name);
        if (count is int limit) attempts = attempts.Take(limit);
        return Task.FromResult<IReadOnlyList<PeriodicTaskExecutionAttempt>>(attempts.ToList());
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<PeriodicTaskExecutionAttempt>> GetSuccessfulAttemptsAsync(string name, int? count = null, CancellationToken cancellationToken = default)
    {
        var attempts = this.attempts.ToList().Where(a => a.Name == name && a.Successful);
        if (count is int limit) attempts = attempts.Take(limit);
        return Task.FromResult<IReadOnlyList<PeriodicTaskExecutionAttempt>>(attempts.ToList());
    }

    /// <inheritdoc/>
    public Task<PeriodicTaskExecutionAttempt?> GetLastAttemptAsync(string name, CancellationToken cancellationToken = default)
        => Task.FromResult(attempts.ToList().Where(a => a.Name == name).OrderByDescending(a => a.Start).FirstOrDefault());

    /// <inheritdoc/>
    public Task<PeriodicTaskExecutionAttempt?> GetLastSuccessfulAttemptAsync(string name, CancellationToken cancellationToken = default)
        => Task.FromResult(attempts.ToList().Where(a => a.Name == name && a.Successful).OrderByDescending(a => a.Start).FirstOrDefault());

    /// <inheritdoc/>
    public Task AddAsync(PeriodicTaskExecutionAttempt attempt, CancellationToken cancellationToken = default)
    {
        attempts.Add(attempt);
        return Task.CompletedTask;
    }
}
