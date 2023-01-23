using Microsoft.Extensions.DependencyInjection;

namespace Tingle.PeriodicTasks;

/// <summary>
/// Sets the schedule (and optionally the timezone) of a periodic task.
/// The values set using this attribute can be overridden using configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PeriodicTaskScheduleAttribute : Attribute
{
    ///
    public PeriodicTaskScheduleAttribute(string schedule, string? timezone = null)
    {
        Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
        Timezone = timezone;
    }

    ///
    public PeriodicTaskScheduleAttribute(CronSchedule schedule, string? timezone = null)
    {
        Schedule = schedule;
        Timezone = timezone;
    }

    /// <summary>Gets or sets the execution schedule.</summary>
    public CronSchedule Schedule { get; set; }

    /// <summary>
    /// Gets or sets the TimeZone identifier in which the <see cref="Schedule"/> will operate.
    /// This value can be also be set using <see cref="PeriodicTaskOptions.Timezone"/> or via configuration.
    /// <br/>
    /// When set to <see langword="null"/>, <see cref="PeriodicTasksHostOptions.DefaultTimezone"/> is used.
    /// </summary>
    public string? Timezone { get; set; }
}
