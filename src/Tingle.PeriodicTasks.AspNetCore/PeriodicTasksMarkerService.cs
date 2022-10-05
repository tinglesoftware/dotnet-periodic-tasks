using Microsoft.Extensions.DependencyInjection;

namespace Tingle.PeriodicTasks.AspNetCore;

/// <summary>
/// A marker class used to determine if all the required periodic services were added
/// to the <see cref="IServiceCollection"/>.
/// </summary>
internal class PeriodicTasksMarkerService
{
}
