namespace Tingle.PeriodicTasks.Internal;

internal static class DelayUtil
{
    public static async Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        // This method exists because we cannot delay for more than int.MaxValue so we have to split it into iterations
        // Precision on the millisecond level is not guaranteed and is a rare necessity
        var milliseconds = Convert.ToInt64(delay.TotalMilliseconds);
        var iterations = (milliseconds - 1) / int.MaxValue;

        while (iterations-- > 0)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(int.MaxValue), cancellationToken).ConfigureAwait(false);
            milliseconds -= int.MaxValue;
        }

        await Task.Delay(TimeSpan.FromMilliseconds(milliseconds), cancellationToken).ConfigureAwait(false);
    }
}
