// ReSharper disable once CheckNamespace

namespace Annium.Blazor.Interop;

/// <summary>
/// Represents a JavaScript object that can be identified by an ID
/// </summary>
public interface IObject
{
    /// <summary>
    /// Gets the unique identifier for this JavaScript object
    /// </summary>
    string Id { get; }
}
