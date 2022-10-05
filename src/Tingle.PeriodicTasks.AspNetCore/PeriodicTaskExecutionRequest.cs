using System.ComponentModel.DataAnnotations;

namespace Tingle.PeriodicTasks.AspNetCore;

internal class PeriodicTaskExecutionRequest
{
    /// <summary>
    /// The name of the registered task to execute.
    /// </summary>
    [Required]
    public string? Name { get; set; }

    /// <summary>
    /// Whether to await execution to complete.
    /// </summary>
    public bool Wait { get; set; }

    /// <summary>
    /// Whether to throw an exception if one is encountered.
    /// </summary>
    public bool Throw { get; set; }
}
