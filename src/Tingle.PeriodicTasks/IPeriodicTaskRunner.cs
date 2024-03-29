﻿using System.Diagnostics.CodeAnalysis;
using Tingle.PeriodicTasks.Internal;

namespace Tingle.PeriodicTasks;

/// <summary>A periodic task runner.</summary>
public interface IPeriodicTaskRunner
{
    /// <summary>Run the execution continuously based on the provided settings.</summary>
    /// <param name="name">Name of the task.</param>
    /// <param name="cancellationToken">A token to cancel execution.</param>
    /// <returns></returns>
    Task RunAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Execute the task once.</summary>
    /// <param name="name">Name of the task.</param>
    /// <param name="cancellationToken">A token to cancel execution.</param>
    /// <returns></returns>
    /// <remarks>
    /// Use this method if your application needs to execute the task
    /// before the next schedule is reached or if the task is disabled.
    /// </remarks>
    Task<PeriodicTaskExecutionAttempt?> ExecuteAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Execute the task once.</summary>
    /// <param name="name">Name of the task.</param>
    /// <param name="throwOnError">Whether to throw an exception on failure.</param>
    /// <param name="awaitExecution">
    /// Gets or sets whether the task execution should be awaited.
    /// This overrides the value in <see cref="PeriodicTaskOptions.AwaitExecution"/>.
    /// </param>
    /// <param name="cancellationToken">A token to cancel execution.</param>
    /// <returns></returns>
    /// <remarks>
    /// Use this method if your application needs to execute the task
    /// before the next schedule is reached or if the task is disabled.
    /// </remarks>
    Task<PeriodicTaskExecutionAttempt?> ExecuteAsync(string name, bool throwOnError, bool? awaitExecution, CancellationToken cancellationToken = default);
}

/// <summary>A periodic task runner tasks of <typeparamref name="TTask"/> type.</summary>
/// <typeparam name="TTask">The type of task to be executed.</typeparam>
public interface IPeriodicTaskRunner<[DynamicallyAccessedMembers(TrimmingHelper.Task)] TTask> : IPeriodicTaskRunner
{

}
