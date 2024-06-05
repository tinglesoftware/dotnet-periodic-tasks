using Microsoft.Extensions.Hosting;
using Polly;
using System.Diagnostics.CodeAnalysis;
using Tingle.PeriodicTasks;
using Tingle.PeriodicTasks.Internal;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Options for <see cref="PeriodicTasksHost"/>.</summary>
public class PeriodicTasksHostOptions
{
    /// Dictionary is used instead of HashSet to ensure the name (key) is never duplicated, case-insensitive.
    /// Names must be case-insensitive because they are used to form lock names and the underlying distributed
    /// lock provider may not support case sensitivity.
    /// For example, using files for locks may fail because file names on Windows are case insensitive.
    private readonly Dictionary<string, PeriodicTaskTypeRegistration> registrations = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the prefix value for the lock names.
    /// When not provided <see cref="IHostEnvironment.ApplicationName"/> is used.
    /// </summary>
    public string? LockNamePrefix { get; set; }

    /// <summary>
    /// Optional default <see cref="ResiliencePipeline"/> to use for periodic tasks where it is not specified.
    /// To specify a value per periodic task, use the <see cref="PeriodicTaskOptions.ResiliencePipeline"/> option.
    /// Defaults to <see langword="null"/>.
    /// </summary>
    public ResiliencePipeline? DefaultResiliencePipeline { get; set; }

    /// <summary>
    /// Gets or sets the default <see cref="CronSchedule"/> to use for periodic tasks where it is not specified.
    /// To specify a value per periodic task, use the <see cref="PeriodicTaskOptions.Schedule"/> option.
    /// Defaults to <see cref="CronSchedule.Hourly"/> which implies every top of the hour, every day.
    /// </summary>
    public CronSchedule DefaultSchedule { get; set; } = CronSchedule.Hourly;

    /// <summary>
    /// Gets or sets the default TimeZone identifier to use for periodic tasks where it is not specified.
    /// To specify a value per periodic task, use the <see cref="PeriodicTaskOptions.Timezone"/> option.
    /// Defaults to <c>Etc/UTC</c>.
    /// </summary>
    public string DefaultTimezone { get; set; } = "Etc/UTC";

    /// <summary>
    /// Gets or sets the default lock acquisition wait time to use for periodic tasks where it is not specified.
    /// To specify a value per periodic task, use the <see cref="PeriodicTaskOptions.LockTimeout"/> option.
    /// Defaults to <see cref="TimeSpan.Zero"/>.
    /// </summary>
    public TimeSpan DefaultLockTimeout { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Gets or sets the default maximum amount of the time for a single execution to use for periodic tasks where it is not specified.
    /// To specify a value per periodic task, use the <see cref="PeriodicTaskOptions.Deadline"/> option.
    /// Defaults to <c>59 minutes</c>.
    /// </summary>
    public TimeSpan DefaultDeadline { get; set; } = TimeSpan.FromMinutes(59);

    /// <summary>
    /// Gets or sets the default format for execution identifiers to use for periodic tasks where it is not specified.
    /// To specify a value per periodic task, use the <see cref="PeriodicTaskOptions.ExecutionIdFormat"/> option.
    /// Defaults to <see cref="PeriodicTaskIdFormat.GuidNoDashes"/>.
    /// </summary>
    public PeriodicTaskIdFormat DefaultExecutionIdFormat { get; set; } = PeriodicTaskIdFormat.GuidNoDashes;

    /// <summary>The periodic tasks registered.</summary>
    public IReadOnlyDictionary<string, PeriodicTaskTypeRegistration> Registrations => registrations;

    internal void AddRegistration<[DynamicallyAccessedMembers(TrimmingHelper.Task)] TTask>(string name)
        => registrations.Add(name, new(typeof(TTask), typeof(IPeriodicTaskRunner<TTask>)));
}

/// <summary>Registration for a periodic task.</summary>
public readonly record struct PeriodicTaskTypeRegistration([DynamicallyAccessedMembers(TrimmingHelper.Task)] Type Type, [DynamicallyAccessedMembers(TrimmingHelper.Task)] Type RunnerType)
{
    /// <summary>Deconstructs the registration into its parts.</summary>
    /// <param name="type">The type of the periodic task.</param>
    /// <param name="runnerType">The type of the runner for the periodic task.</param>
    public void Deconstruct(out Type type, out Type runnerType)
    {
        type = Type;
        runnerType = RunnerType;
    }
}
