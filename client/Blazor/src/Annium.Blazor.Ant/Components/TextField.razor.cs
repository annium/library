using System;
using System.Collections.Generic;
using Annium.Components.State;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Ant.Components
{
    public partial class TextField<TValue> where TValue : IEquatable<TValue>
    {
        [Parameter]
        public IAtomicContainer<TValue> State { get; set; } = default!;

        [Parameter]
        public RenderFragment? ChildContent { get; set; }


        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> Attributes { get; set; } = default!;
    }
}