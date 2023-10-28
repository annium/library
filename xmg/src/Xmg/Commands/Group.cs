using Annium.Extensions.Arguments;

namespace Xmg.Commands;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "xmg";

    public static string Description => "Db migration tool";

    public Group()
    {
        Add<GenerateCommand>();
        Add<ParseCommand>();
    }
}
