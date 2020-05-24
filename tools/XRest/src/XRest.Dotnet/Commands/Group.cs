namespace XRest.Dotnet.Commands
{
    public class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "dotnet";

        public override string Description { get; } = ".NET commands";

        public Group()
        {
            Add<GenerateCommand>();
        }
    }
}