namespace Xc.Commands
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "xc";

        public override string Description { get; } = "Configuration manager";

        public Group()
        {
            Add<CleanCommand>();
            Add<ConfigureCommand>();
            Add<ShowCommand>();
            Add<VerifyCommand>();
        }
    }
}