using Microsoft.Extensions.Configuration;

namespace Tingle.PeriodicTasks;

/// <summary>
/// Provides an interface for implementing a construct that provides
/// access to PeriodicTasks-related configuration sections.
/// </summary>
public interface IPeriodicTasksConfigurationProvider
{
    /// <summary>
    /// Gets the <see cref="IConfigurationSection"/> where PeriodicTasks options are stored.
    /// </summary>
    IConfiguration Configuration { get; }
}
