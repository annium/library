using System;
using System.Collections.Generic;
using Annium.Components.State.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Annium.Blazor.Ant.Components
{
    public partial class TextField<TValue> where TValue : IEquatable<TValue>
    {
        [CascadingParameter]
        public IFormField<TValue> FormField { get; set; } = default!;

        [Parameter]
        public IAtomicContainer<TValue> State { get; set; } = default!;

        [Parameter]
        public EventCallback<KeyboardEventArgs> OnPressEnter { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> Attributes { get; set; } = default!;

        private TValue Value => InternalState.Value;

        private void SetValue(ChangeEventArgs args)
        {
            try
            {
                InternalState.Set(mapper.Map<TValue>(args.Value!));
            }
            catch
            {
            }
        }

        private IAtomicContainer<TValue> InternalState => State ?? FormField.State;
    }
}