using System.Collections.Generic;

namespace Annium.Components.State.Forms.Internal;

/// <summary>
/// Internal capability exposing a composite container's child states keyed by the path segment a validator uses
/// for them — the property name for objects, the index for arrays, the key for maps. Used to route dotted-path
/// validation errors (e.g. <c>Author.Name</c>) into the matching nested child, recursively.
/// </summary>
internal interface INamedChildStates
{
    /// <summary>
    /// Gets the child tracked states keyed by their validation path segment.
    /// </summary>
    IEnumerable<KeyValuePair<string, ITrackedState>> NamedChildren { get; }
}
