using System;
using Annium.Core.Mapper;

namespace Annium.Blazor.Routing.Internal.Locations.Segments;

/// <summary>
/// Represents a parameterized location segment that can match and convert URL segments to typed values
/// </summary>
/// <param name="Name">The name of the parameter</param>
/// <param name="Type">The type that the parameter should be converted to</param>
internal sealed record ParamLocationSegment(string Name, Type Type) : ILocationSegment
{
    /// <summary>
    /// Attempts to match and convert a URL segment to the parameter's target type
    /// </summary>
    /// <param name="segment">The URL segment to match and convert</param>
    /// <param name="mapper">The mapper instance to use for type conversion</param>
    /// <returns>The converted value if successful; otherwise, null</returns>
    public object? Match(string segment, IMapper mapper)
    {
        try
        {
            return mapper.Map(segment, Type);
        }
        catch (MappingException)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Returns the string representation of this parameter location segment
    /// </summary>
    /// <returns>A string in the format "Name:TypeName"</returns>
    public override string ToString() => $"{Name}:{Type.FriendlyName()}";
}
