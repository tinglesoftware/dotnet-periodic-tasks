namespace Tingle.PeriodicTasks;

/// <summary>A simple timer based on <see cref="CronSchedule"/>.</summary>
public class CronScheduleTimer : IDisposable
{
    private readonly CronSchedule schedule;
    private readonly TimeZoneInfo timezone;

    private readonly Func<CronScheduleTimer, object?, CancellationToken, Task> callback;
    private readonly object? callbackArg;

    private Task? task;
    private CancellationTokenSource? lifetimeCts;

    /// <summary>Creates an instance of <see cref="CronScheduleTimer"/>.</summary>
    /// <param name="schedule">CRON Schedule to use.</param>
    /// <param name="timezone">Timezone to use.</param>
    /// <param name="callback">Function to be invoked per schedule. It should b short lived to avoid altering the subsequent trigger.</param>
    /// <param name="callbackArg">Optional argument to be passed to <paramref name="callback"/> during invocation.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CronScheduleTimer(CronSchedule schedule, string timezone, Func<CronScheduleTimer, object?, CancellationToken, Task> callback, object? callbackArg)
    {
        this.schedule = schedule;
        this.timezone = TimeZoneInfo.FindSystemTimeZoneById(timezone ?? throw new ArgumentNullException(nameof(timezone)));
        this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
        this.callbackArg = callbackArg;
    }

    /// <summary>Creates an instance of <see cref="CronScheduleTimer"/>.</summary>
    /// <param name="schedule">CRON Schedule to use.</param>
    /// <param name="timezone">Timezone to use.</param>
    /// <param name="callback">Function to be invoked per schedule. It should b short lived to avoid altering the subsequent trigger.</param>
    public CronScheduleTimer(CronSchedule schedule, string timezone, Func<CronScheduleTimer, CancellationToken, Task> callback)
        : this(schedule, timezone, (t, a, c) => callback(t, c), null) { }

    /// <summary>Creates an instance of <see cref="CronScheduleTimer"/>.</summary>
    /// <param name="schedule">CRON Schedule to use.</param>
    /// <param name="callback">Function to be invoked per schedule. It should b short lived to avoid altering the subsequent trigger.</param>
    /// <param name="callbackArg">Optional argument to be passed to <paramref name="callback"/> during invocation.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CronScheduleTimer(CronSchedule schedule, Func<CronScheduleTimer, object?, CancellationToken, Task> callback, object? callbackArg)
        : this(schedule, "Etc/UTC", callback, callbackArg) { }

    /// <summary>Creates an instance of <see cref="CronScheduleTimer"/>.</summary>
    /// <param name="schedule">CRON Schedule to use.</param>
    /// <param name="callback">Function to be invoked per schedule. It should b short lived to avoid altering the subsequent trigger.</param>
    public CronScheduleTimer(CronSchedule schedule, Func<CronScheduleTimer, CancellationToken, Task> callback)
        : this(schedule, "Etc/UTC", (t, a, c) => callback(t, c), null) { }

    /// <summary>CRON Schedule in use.</summary>
    public CronSchedule Schedule => schedule;

    /// <summary>Timezone in which to operate.</summary>
    public TimeZoneInfo Timezone => timezone;

    /// <summary>Triggered when the creator is ready to run the schedule.</summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        // Create linked token to allow cancelling executing task from provided token
        lifetimeCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Store the task we're executing
        task = ExecuteAsync(lifetimeCts.Token);

        return Task.CompletedTask;
    }

    /// <summary>Triggered when the creator is performing a graceful shutdown.</summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        // Stop called without start
        if (task == null) return;

        try
        {
            // Signal cancellation to the executing method
            lifetimeCts!.Cancel();
        }
        finally
        {
            // Wait until the task completes or the stop token triggers
            await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
        }
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var next = schedule.GetNextOccurrence(DateTimeOffset.UtcNow, timezone);
            if (next is null)
            {
                throw new InvalidOperationException($"Could not infer next occurrence for '{schedule}' in '{timezone}'");
            }

            // calculate the delay to apply
            var delay = next.Value - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Internal.DelayUtil.DelayAsync(delay, cancellationToken).ConfigureAwait(false);
            }

            // invoke the callback
            await callback(this, callbackArg, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        lifetimeCts?.Cancel();
        GC.SuppressFinalize(this);
    }
}
