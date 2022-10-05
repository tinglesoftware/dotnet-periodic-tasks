using Microsoft.Extensions.DependencyInjection.Extensions;
using Tingle.PeriodicTasks.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods on <see cref="PeriodicTasksBuilder"/> for periodic tasks.
/// </summary>
public static class PeriodicTasksBuilderExtensions
{
    /// <summary>
    /// Add support for exposing endpoints/routes on AspNetCore.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static PeriodicTasksBuilder AddAspNetCore(this PeriodicTasksBuilder builder)
        => builder.AddAspNetCore(_ => { /* nothing to do here really */ });

    /// <summary>
    /// Add support for exposing endpoints/routes on AspNetCore.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure">A <see cref="PeriodicTasksAspNetCoreOptions"/> used to configure the health checks.</param>
    /// <returns></returns>
    public static PeriodicTasksBuilder AddAspNetCore(this PeriodicTasksBuilder builder, Action<PeriodicTasksAspNetCoreOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var services = builder.Services;
        services.TryAddSingleton<PeriodicTasksMarkerService>();
        services.Configure(configure);
        services.AddScoped<PeriodicTasksEndpointsHandler>();
        services.AddHttpContextAccessor();

        return builder;
    }
}

///
public class PeriodicTasksAspNetCoreOptions
{

}
