namespace Microsoft.Extensions.Logging;

internal static partial class ILoggerExtensions
{
    #region Host

    [LoggerMessage(101, LogLevel.Debug, "Application did not startup. Periodic Tasks cannot continue.")]
    public static partial void ApplicationDidNotStartup(this ILogger logger);

    #endregion

    #region Runner

    [LoggerMessage(201, LogLevel.Trace, "Period Task {TaskName} is not enabled.")]
    public static partial void TaskNotEnabled(this ILogger logger, string taskName);

    [LoggerMessage(202, LogLevel.Error, "Unable to get the next occurrence for {Expression} in {TimezoneId} used by '{TaskName}'")]
    public static partial void UnableToGetNextOccurrence(this ILogger logger, string expression, string timezoneId, string taskName);

    [LoggerMessage(203, LogLevel.Debug, "Delaying for {DelayReadable} ({Delay}) to the next schedule at {Next} for '{TaskName}'")]
    public static partial void DelayingToNextOccurrence(this ILogger logger, string delayReadable, TimeSpan delay, DateTimeOffset next, string taskName);

    [LoggerMessage(204, LogLevel.Warning, "Execution of '{ExecutionId}' reached it's deadline.")]
    public static partial void ExecutionDeadlined(this ILogger logger, string executionId);

    [LoggerMessage(205, LogLevel.Error, "Exception running '{ExecutionId}'")]
    public static partial void ExceptionInPeriodicTask(this ILogger logger, Exception exception, string executionId);

    [LoggerMessage(206, LogLevel.Error, "Exception notifying subscriber(s) about '{ExecutionId}'")]
    public static partial void ExceptionNotifyingSubscribers(this ILogger logger, Exception exception, string executionId);

    [LoggerMessage(207, LogLevel.Debug, "Acquiring distributed lock '{LockName}' for '{ExecutionId}'")]
    public static partial void AcquiringDistributedLock(this ILogger logger, string lockName, string executionId);

    [LoggerMessage(208, LogLevel.Debug, "Unable to acquire lock '{LockName}' for '{ExecutionId}'. Likely an ongoing execution.")]
    public static partial void UnableToAcquireDistributedLock(this ILogger logger, string lockName, string executionId);

    [LoggerMessage(209, LogLevel.Debug, "Distributed lock '{LockName}' acquired for '{ExecutionId}'")]
    public static partial void AcquiredDistributedLock(this ILogger logger, string lockName, string executionId);

    #endregion
}
