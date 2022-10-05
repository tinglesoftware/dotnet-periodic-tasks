using Polly.Retry;

namespace Tingle.PeriodicTasks;

/// <summary>Options for an <see cref="IPeriodicTask"/>.</summary>
public class PeriodicTaskOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the task is enabled.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool Enable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the task should be executed on startup.
    /// This is useful for tasks that need to run early then repeat a while later
    /// instead of having a high rate <see cref="Schedule"/>.
    /// <br/>
    /// <see cref="Enable"/> must be <see langword="true"/> for this to work.
    /// </summary>
    public virtual bool ExecuteOnStartup { get; set; }

    /// <summary>
    /// Gets or sets the execution schedule. Defaults to <see cref="CronSchedule.Hourly"/>
    /// which implies every top of the hour, every day
    /// </summary>
    public CronSchedule Schedule { get; set; } = CronSchedule.Hourly;

    /// <summary>
    /// Gets or sets the <see cref="TimeZoneInfo"/> in which the
    /// <see cref="Schedule"/> will operate.
    /// Defaults to <see cref="TimeZoneInfo.Utc"/>.
    /// </summary>
    public TimeZoneInfo Timezone { get; set; } = TimeZoneInfo.Utc;

    /// <summary>
    /// Gets or sets how long to wait before giving up on lock acquisition.
    /// Defaults to <see cref="TimeSpan.Zero"/>.
    /// </summary>
    public TimeSpan LockTimeout { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Gets or sets whether the task execution should be awaited in the
    /// same thread synchronization context for scheduling.
    /// When set to <see langword="true"/>, some executions of the job
    /// maybe skipped if one execution takes a lot of time.
    /// <br/>
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool AwaitExecution { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum amount of the time a single execution of the task is allowed to run.
    /// <br/>
    /// Defaults to <c>59 minutes</c>.
    /// </summary>
    public TimeSpan Deadline { get; set; } = TimeSpan.FromMinutes(59);

    /// <summary>
    /// The preferred format to use when generating identifiers for executions.
    /// Defaults to <see cref="PeriodicTaskIdFormat.GuidNoDashes"/>.
    /// </summary>
    public PeriodicTaskIdFormat ExecutionIdFormat { get; set; } = PeriodicTaskIdFormat.GuidNoDashes;

    /// <summary>
    /// The retry policy to apply when executing the job.
    /// This is an outer wrapper around the
    /// <see cref="IPeriodicTask.ExecuteAsync(string, CancellationToken)"/>
    /// method.
    /// When set to <see langword="null"/>, the method is only invoked once.
    /// Defaults to <see langword="null"/>.
    /// When this value is set, it overrides the default value set on the host.
    /// </summary>
    /// <remarks>
    /// When a value is provided, the host may extend the duration of the distributed for the task
    /// until the execution with retry policy completes successfully or not.
    /// </remarks>
    public AsyncRetryPolicy? RetryPolicy { get; set; }

    /// <summary>
    /// The name of the distributed lock to be acquired during execution.
    /// </summary>
    public string? LockName { get; set; }
}
