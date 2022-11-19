namespace Microsoft.Extensions.Logging;

internal static partial class ILoggerExtensions
{
    #region Host

    [LoggerMessage(401, LogLevel.Warning, "Periodic Task with name: '{Name}' could not be found.")]
    public static partial void PeriodicTaskNotFound(this ILogger logger, string name);

    #endregion
}
