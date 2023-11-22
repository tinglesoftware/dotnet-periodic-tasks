using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.Configuration;

/// <summary>Extensions for <see cref="IConfiguration"/>.</summary>
public static class IConfigurationExtensions
{
    /// <summary>
    /// Gets the name of the periodic task configured to run on startup.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/> to use.</param>
    /// <param name="name">
    /// When this method returns, contains the name of the periodic task to run on startup,
    /// if the configuration key is found with a non-null value; otherwise, the default 
    /// value for the type of the value parameter.
    /// This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="IConfiguration"/> contains a non-null value
    /// for the <c>PERIODIC_TASK_NAME</c> configuration key; otherwise <see langword="false"/>.
    /// </returns>
    public static bool TryGetPeriodicTaskName(this IConfiguration configuration, [NotNullWhen(true)] out string? name)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return (name = configuration["PERIODIC_TASK_NAME"] is string s ? s : null) is not null;
    }

    internal static bool TryGetValue(this IConfiguration configuration, string key, [NotNullWhen(true)] out string? value)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return (value = configuration[key] is string s ? s : null) is not null;
    }
}
