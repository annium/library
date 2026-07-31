using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.XRest.Clients.Csharp.Extensions;
using Annium.XRest.Clients.Csharp.Views.Api;
using Annium.XRest.Clients.Csharp.Views.Client;

namespace Annium.XRest.Clients.Csharp.Components.Processors;

internal static class ClientBuilder
{
    private static readonly Namespace _systemThreadingNamespace = Constants.SystemThreadingNamespace.ToNamespace();
    private static readonly Namespace _systemThreadingTasksNamespace =
        Constants.SystemThreadingTasksNamespace.ToNamespace();
    private static readonly Namespace _anniumNetHttpNamespace = Constants.AnniumNetHttpNamespace.ToNamespace();

    public static IClientView BuildClient(
        Namespace clientsNamespace,
        string type,
        string name,
        IReadOnlyCollection<ControllerView> controllers
    )
    {
        if (controllers.Count == 0)
            throw new ArgumentException("Can't build container without clients");

        if (controllers.Count == 1)
            return new ClientContainerView(
                new[] { controllers.First().Namespace, _anniumNetHttpNamespace }
                    .ToUsagesFrom(clientsNamespace)
                    .ToUsageStrings(),
                clientsNamespace.ToString(),
                name,
                type,
                controllers.Select(BuildClientNode).ToArray()
            );

        var lookup = controllers.ToLookup(x => x.Namespace == clientsNamespace);

        var childControllers = lookup[true].ToArray();

        // the container type lives in the namespace it is named after, so naming it after the bare
        // last segment ("Admin" inside "…Clients.Admin") makes the reference from the parent resolve
        // to the namespace instead of the type — CS0118. The "Root" suffix keeps the two distinct.
        var ancestors = lookup[false]
            .GroupBy(x => x.Namespace)
            .ToDictionary(x => x.Key, x => BuildClient(x.Key, $"{x.Key[^1]}Root", $"{x.Key[^1]}Root", x.ToArray()));

        var usages = childControllers
            .Select(x => x.Namespace)
            .Append(_anniumNetHttpNamespace)
            .Concat(ancestors.Keys)
            .ToUsagesFrom(clientsNamespace)
            .ToUsageStrings();

        var children = childControllers.Select(BuildClientNode).OrderBy(x => x.Namespace.ToString()).ToArray();

        var clients = ancestors.Values.OrderBy(x => x.Namespace.ToString()).Concat(children).ToArray();

        return new ClientContainerView(usages, clientsNamespace.ToString(), name, type, clients);
    }

    private static IClientView BuildClientNode(ControllerView controller)
    {
        var usages = controller
            .Usages.Concat([_systemThreadingNamespace, _systemThreadingTasksNamespace, _anniumNetHttpNamespace])
            .ToUsagesFrom(controller.Namespace)
            .ToUsageStrings();
        var @namespace = controller.Namespace.ToString();
        var name = controller.Name;
        var type = $"{controller.Name}Client";
        var actions = controller.Actions;

        return new ClientView(usages, @namespace, name, type, actions);
    }
}
