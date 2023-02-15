using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using XRest.Clients.Csharp.Extensions;
using XRest.Clients.Csharp.Views;

namespace XRest.Clients.Csharp.Components.Processors;

internal static class ClientBuilder
{
    private static readonly Namespace TplNamespace = Constants.TplNamespace.ToNamespace();
    private static readonly Namespace HttpNamespace = Constants.NetHttpNamespace.ToNamespace();

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
                new[] { controllers.First().Namespace, HttpNamespace }.ToUsagesFrom(clientsNamespace).ToUsageStrings(),
                clientsNamespace.ToString(),
                name,
                type,
                controllers.Select(BuildClientNode).ToArray()
            );

        var lookup = controllers.ToLookup(x => x.Namespace == clientsNamespace);

        var childControllers = lookup[true].ToArray();

        var ancestors = lookup[false]
            .GroupBy(x => x.Namespace)
            .ToDictionary(
                x => x.Key,
                x => BuildClient(x.Key, x.Key[^1], $"{x.Key[^1]}Root", x.ToArray())
            );

        var usages = childControllers
            .Select(x => x.Namespace)
            .Append(HttpNamespace)
            .Concat(ancestors.Keys)
            .ToUsagesFrom(clientsNamespace)
            .ToUsageStrings();

        var children = childControllers
            .Select(BuildClientNode)
            .OrderBy(x => x.Namespace.ToString())
            .ToArray();

        var clients = ancestors.Values
            .OrderBy(x => x.Namespace.ToString())
            .Concat(children)
            .ToArray();

        return new ClientContainerView(usages, clientsNamespace.ToString(), name, type, clients);
    }

    private static IClientView BuildClientNode(ControllerView controller)
    {
        var usages = controller.Usages
            .Concat(new[] { TplNamespace, HttpNamespace })
            .ToUsagesFrom(controller.Namespace)
            .ToUsageStrings();
        var @namespace = controller.Namespace.ToString();
        var name = controller.Name;
        var type = $"{controller.Name}Client";
        var actions = controller.Actions;

        return new ClientView(usages, @namespace, name, type, actions);
    }
}