using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using Tingle.PeriodicTasks;

namespace Microsoft.Extensions.DependencyInjection;

internal class PeriodicTaskConfigureOptions : IConfigureNamedOptions<PeriodicTaskOptions>, IPostConfigureOptions<PeriodicTaskOptions>, IValidateOptions<PeriodicTaskOptions>
{
    private readonly PeriodicTasksHostOptions tasksHostOptions;
    private readonly IPeriodicTasksConfigurationProvider configurationProvider;

    public PeriodicTaskConfigureOptions(IOptions<PeriodicTasksHostOptions> tasksHostOptionsAccessor, IPeriodicTasksConfigurationProvider configurationProvider)
    {
        tasksHostOptions = tasksHostOptionsAccessor?.Value ?? throw new ArgumentNullException(nameof(tasksHostOptionsAccessor));
        this.configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
    }

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
        configuration.Bind(options);

        // binding using short name is not done because it may result in duplicates

        // bind using the full type name
        configuration = configurationProvider.Configuration.GetSection($"Tasks:{type.FullName}");
        configuration.Bind(options);
    }

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
}
