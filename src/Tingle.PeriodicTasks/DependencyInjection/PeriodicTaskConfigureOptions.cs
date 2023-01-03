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

        var configuration = configurationProvider.Configuration.GetSection($"Tasks:{name}");
        configuration.Bind(options);
    }

    public void PostConfigure(string? name, PeriodicTaskOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        options.LockName ??= $"{tasksHostOptions.LockNamePrefix}:{name}";
        options.RetryPolicy ??= tasksHostOptions.DefaultRetryPolicy;

        // try configure the description if null
        if (options.Description is null)
        {
            var type = tasksHostOptions.Registrations[name];
            var attrs = type.GetCustomAttributes(false);
            options.Description = attrs.OfType<PeriodicTaskDescriptionAttribute>().SingleOrDefault()?.Description
                               ?? attrs.OfType<DescriptionAttribute>().SingleOrDefault()?.Description
                               ?? string.Empty; // makes sure it is visible in AspNetCore endpoint responses
        }
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
        if (options.Schedule == default)
        {
            return ValidateOptionsResult.Fail($"'{nameof(options.Schedule)}' must be provided.");
        }

        // ensure we have a timezone
        if (options.Timezone is null)
        {
            return ValidateOptionsResult.Fail($"'{nameof(options.Timezone)}' must be provided.");
        }

        if (options.Deadline < TimeSpan.FromMinutes(1))
        {
            return ValidateOptionsResult.Fail($"'{nameof(options.Deadline)}' must be greater than or equal to 1 minute.");
        }

        return ValidateOptionsResult.Success;
    }
}
