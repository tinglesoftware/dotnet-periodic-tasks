using Tingle.PeriodicTasks;

var databaseTimer = new CronScheduleTimer("*/1 * * * *", async (_, cancellationToken) =>
{
    Console.WriteLine("Cleaned up old records from the database");
    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
});
var dnsTimer = new CronScheduleTimer("*/5 * * * * *", async (_, cancellationToken) =>
{
    Console.WriteLine("All DNS records are fine");
    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
});

using var cts = new CancellationTokenSource();

// Listen for Ctrl+C or SIGINT
Console.CancelKeyPress += (sender, eventArgs) =>
{
    cts.Cancel();                // Trigger the token
    eventArgs.Cancel = true;     // Prevent immediate shutdown
};

var cancellationToken = cts.Token;
Console.WriteLine("Started timers.");
Console.WriteLine("Press Ctrl+C to exit...");
await databaseTimer.StartAsync(cancellationToken);
await dnsTimer.StartAsync(cancellationToken);

try
{
    // Wait for the cancellation token to be triggered
    await Task.Delay(Timeout.Infinite, cancellationToken);
}
catch (TaskCanceledException) { }
finally
{
    await databaseTimer.StopAsync();
    await dnsTimer.StopAsync();
}
