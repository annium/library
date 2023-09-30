using System;
using Annium.Core.Mapper;

namespace Annium.Blazor.Routing.Internal.Locations.Segments;

internal sealed record ParamLocationSegment(string Name, Type Type) : ILocationSegment
{
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

    public override string ToString() => $"{Name}:{Type.FriendlyName()}";
}