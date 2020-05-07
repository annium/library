namespace Xmg.Commands
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "xmg";

        public override string Description { get; } = "Db migration tool";

        public Group()
        {
            Add<GenerateCommand>();
            Add<ParseCommand>();
        }
    }
}