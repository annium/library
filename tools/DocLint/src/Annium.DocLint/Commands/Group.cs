using Annium.Extensions.Arguments;

namespace Annium.DocLint.Commands;

internal class Group : Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "doclint";

    public static string Description => ".NET documentation validator";

    public Group()
    {
        Add<LintCommand>();
    }
}
