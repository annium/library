using Annium.Testing;
using XRest.Core.Extensions;
using XRest.Core.Helpers;
using XRest.Core.Models.Types;
using Xunit;

namespace XRest.Core.Tests.Helpers;

public class TypeHelperTests
{
    [Fact]
    public void BaseType()
    {
        // act
        var model = TypeHelper.GetTypeModel<int>();

        // assert
        model.As<StructModel>().Namespace.Is(typeof(int).GetNamespace());
        model.As<StructModel>().Name.Is("int");
    }
}