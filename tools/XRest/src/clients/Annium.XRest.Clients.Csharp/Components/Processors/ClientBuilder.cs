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
                BuildClientNodes(controllers, type)
            );

        var lookup = controllers.ToLookup(x => x.Namespace == clientsNamespace);

        var childControllers = lookup[true].ToArray();

        // the container type lives in the namespace it is named after, so naming it after the bare
        // last segment ("Admin" inside "…Clients.Admin") makes the reference from the parent resolve
        // to the namespace instead of the type — CS0118. The "Root" suffix keeps the two distinct.
        // The whole path below the client root goes into the name, not just that last segment: the
        // grouping is by full namespace, so two branches ending alike (Admin.Items, Public.Items)
        // would otherwise both be `ItemsRoot` and collide on the parent — CS0102 and CS0104.
        var branches = lookup[false].GroupBy(x => x.Namespace).ToArray();
        var names = GetBranchNames(clientsNamespace, branches.Select(x => x.Key));

        var ancestors = branches.ToDictionary(
            x => x.Key,
            x => BuildClient(x.Key, names[x.Key], names[x.Key], x.ToArray())
        );

        var usages = childControllers
            .Select(x => x.Namespace)
            .Append(_anniumNetHttpNamespace)
            .Concat(ancestors.Keys)
            .ToUsagesFrom(clientsNamespace)
            .ToUsageStrings();

        var children = BuildClientNodes(childControllers, type, names.Values)
            .OrderBy(x => x.Namespace.ToString())
            .ToArray();

        var clients = ancestors.Values.OrderBy(x => x.Namespace.ToString()).Concat(children).ToArray();

        return new ClientContainerView(usages, clientsNamespace.ToString(), name, type, clients);
    }

    /// <summary>
    /// Names the container of each namespace branch below the client root. The whole path goes into
    /// the name, not just its last segment, so branches ending alike stay apart.
    /// </summary>
    /// <param name="clientsNamespace">The namespace the containers hang off.</param>
    /// <param name="branches">The namespaces to name.</param>
    /// <returns>The container name of each branch.</returns>
    public static IReadOnlyDictionary<Namespace, string> GetBranchNames(
        Namespace clientsNamespace,
        IEnumerable<Namespace> branches
    )
    {
        var names = branches
            .Distinct()
            .ToDictionary(x => x, x => $"{string.Concat(x.Skip(clientsNamespace.Count))}Root");

        // concatenation loses the segment boundaries, so `A.BC` and `AB.C` both read `ABCRoot`; where
        // that happens the boundaries come back as underscores, and only there, so ordinary names stay
        // as they are
        foreach (var collision in names.GroupBy(x => x.Value).Where(x => x.Count() > 1).SelectMany(x => x))
            names[collision.Key] = $"{string.Join('_', collision.Key.Skip(clientsNamespace.Count))}Root";

        return names;
    }

    private static IReadOnlyList<IClientView> BuildClientNodes(
        IReadOnlyCollection<ControllerView> controllers,
        string containerType,
        IEnumerable<string>? containerNames = null
    )
    {
        // the container's own type, and its nested containers, are declared in the same scope as these
        // clients, so all of them compete for the same names
        var taken = new HashSet<string>(containerNames ?? []) { containerType };

        return controllers.Select(x => BuildClientNode(x, containerType, taken)).ToArray();
    }

    private static IClientView BuildClientNode(ControllerView controller, string containerType, HashSet<string> taken)
    {
        var usages = controller
            .Usages.Concat([_systemThreadingNamespace, _systemThreadingTasksNamespace, _anniumNetHttpNamespace])
            .ToUsagesFrom(controller.Namespace)
            .ToUsageStrings();
        var @namespace = controller.Namespace.ToString();
        // the controller becomes a property of its container, and a member cannot share the name of
        // the type that declares it — a RootController at the API root, or an AdminRootController
        // under Admin, otherwise emitted `public XClient X => …` inside `class X` (CS0542)
        var type = Naming.Take($"{controller.Name}Client", taken);
        var name = Naming.Take(controller.Name == containerType ? type : controller.Name, taken);
        var actions = controller.Actions;

        return new ClientView(usages, @namespace, name, type, actions);
    }
}
