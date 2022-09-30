using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

internal class PeriodicTasksHostConfigureOptions : IConfigureOptions<PeriodicTasksHostOptions>, IValidateOptions<PeriodicTasksHostOptions>
{
    private readonly IHostEnvironment environment;

    public PeriodicTasksHostConfigureOptions(IHostEnvironment environment)
    {
        this.environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <inheritdoc/>
    public void Configure(PeriodicTasksHostOptions options)
    {
        options.LockNamePrefix ??= environment.ApplicationName.ToLowerInvariant();
    }

    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string name, PeriodicTasksHostOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.LockNamePrefix))
        {
            return ValidateOptionsResult.Fail($"'{nameof(options.LockNamePrefix)}' must be provided.");
        }

        return ValidateOptionsResult.Success;
    }
}
