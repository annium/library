namespace XRest.Commands
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "xrest";

        public override string Description { get; } = "REST client generator";

        public Group()
        {
            Add<Clients.Dotnet.Commands.Group>();
            Add<Clients.TypeScript.Commands.Group>();
            Add<ParseCommand>();
        }
    }
}