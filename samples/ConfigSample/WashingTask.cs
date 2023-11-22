using Tingle.PeriodicTasks;

namespace ConfigSample;

internal class WashingTask(ILogger<WashingTask> logger) : IPeriodicTask
{
    public async Task ExecuteAsync(PeriodicTaskExecutionContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("I will do the cleaning later. Have you eaten yet?");
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
    }
}
