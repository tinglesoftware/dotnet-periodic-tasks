﻿using Medallion.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tingle.PeriodicTasks.Diagnostics;

namespace Tingle.PeriodicTasks.Internal;

internal class PeriodicTaskRunner<[DynamicallyAccessedMembers(TrimmingHelper.Task)] TTask>(IServiceProvider serviceProvider,
                                                                                           IHostEnvironment environment,
                                                                                           IPeriodicTaskIdGenerator idGenerator,
                                                                                           IOptionsMonitor<PeriodicTaskOptions> optionsMonitor,
                                                                                           IDistributedLockProvider lockProvider,
                                                                                           IEnumerable<IPeriodicTaskEventSubscriber> subscribers,
                                                                                           ILogger<PeriodicTaskRunner<TTask>> logger) : IPeriodicTaskRunner<TTask>
    where TTask : class, IPeriodicTask
{
    private readonly IList<IPeriodicTaskEventSubscriber> subscribers = subscribers?.ToList() ?? throw new ArgumentNullException(nameof(subscribers));

    public async Task RunAsync(string name, CancellationToken cancellationToken = default)
    {
        var options = optionsMonitor.Get(name);

        if (!options.Enable)
        {
            logger.TaskNotEnabled(name);
            return;
        }

        if (options.ExecuteOnStartup)
        {
            // execute the task
            await ExecuteAsync(name, cancellationToken).ConfigureAwait(false);
        }

        var schedule = options.Schedule!.Value;
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(options.Timezone!);
        while (!cancellationToken.IsCancellationRequested)
        {
            var next = schedule.GetNextOccurrence(DateTimeOffset.UtcNow, timezone);
            if (next is null)
            {
                logger.UnableToGetNextOccurrence(expression: schedule, timezoneId: timezone.Id, taskName: name);
                break;
            }

            // calculate the delay to apply
            var delay = next.Value - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                logger.DelayingToNextOccurrence(delay.ToReadableString(), delay, next.Value, name);
                await DelayUtil.DelayAsync(delay, cancellationToken).ConfigureAwait(false);
            }

            // execute the task
            await ExecuteAsync(name, cancellationToken).ConfigureAwait(false);
        }
    }

    public Task<PeriodicTaskExecutionAttempt?> ExecuteAsync(string name, CancellationToken cancellationToken = default)
        => ExecuteAsync(name: name, throwOnError: false, awaitExecution: null, cancellationToken: cancellationToken);

    public async Task<PeriodicTaskExecutionAttempt?> ExecuteAsync(string name, bool throwOnError, bool? awaitExecution, CancellationToken cancellationToken = default)
    {
        var options = optionsMonitor.Get(name);

        // Instrumentation
        using var activity = PeriodicTasksActivitySource.StartActivity(ActivityNames.Execute, ActivityKind.Consumer);
        if (activity is not null)
        {
            activity.DisplayName = $"Periodic Task: {name}";
            activity.AddTag(ActivityTagNames.PeriodicTaskName, name);
            activity.AddTag(ActivityTagNames.PeriodicTaskType, typeof(TTask).FullName);
            activity.AddTag(ActivityTagNames.PeriodicTaskSchedule, options.Schedule!.ToString());
            activity.AddTag(ActivityTagNames.PeriodicTaskTimezone, TimeZoneInfo.FindSystemTimeZoneById(options.Timezone!).Id);
            activity.AddTag(ActivityTagNames.PeriodicTaskDeadline, options.Deadline!.ToString());
        }

        // create linked CancellationTokenSource and attach deadline if not null
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(options.Deadline!.Value);

        // execute the task
        var id = idGenerator.Generate(name, options.ExecutionIdFormat!.Value);
        var t = ExecuteInnerAsync(activity: activity,
                                  executionId: id,
                                  name: name,
                                  options: options,
                                  throwOnError: throwOnError,
                                  cancellationToken: cts.Token);

        if (awaitExecution ?? options.AwaitExecution)
        {
            try
            {
                return await t.ConfigureAwait(false);
            }
            // catch exception for cancellation
            catch (TaskCanceledException)
            {
                // if the new token is cancelled but the original is not, the task execution reached its deadline
                if (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                {
                    logger.ExecutionDeadlined(id);
                }
            }
        }

        return null;
    }

    internal async Task<PeriodicTaskExecutionAttempt?> ExecuteInnerAsync(Activity? activity, string executionId, string name, PeriodicTaskOptions options, bool throwOnError, CancellationToken cancellationToken = default)
    {
        var start = DateTimeOffset.UtcNow;
        var lockName = options.LockName!;
        var lockTimeout = options.LockTimeout!.Value;

        // acquire a distributed lock
        logger.AcquiringDistributedLock(lockName, executionId);
        var @lock = lockProvider.CreateLock(name: lockName);
        using var handle = await @lock.TryAcquireAsync(lockTimeout, cancellationToken).ConfigureAwait(false);
        if (handle is null)
        {
            logger.UnableToAcquireDistributedLock(lockName, executionId);
            return null; // do not do anything else
        }
        logger.AcquiredDistributedLock(lockName, executionId);

        // create a cancellation token linking the one supplied and the one from the lock when lost.
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, handle.HandleLostToken);

        using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var provider = scope.ServiceProvider;

        // execute the periodic task
        Exception? caught = null;
        try
        {
            var task = ActivatorUtilities.GetServiceOrCreateInstance<TTask>(provider);

            var context = new PeriodicTaskExecutionContext(name, executionId, typeof(TTask));

            // Invoke handler method, with resilience pipeline if specified
            var resiliencePipeline = options.ResiliencePipeline;
            if (resiliencePipeline is not null)
            {
                var contextData = new Dictionary<string, object> { ["context"] = context, };
                var attemptNumber = 0;
                await resiliencePipeline.ExecuteAsync(
                    async (ctx, ct) =>
                    {
                        attemptNumber++;
                        await ExecuteTrackedAsync(task, context, attemptNumber, cts.Token).ConfigureAwait(false);
                    },
                    contextData,
                    cancellationToken
                ).ConfigureAwait(false);
            }
            else
            {
                await ExecuteTrackedAsync(task, context, 1, cts.Token).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Interlocked.Exchange(ref caught, ex);
            logger.ExceptionInPeriodicTask(ex, executionId);

            // record the exception in the activity and set the status to error
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddException(ex);
        }

        var end = DateTimeOffset.UtcNow;

        // prepare attempt
        var attempt = new PeriodicTaskExecutionAttempt
        {
            Id = executionId,
            Name = name,
            ApplicationName = environment.ApplicationName,
            EnvironmentName = environment.EnvironmentName,
            MachineName = Environment.MachineName,
            Start = start,
            End = end,
            Successful = caught is null,
            ExceptionMessage = caught?.Message,
            ExceptionStackTrace = caught?.StackTrace,
        };

        // add attempt to store
        try
        {
            var attemptsStore = provider.GetRequiredService<IPeriodicTaskExecutionAttemptsStore>();
            await attemptsStore.AddAsync(attempt, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.ExceptionAddingToStore(ex, executionId);
        }

        // notify subscribers
        if (subscribers.Count > 0)
        {
            try
            {
                foreach (var s in subscribers)
                {
                    await s.OnExecutedAsync(attempt, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                logger.ExceptionNotifyingSubscribers(ex, executionId);
            }
        }

        // throw exception if any and if told to
        if (throwOnError && caught is not null)
        {
            throw new PeriodicTaskException($"Exception running {executionId}", caught)
            {
                Id = executionId,
                Name = name,
                TaskType = typeof(TTask),
            };
        }

        return attempt;
    }

    private async Task ExecuteTrackedAsync(IPeriodicTask task, PeriodicTaskExecutionContext context, int attemptNumber, CancellationToken cancellationToken)
    {
        using var activity = PeriodicTasksActivitySource.StartActivity(ActivityNames.ExecuteAttempt, ActivityKind.Consumer);
        if (activity is not null)
        {
            activity.DisplayName = $"Periodic Task: {context.Name} (Attempt: {attemptNumber})";
            activity.AddTag(ActivityTagNames.PeriodicTaskName, context.Name);
            activity.AddTag(ActivityTagNames.PeriodicTaskType, typeof(TTask).FullName);
            activity.AddTag(ActivityTagNames.PeriodicTaskAttemptNumber, attemptNumber);
        }

        try
        {
            await task.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddException(ex);
            throw;
        }
    }
}
