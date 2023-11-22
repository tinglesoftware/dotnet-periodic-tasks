using System.Diagnostics.CodeAnalysis;

namespace Tingle.PeriodicTasks.Internal;

internal static class TrimmingHelper
{
    internal const DynamicallyAccessedMemberTypes Task = DynamicallyAccessedMemberTypes.PublicConstructors;
    internal const DynamicallyAccessedMemberTypes Runner = DynamicallyAccessedMemberTypes.PublicConstructors;
}
