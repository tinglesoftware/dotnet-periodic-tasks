using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tingle.PeriodicTasks.AspNetCore;

internal class PeriodicTasksEndpointsHandler
{
    private readonly IHttpContextAccessor contextAccessor;
    private readonly PeriodicTasksHostOptions hostOptions;
    private readonly IOptionsMonitor<PeriodicTaskOptions> optionsMonitor;

    public PeriodicTasksEndpointsHandler(IHttpContextAccessor contextAccessor, IOptions<PeriodicTasksHostOptions> hostOptionsAccessor, IOptionsMonitor<PeriodicTaskOptions> optionsMonitor)
    {
        this.contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        this.optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        hostOptions = hostOptionsAccessor?.Value ?? throw new ArgumentNullException(nameof(hostOptionsAccessor));
    }

    public IResult List() => Results.Ok(GetRegistrations());

    public IResult Get(string name) => Results.Ok(GetRegistration(name));

    public IResult GetHistory(string name)
    {
        var registration = GetRegistration(name);
        if (registration is null) return RegistrationNotFound(name);

        var attempts = new List<PeriodicTaskExecutionAttempt>(); // will have data once we have storage support
        return Results.Ok(attempts);
    }

    public async Task<IResult> ExecuteAsync(PeriodicTaskExecutionRequest request)
    {
        var name = request.Name ?? throw new InvalidOperationException("The name of the periodic task must be provided!");
        var registration = GetRegistration(name);
        if (registration is null) return RegistrationNotFound(name);

        // find the task type
        var context = GetHttpContext();
        var provider = context.RequestServices;
        var type = hostOptions.Registrations[name];

        // make the runner type
        var genericRunnerType = typeof(IPeriodicTaskRunner<>);
        var runnerType = genericRunnerType.MakeGenericType(type);
        var runner = (IPeriodicTaskRunner)provider.GetRequiredService(runnerType);

        // execute
        var cancellationToken = context.RequestAborted;
        var t = runner.ExecuteAsync(name: name, throwOnError: request.Throw, cancellationToken: cancellationToken);
        if (request.Wait)
        {
            await t.ConfigureAwait(false);
        }

        PeriodicTaskExecutionAttempt? attempt = null; // when attempt is offered, use it
        return Results.Ok(attempt);
    }

    private PeriodicTaskRegistration? GetRegistration(string name)
        => GetRegistrations().SingleOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));

    private List<PeriodicTaskRegistration> GetRegistrations()
    {
        var provider = GetRequestServices();
        var registrations = hostOptions.Registrations;
        var results = new List<PeriodicTaskRegistration>();
        foreach (var (name, type) in registrations)
        {
            var options = optionsMonitor.Get(name);
            results.Add(new PeriodicTaskRegistration(name, type, options));
        }
        return results;
    }

    private IServiceProvider GetRequestServices() => GetHttpContext().RequestServices;
    private HttpContext GetHttpContext() => contextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext must be accessible");

    private static IResult RegistrationNotFound(string name)
    {
        return Results.Problem(title: "periodic_task_registration_not_found",
                               detail: $"No periodic task named '{name}' exists.",
                               statusCode: 400);
    }
}
