using Medallion.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tingle.PeriodicTasks.Internal;

internal class PeriodicTaskRunner<TTask> : IPeriodicTaskRunner<TTask>
    where TTask : class, IPeriodicTask
{
    private readonly IServiceProvider serviceProvider;
    private readonly IHostEnvironment environment;
    private readonly IPeriodicTaskIdGenerator idGenerator;
    private readonly IOptionsMonitor<PeriodicTaskOptions> optionsMonitor;
    private readonly ILogger logger;

    public PeriodicTaskRunner(IServiceProvider serviceProvider,
                              IHostEnvironment environment,
                              IPeriodicTaskIdGenerator idGenerator,
                              IOptionsMonitor<PeriodicTaskOptions> optionsMonitor,
                              ILogger<PeriodicTaskRunner<TTask>> logger)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this.environment = environment ?? throw new ArgumentNullException(nameof(environment));
        this.idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task RunAsync(string name, CancellationToken cancellationToken)
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

        var schedule = options.Schedule;
        while (!cancellationToken.IsCancellationRequested)
        {
            var timezone = options.Timezone;
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
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }

            // execute the task
            await ExecuteAsync(name, cancellationToken).ConfigureAwait(false);
        }
    }

    public Task ExecuteAsync(string name, CancellationToken cancellationToken)
        => ExecuteAsync(name: name, throwOnError: false, cancellationToken: cancellationToken);

    public async Task ExecuteAsync(string name, bool throwOnError, CancellationToken cancellationToken)
    {
        var options = optionsMonitor.Get(name);

        // create linked CancellationTokenSource and attach deadline if not null
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(options.Deadline);

        // execute the task
        var id = idGenerator.Generate(name, options.ExecutionIdFormat);
        var t = ExecuteInnerAsync(executionId: id,
                                  name: name,
                                  options: options,
                                  throwOnError: throwOnError,
                                  cancellationToken: cts.Token);

        if (options.AwaitExecution)
        {
            try
            {
                await t.ConfigureAwait(false);
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
    }

    internal async Task ExecuteInnerAsync(string executionId, string name, PeriodicTaskOptions options, bool throwOnError, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var provider = scope.ServiceProvider;

        var start = DateTimeOffset.UtcNow;
        var lockName = options.LockName!;
        var lockTimeout = options.LockTimeout;

        // acquire a distributed lock
        var lockProvider = provider.GetRequiredService<IDistributedLockProvider>();
        logger.AcquiringDistributedLock(lockName, executionId);
        var @lock = lockProvider.CreateLock(name: lockName);
        using var handle = await @lock.TryAcquireAsync(lockTimeout, cancellationToken).ConfigureAwait(false);
        if (handle is null)
        {
            logger.UnableToAcquireDistributedLock(lockName, executionId);
            return; // do not do anything else
        }
        logger.AcquiredDistributedLock(lockName, executionId);

        // create a cancellation token linking the one supplied and the one from the lock when lost.
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, handle.HandleLostToken);

        // execute the periodic task
        Exception? caught = null;
        try
        {
            var task = ActivatorUtilities.GetServiceOrCreateInstance<TTask>(provider);

            // Invoke handler method, with retry if specified
            var retryPolicy = options.RetryPolicy;
            if (retryPolicy != null)
            {
                await retryPolicy.ExecuteAsync(ct => task.ExecuteAsync(name, cts.Token), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await task.ExecuteAsync(name, cts.Token).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            logger.ExceptionInPeriodicTask(ex, executionId);
            Interlocked.Exchange(ref caught, ex);
        }

        var end = DateTimeOffset.UtcNow;

        // notify subscribers
        var subscribers = provider.GetRequiredService<IEnumerable<IPeriodicTaskEventSubscriber>>();
        if (subscribers.Any())
        {
            // prepare notification
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
            throw new PeriodicTaskException($"Exception running {executionId}", caught);
        }
    }
}
