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
    private readonly Dictionary<string, Type> registrations = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the prefix value for the lock names.
    /// When not provided <see cref="IHostEnvironment.ApplicationName"/> is used.
    /// </summary>
    public string? LockNamePrefix { get; set; }

    /// <summary>
    /// Optional default retry policy to use for periodic tasks where it is not specified.
    /// To specify a value per periodic task, use the <see cref="PeriodicTaskOptions.RetryPolicy"/> option.
    /// Defaults to <see langword="null"/>.
    /// </summary>
    public AsyncPolicy? DefaultRetryPolicy { get; set; }

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
    public IReadOnlyDictionary<string, Type> Registrations => registrations;

    internal void AddRegistration(string name, [DynamicallyAccessedMembers(TrimmingHelper.Task)] Type type) => registrations.Add(name, type);
}
