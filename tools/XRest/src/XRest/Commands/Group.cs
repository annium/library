namespace XRest.Commands
{
    internal class Group : Annium.Extensions.Arguments.Group
    {
        public override string Id { get; } = "xrest";

        public override string Description { get; } = "REST client generator";

        public Group()
        {
            Add<ParseCommand>();
        }
    }
}