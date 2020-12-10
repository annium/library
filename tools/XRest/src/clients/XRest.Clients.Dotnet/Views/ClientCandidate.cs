using System.Collections.Generic;
using System.Linq;
using XRest.Core.Models;

namespace XRest.Clients.Dotnet.Views
{
    internal class ClientCandidate
    {
        public IReadOnlyCollection<Namespace> Usages { get; }
        public Namespace Namespace { get; }
        public string Name { get; }
        public string Type { get; }
        public IReadOnlyCollection<ActionView> Actions { get; }

        public ClientCandidate(
            IReadOnlyCollection<Namespace> usages,
            Namespace ns,
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

        public static explicit operator ClientView(ClientCandidate x) =>
            new ClientView(x.Usages.Select(y => y.ToString()).ToArray(), x.Namespace.ToString(), x.Name, x.Type, x.Actions);
    }
}