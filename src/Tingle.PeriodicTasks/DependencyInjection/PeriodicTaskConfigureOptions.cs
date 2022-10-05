using Microsoft.Extensions.Options;
using Tingle.PeriodicTasks;

namespace Microsoft.Extensions.DependencyInjection;

internal class PeriodicTaskConfigureOptions : IConfigureNamedOptions<PeriodicTaskOptions>, IPostConfigureOptions<PeriodicTaskOptions>, IValidateOptions<PeriodicTaskOptions>
{
    private readonly PeriodicTasksHostOptions tasksHostOptions;

    public PeriodicTaskConfigureOptions(IOptions<PeriodicTasksHostOptions> tasksHostOptionsAccessor)
    {
        tasksHostOptions = tasksHostOptionsAccessor?.Value ?? throw new ArgumentNullException(nameof(tasksHostOptionsAccessor));
    }

    /// <inheritdoc/>
    public void Configure(PeriodicTaskOptions options)
    {
        throw new InvalidOperationException($"Unnamed '{nameof(PeriodicTasksHostOptions)}' options should not be configured.");
    }

    /// <inheritdoc/>
    public void Configure(string name, PeriodicTaskOptions options)
    {
        options.LockName = $"{tasksHostOptions.LockNamePrefix}:{name}";
    }

    /// <inheritdoc/>
    public void PostConfigure(string name, PeriodicTaskOptions options)
    {
        options.RetryPolicy ??= tasksHostOptions.DefaultRetryPolicy;
    }

    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string name, PeriodicTaskOptions options)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
        }

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
