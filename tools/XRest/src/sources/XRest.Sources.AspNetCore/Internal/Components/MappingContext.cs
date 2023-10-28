using Annium.Net.Types;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace XRest.Sources.AspNetCore.Internal.Components;

internal sealed record MappingContext(IModelMapper Mapper, IMapperConfig Config)
{
    public IRef Map(ContextualType type)
    {
        return Mapper.Map(type);
    }
}
