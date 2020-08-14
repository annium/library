using System;
using Annium.Blazor.Core.Tools;
using Annium.Components.State;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.Ant.Components
{
    public partial class FormItem<TValue> : IFormItem<TValue>
        where TValue : IEquatable<TValue>
    {
        [Parameter]
        public IAtomicContainer<TValue> State { get; set; } = default!;

        [Parameter]
        public RenderFragment ChildContent { get; set; } = default!;

        [Parameter]
        public string? WrapperClass { get; set; }

        private string WrapperClassName => _wrapperClassBuilder.Clone().With(WrapperClass ?? string.Empty).Build();
        private readonly ClassBuilder _wrapperClassBuilder;

        public FormItem()
        {
            _wrapperClassBuilder = new ClassBuilder()
                .With("ant-form-item")
                .With(() => State.HasStatus(Status.Error), "ant-form-item-has-error");
        }
    }
}