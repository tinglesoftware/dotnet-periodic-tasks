using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using Tingle.PeriodicTasks;

namespace Microsoft.Extensions.DependencyInjection;

internal class PeriodicTaskConfigureOptions(IOptions<PeriodicTasksHostOptions> tasksHostOptionsAccessor,
                                            IPeriodicTasksConfigurationProvider configurationProvider) : IConfigureNamedOptions<PeriodicTaskOptions>,
                                                                                                         IPostConfigureOptions<PeriodicTaskOptions>,
                                                                                                         IValidateOptions<PeriodicTaskOptions>
{
    private readonly PeriodicTasksHostOptions tasksHostOptions = tasksHostOptionsAccessor?.Value ?? throw new ArgumentNullException(nameof(tasksHostOptionsAccessor));

    /// <inheritdoc/>
    public void Configure(PeriodicTaskOptions options)
    {
        throw new InvalidOperationException($"Unnamed '{nameof(PeriodicTasksHostOptions)}' options should not be configured.");
    }

    /// <inheritdoc/>
    public void Configure(string? name, PeriodicTaskOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        // set schedule and timezone from Attribute
        var type = tasksHostOptions.Registrations[name];
        var attrs = type.GetCustomAttributes(false);
        if (attrs.OfType<PeriodicTaskScheduleAttribute>().SingleOrDefault() is PeriodicTaskScheduleAttribute attrSchedule)
        {
            options.Schedule ??= attrSchedule.Schedule;
            options.Timezone ??= attrSchedule.Timezone;
        }

        // set description from Attribute if null
        options.Description ??= attrs.OfType<PeriodicTaskDescriptionAttribute>().SingleOrDefault()?.Description
                             ?? attrs.OfType<DescriptionAttribute>().SingleOrDefault()?.Description
                             ?? string.Empty; // makes sure it is visible in AspNetCore endpoint responses

        // bind using the inferred/formatted name
        var configuration = configurationProvider.Configuration.GetSection($"Tasks:{name}");
        PeriodicTaskConfigureOptions.PopulateFromConfiguration(configuration, options);

        // binding using short name is not done because it may result in duplicates

        // bind using the full type name
        configuration = configurationProvider.Configuration.GetSection($"Tasks:{type.FullName}");
        PeriodicTaskConfigureOptions.PopulateFromConfiguration(configuration, options);
    }

    /// <inheritdoc/>
    public void PostConfigure(string? name, PeriodicTaskOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        options.LockName ??= $"{tasksHostOptions.LockNamePrefix}:{name}";
        options.Schedule ??= tasksHostOptions.DefaultSchedule;
        options.Timezone ??= tasksHostOptions.DefaultTimezone;
        options.LockTimeout ??= tasksHostOptions.DefaultLockTimeout;
        options.Deadline ??= tasksHostOptions.DefaultDeadline;
        options.ExecutionIdFormat ??= tasksHostOptions.DefaultExecutionIdFormat;
        options.RetryPolicy ??= tasksHostOptions.DefaultRetryPolicy;
    }

    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, PeriodicTaskOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        // ensure we have a lock name
        if (string.IsNullOrWhiteSpace(options.LockName))
        {
            return ValidateOptionsResult.Fail($"'{nameof(options.LockName)}' must be provided.");
        }

        // ensure we have a schedule
        if (options.Schedule is null)
        {
            return ValidateOptionsResult.Fail($"'{nameof(options.Schedule)}' must be provided.");
        }

        // ensure we have a timezone
        if (options.Timezone is null)
        {
            return ValidateOptionsResult.Fail($"'{nameof(options.Timezone)}' must be provided.");
        }

        // ensure we have a valid timezone
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(options.Timezone);
        }
        catch (TimeZoneNotFoundException)
        {
            return ValidateOptionsResult.Fail($"'{nameof(options.Timezone)}' must be a valid Windows or IANA TimeZone identifier.");
        }

        // ensure deadline is not less than 1 minute
        if (options.Deadline < TimeSpan.FromMinutes(1))
        {
            return ValidateOptionsResult.Fail($"'{nameof(options.Deadline)}' must be greater than or equal to 1 minute.");
        }

        return ValidateOptionsResult.Success;
    }

    internal static void PopulateFromConfiguration(IConfiguration config, PeriodicTaskOptions options)
    {
        if (config.TryGetValue(nameof(options.Enable), out var value)) options.Enable = bool.Parse(value);
        if (config.TryGetValue(nameof(options.Description), out value)) options.Description = value;
        if (config.TryGetValue(nameof(options.ExecuteOnStartup), out value)) options.ExecuteOnStartup = bool.Parse(value);
        if (config.TryGetValue(nameof(options.Schedule), out value)) options.Schedule = value;
        if (config.TryGetValue(nameof(options.Timezone), out value)) options.Timezone = value;
        if (config.TryGetValue(nameof(options.LockTimeout), out value)) options.LockTimeout = TimeSpan.Parse(value);
        if (config.TryGetValue(nameof(options.AwaitExecution), out value)) options.AwaitExecution = bool.Parse(value);
        if (config.TryGetValue(nameof(options.Deadline), out value)) options.Deadline = TimeSpan.Parse(value);
        if (config.TryGetValue(nameof(options.ExecutionIdFormat), out value)) options.ExecutionIdFormat = Enum.Parse<PeriodicTaskIdFormat>(value, ignoreCase: true);
        if (config.TryGetValue(nameof(options.LockName), out value)) options.LockName = value;
    }
}
