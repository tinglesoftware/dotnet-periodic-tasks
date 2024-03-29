﻿using Microsoft.Extensions.DependencyInjection;
using Polly;

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
    /// Optional description of the task.
    /// This value is useful for display purposes.
    /// <br/>
    /// When not configured, a value is pulled from either <see cref="PeriodicTaskDescriptionAttribute"/>
    /// or <see cref="System.ComponentModel.DescriptionAttribute"/> as annotated/decorated on the task.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the task should be executed on startup.
    /// This is useful for tasks that need to run early then repeat a while later
    /// instead of having a high rate <see cref="Schedule"/>.
    /// <br/>
    /// <see cref="Enable"/> must be <see langword="true"/> for this to work.
    /// </summary>
    public virtual bool ExecuteOnStartup { get; set; }

    /// <summary>
    /// Gets or sets the execution schedule.
    /// A value can be also be configured via <see cref="PeriodicTaskScheduleAttribute"/> on the task.
    /// <br/>
    /// When set to <see langword="null"/>, <see cref="PeriodicTasksHostOptions.DefaultSchedule"/> is used.
    /// Defaults to <see langword="null"/>.
    /// </summary>
    public CronSchedule? Schedule { get; set; }

    /// <summary>
    /// Gets or sets the TimeZone identifier in which the <see cref="Schedule"/> will operate.
    /// A value can be also be configured via <see cref="PeriodicTaskScheduleAttribute"/> on the task.
    /// <br/>
    /// When set to <see langword="null"/>, <see cref="PeriodicTasksHostOptions.DefaultTimezone"/> is used.
    /// Defaults to <see langword="null"/>.
    /// </summary>
    public string? Timezone { get; set; }

    /// <summary>
    /// Gets or sets how long to wait before giving up on lock acquisition.
    /// <br/>
    /// When set to <see langword="null"/>, <see cref="PeriodicTasksHostOptions.DefaultLockTimeout"/> is used.
    /// Defaults to <see langword="null"/>.
    /// </summary>
    public TimeSpan? LockTimeout { get; set; }

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
    /// When set to <see langword="null"/>, <see cref="PeriodicTasksHostOptions.DefaultDeadline"/> is used.
    /// Defaults to <see langword="null"/>.
    /// </summary>
    public TimeSpan? Deadline { get; set; }

    /// <summary>
    /// The preferred format to use when generating identifiers for executions.
    /// <br/>
    /// When set to <see langword="null"/>, <see cref="PeriodicTasksHostOptions.DefaultExecutionIdFormat"/> is used.
    /// Defaults to <see langword="null"/>.
    /// </summary>
    public PeriodicTaskIdFormat? ExecutionIdFormat { get; set; }

    /// <summary>
    /// The name of the distributed lock to be acquired during execution.
    /// </summary>
    public string? LockName { get; set; }

    /// <summary>
    /// The <see cref="Polly.ResiliencePipeline"/> to apply when executing the job.
    /// This is an outer wrapper around the
    /// <see cref="IPeriodicTask.ExecuteAsync(PeriodicTaskExecutionContext, CancellationToken)"/>
    /// method.
    /// When set to <see langword="null"/>, the method is only invoked once.
    /// Defaults to <see langword="null"/>.
    /// When this value is set, it overrides the default value set on the host.
    /// </summary>
    /// <remarks>
    /// When a value is provided, the host may extend the duration of the distributed for the task
    /// until the execution with this pipeline completes successfully or not.
    /// </remarks>
    public ResiliencePipeline? ResiliencePipeline { get; set; }
}
