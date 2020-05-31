using System.Collections.Generic;

namespace XRest.Clients.Dotnet.Views
{
    internal class ClientView : IClientView
    {
        public IReadOnlyCollection<string> Usages { get; }
        public string Namespace { get; }
        public string Name { get; }
        public string Type { get; }
        public IReadOnlyCollection<ActionView> Actions { get; }

        public ClientView(
            IReadOnlyCollection<string> usages,
            string ns,
            string name,
            string type,
            IReadOnlyCollection<ActionView> actions
        )
        {
            Usages = usages;
            Namespace = ns;
            Name = name;
            Type = type;
            Actions = actions;
        }

        public override string ToString() => Name;
    }
}