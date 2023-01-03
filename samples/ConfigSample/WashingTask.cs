using Tingle.PeriodicTasks;

namespace ConfigSample;

internal class WashingTask : IPeriodicTask
{
    private readonly ILogger logger;

    public WashingTask(ILogger<WashingTask> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(string name, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("I will do the cleaning later. Have you eaten yet?");
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
    }
}
