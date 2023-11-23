using Microsoft.AspNetCore.Builder;

namespace Tingle.PeriodicTasks.AspNetCore;

/// <summary>
/// The request type for the "/execute" endpoint added by <see cref="PeriodicTasksEndpointRouteBuilderExtensions.MapPeriodicTasks"/>.
/// </summary>
public class PeriodicTaskExecutionRequest
{
    /// <summary>
    /// The name of the registered task to execute.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Whether to await execution to complete.
    /// </summary>
    public bool Wait { get; set; }

    /// <summary>
    /// Whether to throw an exception if one is encountered.
    /// </summary>
    public bool Throw { get; set; }
}
