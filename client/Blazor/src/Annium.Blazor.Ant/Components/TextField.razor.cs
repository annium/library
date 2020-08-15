using System;
using System.Collections.Generic;
using Annium.Components.State;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Ant.Components
{
    public partial class TextField<TValue> where TValue : IEquatable<TValue>
    {
        [CascadingParameter]
        public IFormField<TValue> FormField { get; set; } = default!;

        [Parameter]
        public IAtomicContainer<TValue> State { get; set; } = default!;

        [Parameter]
        public RenderFragment? ChildContent { get; set; }


        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> Attributes { get; set; } = default!;

        [Parameter]
        public string? InputClass { get; set; }

        private IAtomicContainer<TValue> InternalState => State ?? FormField.State;
    }
}