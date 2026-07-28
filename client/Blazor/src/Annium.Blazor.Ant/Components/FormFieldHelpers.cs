using System;
using Annium.Components.State.Forms;
using Annium.Core.Mapper;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Ant.Components;

/// <summary>
/// Shared helpers for the Ant form-field components.
/// </summary>
internal static class FormFieldHelpers
{
    /// <summary>
    /// Converts the raw input value to <typeparamref name="TValue"/> via the mapper and stores it in the state
    /// container. Invalid input is ignored, leaving the previous value in place (exception-free input contract).
    /// </summary>
    /// <typeparam name="TValue">The value type managed by the state container.</typeparam>
    /// <param name="state">The state container to update.</param>
    /// <param name="mapper">The mapper used to convert the raw input value.</param>
    /// <param name="args">The change event arguments carrying the raw input value.</param>
    public static void TrySetMappedValue<TValue>(IAtomicContainer<TValue> state, IMapper mapper, ChangeEventArgs args)
        where TValue : IEquatable<TValue>
    {
        try
        {
            state.Set(mapper.Map<TValue>(args.Value!));
        }
        catch
        {
            // ignored: invalid input keeps the previous value
        }
    }
}
