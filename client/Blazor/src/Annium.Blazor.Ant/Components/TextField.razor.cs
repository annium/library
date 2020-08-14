using System;
using System.Collections.Generic;
using Annium.Blazor.Core.Tools;
using Annium.Components.State;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Ant.Components
{
    public partial class TextField<TValue> where TValue : IEquatable<TValue>
    {
        [CascadingParameter]
        public IFormItem<TValue> FormItem { get; set; } = default!;

        [Parameter]
        public IAtomicContainer<TValue> State { get; set; } = default!;

        [Parameter]
        public RenderFragment? ChildContent { get; set; }


        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> Attributes { get; set; } = default!;

        [Parameter]
        public string? InputClass { get; set; }

        private IAtomicContainer<TValue> InternalState => State ?? FormItem.State;
        private string ClassName => _classBuilder.Build();
        private string ErrorClassName { get; set; } = default!;

        private readonly ClassBuilder _classBuilder;

        public TextField()
        {
            _classBuilder = new ClassBuilder()
                .With(() => InternalState.HasStatus(Status.Error), () => ErrorClassName);
        }
    }
}