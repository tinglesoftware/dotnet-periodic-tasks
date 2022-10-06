namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Builds conventions that will be used for customization of periodic tasks <see cref="EndpointBuilder"/> instances.
/// </summary>
internal class PeriodicTasksEndpointConventionBuilder : IEndpointConventionBuilder
{
    private readonly List<IEndpointConventionBuilder> builders;

    internal PeriodicTasksEndpointConventionBuilder(IEnumerable<IEndpointConventionBuilder> builders)
    {
        this.builders = builders.ToList();
    }

    /// <summary>
    /// Adds the specified convention to the builder. Conventions are used to customize <see cref="EndpointBuilder"/> instances.
    /// </summary>
    /// <param name="convention">The convention to add to the builder.</param>
    public void Add(Action<EndpointBuilder> convention)
    {
        foreach (var builder in builders)
        {
            builder.Add(convention);
        }
    }
}
