using Annium.Core.Runtime.Types;
using Annium.Net.Types;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace XRest.Plugins.AspNetCore.Internal.Components;

internal sealed record MappingContext(
    IModelMapper Mapper
)
{
    public IRef Map(ContextualType type)
    {
        return Mapper.Map(type);
    }
}