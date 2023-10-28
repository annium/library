using Annium.Extensions.Arguments;

namespace Xc.Commands;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "xc";

    public static string Description => "Configuration manager";

    public Group()
    {
        Add<CleanCommand>();
        Add<ConfigureCommand>();
        Add<ShowCommand>();
        Add<VerifyCommand>();
    }
}
