using System;
using Annium.Core.Mapper;
using Annium.Core.Primitives;

namespace Annium.Blazor.Routing.Internal.Implementations.Locations.Segments;

internal sealed record ParamLocationSegment(string Name, Type type) : ILocationSegment
{
    public object? Match(string segment, IMapper mapper)
    {
        try
        {
            return mapper.Map(segment, type);
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

    public override string ToString() => $"{Name}:{type.FriendlyName()}";
}