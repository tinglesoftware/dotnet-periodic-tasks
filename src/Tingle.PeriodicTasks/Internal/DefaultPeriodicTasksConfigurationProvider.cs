using Microsoft.Extensions.Configuration;

namespace Tingle.PeriodicTasks.Internal;

/// <summary>
/// Default implementation of <see cref="IPeriodicTasksConfigurationProvider"/>.
/// </summary>
internal class DefaultPeriodicTasksConfigurationProvider : IPeriodicTasksConfigurationProvider
{
    private readonly IConfiguration configuration;
    private const string PeriodicTasksKey = "PeriodicTasks";

    public DefaultPeriodicTasksConfigurationProvider(IConfiguration configuration)
    {
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc/>
    public IConfiguration Configuration => configuration.GetSection(PeriodicTasksKey);
}
