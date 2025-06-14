using System;
using System.Collections.Generic;
using Annium.Blazor.Core.Tools;
using Annium.Components.State.Forms;
using Microsoft.AspNetCore.Components;

namespace Annium.Blazor.MatBlazor.Components
{
    /// <summary>
    /// A Material Design text field component that provides state management and validation styling.
    /// </summary>
    /// <typeparam name="TValue">The type of value managed by the text field.</typeparam>
    public partial class TextField<TValue>
        where TValue : IEquatable<TValue>
    {
        /// <summary>
        /// Gets or sets the atomic container that manages the text field state.
        /// </summary>
        [Parameter]
        public IAtomicContainer<TValue> State { get; set; } = null!;

        /// <summary>
        /// Gets or sets additional CSS classes to apply to the text field component.
        /// </summary>
        [Parameter]
        public string? Class { get; set; }

        /// <summary>
        /// Gets or sets additional HTML attributes to apply to the text field element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> Attributes { get; set; } = null!;

        /// <summary>
        /// Gets the complete CSS class name for the text field, including error state styling.
        /// </summary>
        public string ClassName => _classBuilder.Clone().With(Class ?? string.Empty).Build();

        /// <summary>
        /// The class builder used to construct CSS classes based on component state.
        /// </summary>
        private readonly IClassBuilder _classBuilder;

        /// <summary>
        /// Initializes a new instance of the TextField class.
        /// </summary>
        public TextField()
        {
            _classBuilder = ClassBuilder.With(() => State.IsStatus(Status.Error), "mdc-text-field--invalid");
        }
    }
}
