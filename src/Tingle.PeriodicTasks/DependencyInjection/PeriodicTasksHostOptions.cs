using Microsoft.Extensions.Hosting;
using Polly.Retry;
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
    public AsyncRetryPolicy? DefaultRetryPolicy { get; set; }

    /// <summary>
    /// Gets or sets the default TimeZone identifier to use for periodic tasks where it is not specified.
    /// To specify a value per periodic task, use the <see cref="PeriodicTaskOptions.Timezone"/> option.
    /// Defaults to <c>Etc/UTC</c>.
    /// </summary>
    public string DefaultTimezone { get; set; } = "Etc/UTC";

    /// <summary>The periodic tasks registered.</summary>
    public IReadOnlyDictionary<string, Type> Registrations => registrations;

    internal void AddRegistration(string name, Type type) => registrations.Add(name, type);
}
