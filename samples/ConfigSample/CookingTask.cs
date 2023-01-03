using Tingle.PeriodicTasks;

namespace ConfigSample;

internal class CookingTask : IPeriodicTask
{
    private readonly ILogger logger;

    public CookingTask(ILogger<CookingTask> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(string name, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("The food is ready to eat");
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
    }
}
