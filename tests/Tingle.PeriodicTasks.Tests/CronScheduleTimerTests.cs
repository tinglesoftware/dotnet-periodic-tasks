namespace Tingle.PeriodicTasks.Tests;

public class CronScheduleTimerTests
{
    [Fact]
    public async Task CronScheduleTimer_Works()
    {
        var invocations = 0;
        var schedule = "*/3 * * * * *"; // every 3 seconds
        var timer = new CronScheduleTimer(schedule, async (t, ct) =>
        {
            Interlocked.Increment(ref invocations);
            await t.StopAsync(ct);
        });
        await timer.StartAsync();
        await Task.Delay(TimeSpan.FromSeconds(7));
        Assert.Equal(1, invocations);
    }
}
