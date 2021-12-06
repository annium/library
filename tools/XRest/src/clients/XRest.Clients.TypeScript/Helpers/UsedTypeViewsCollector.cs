using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using XRest.Clients.TypeScript.Views;
using XRest.Clients.TypeScript.Views.Types;

namespace XRest.Clients.TypeScript.Helpers;

internal class UsedTypeViewsCollector
{
    public IReadOnlyCollection<DefinedTypeView> CollectUsedTypeViews(
        IReadOnlyCollection<DefinedTypeView> exports,
        IReadOnlyCollection<ActionView> actions
    )
    {
        var registry = new Registry();

        foreach (var export in exports)
            CollectTypes(export, registry, true);

        foreach (var action in actions)
            CollectionActionTypes(action, registry);

        return registry.Items;
    }

    private void CollectionActionTypes(ActionView action, Registry registry)
    {
        foreach (var parameter in action.Parameters)
            CollectTypes(parameter.Type, registry, false);
        if (action.HasBody)
            CollectTypes(action.Body!, registry, false);
        CollectTypes(action.Response, registry, false);
    }

    private void CollectTypes(DefinedTypeView view, Registry registry, bool collectProperties)
    {
        var definition = view switch
        {
            ClassView cv => cv.IsGenericType ? cv.GetGenericDefinition() : cv,
            _            => view,
        };
        var isBuiltIn = KnownTypes.BuiltIn.Contains(definition);
        var isRegistered = !isBuiltIn && registry.Register(definition);

        if (view is ClassView classView)
        {
            if (classView.IsGenericType && (isBuiltIn || isRegistered))
                CollectGenericArgumentsTypes(classView, registry);
            if (!isBuiltIn && isRegistered && collectProperties)
                CollectPropertiesTypes(classView, registry);
        }
    }

    private void CollectGenericArgumentsTypes(ClassView x, Registry registry)
    {
        foreach (var genericArgument in x.GenericArguments)
            CollectTypes(genericArgument, registry, false);
    }

    private void CollectPropertiesTypes(ClassView x, Registry registry)
    {
        foreach (var property in x.GetPropertyTypes())
            CollectTypes(property, registry, false);
    }

    private class Registry
    {
        public IReadOnlyCollection<DefinedTypeView> Items => _set.ToArray();

        private readonly HashSet<DefinedTypeView> _set = new HashSet<DefinedTypeView>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Register(DefinedTypeView view) => _set.Add(view);
    }
}