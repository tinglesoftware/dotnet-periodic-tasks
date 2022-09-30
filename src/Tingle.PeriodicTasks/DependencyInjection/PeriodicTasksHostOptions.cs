using Medallion.Threading;
using Microsoft.Extensions.Hosting;
using Polly.Retry;
using Tingle.PeriodicTasks;
using Tingle.PeriodicTasks.Internal;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Options for <see cref="PeriodicTasksHost"/>.</summary>
public class PeriodicTasksHostOptions
{
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
    /// </summary>
    /// <remarks>
    /// <see cref="Dictionary{TKey, TValue}"/> is used instead of <see cref="HashSet{T}"/>
    /// to ensure the name (key) is never duplicated, case-insensitive.
    /// <br/>
    /// Names must be case-insensitive because they are used to form lock names and the
    /// underlying <see cref="IDistributedLockProvider"/> may not support case sensitivity.
    /// For example, using files for locks may fail because file names on Windows are case insensitive.
    /// </remarks>
    internal Dictionary<string, Type> Registrations { get; } = new(StringComparer.OrdinalIgnoreCase);
}
