using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Tingle.PeriodicTasks;

namespace Microsoft.Extensions.DependencyInjection;

internal class PeriodicTasksHostConfigureOptions : IConfigureOptions<PeriodicTasksHostOptions>, IValidateOptions<PeriodicTasksHostOptions>
{
    private readonly IHostEnvironment environment;
    private readonly IPeriodicTasksConfigurationProvider configurationProvider;

    public PeriodicTasksHostConfigureOptions(IHostEnvironment environment, IPeriodicTasksConfigurationProvider configurationProvider)
    {
        this.environment = environment ?? throw new ArgumentNullException(nameof(environment));
        this.configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
    }

    /// <inheritdoc/>
    public void Configure(PeriodicTasksHostOptions options)
    {
        var config = configurationProvider.Configuration;
        if (config.TryGetValue(nameof(options.LockNamePrefix), out var value)) options.LockNamePrefix = value;
        if (config.TryGetValue(nameof(options.DefaultSchedule), out value)) options.DefaultSchedule = value;
        if (config.TryGetValue(nameof(options.DefaultTimezone), out value)) options.DefaultTimezone = value;
        if (config.TryGetValue(nameof(options.DefaultLockTimeout), out value)) options.DefaultLockTimeout = TimeSpan.Parse(value);
        if (config.TryGetValue(nameof(options.DefaultDeadline), out value)) options.DefaultDeadline = TimeSpan.Parse(value);
        if (config.TryGetValue(nameof(options.DefaultExecutionIdFormat), out value)) options.DefaultExecutionIdFormat = Enum.Parse<PeriodicTaskIdFormat>(value, ignoreCase: true);

        // Set the default LockNamePrefix
        options.LockNamePrefix ??= environment.ApplicationName.ToLowerInvariant();
    }

    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, PeriodicTasksHostOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.LockNamePrefix))
        {
            return ValidateOptionsResult.Fail($"'{nameof(options.LockNamePrefix)}' must be provided.");
        }

        // ensure we have a default schedule
        if (options.DefaultSchedule == default)
        {
            return ValidateOptionsResult.Fail($"'{nameof(options.DefaultSchedule)}' must be provided.");
        }

        // ensure we have a default timezone
        if (string.IsNullOrWhiteSpace(options.DefaultTimezone))
        {
            return ValidateOptionsResult.Fail($"'{nameof(options.DefaultTimezone)}' must be provided.");
        }

        // ensure we have a valid default timezone
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(options.DefaultTimezone);
        }
        catch (TimeZoneNotFoundException)
        {
            return ValidateOptionsResult.Fail($"'{nameof(options.DefaultTimezone)}' must be a valid Windows or IANA TimeZone identifier.");
        }

        // ensure the default deadline is not less than 1 minute
        if (options.DefaultDeadline < TimeSpan.FromMinutes(1))
        {
            return ValidateOptionsResult.Fail($"'{nameof(options.DefaultDeadline)}' must be greater than or equal to 1 minute.");
        }

        return ValidateOptionsResult.Success;
    }
}
