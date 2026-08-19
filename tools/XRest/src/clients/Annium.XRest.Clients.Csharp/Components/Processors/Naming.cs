using System.Collections.Generic;

namespace Annium.XRest.Clients.Csharp.Components.Processors;

internal static class Naming
{
    /// <summary>
    /// Takes a name within a scope: the preferred one when it is free, and otherwise the first
    /// numbered variant that is. Every set of generated identifiers that share a scope — a
    /// controller's actions, a container's clients, a call's parameters — resolves collisions against
    /// the whole set this way, rather than only against the one case that first produced a defect.
    /// </summary>
    /// <param name="preferred">The name to use when nothing has taken it.</param>
    /// <param name="taken">The names already given out in this scope; the result is added to it.</param>
    /// <returns>A name unique within the scope.</returns>
    public static string Take(string preferred, HashSet<string> taken)
    {
        var name = preferred;
        for (var index = 2; !taken.Add(name); index++)
            name = $"{preferred}{index}";

        return name;
    }
}
