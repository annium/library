using System;
using System.Collections.Generic;
using Annium.Blazor.Charts.Components;
using Annium.Blazor.Charts.Domain.Contexts;
using Annium.Testing;
using Microsoft.AspNetCore.Components;
using NodaTime;
using OneOf;
using Xunit;

namespace Annium.Blazor.Charts.Tests.Components;

/// <summary>
/// Tests for LabelBase.OnParametersSet's required-parameter validation: either Left or Right must be specified,
/// and either Top or Bottom must be specified
/// </summary>
public class LabelBaseTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the LabelBaseTests class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public LabelBaseTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddCharts());
    }

    /// <summary>
    /// Tests that OnParametersSet throws when neither Left nor Right is specified, even if Top is specified
    /// </summary>
    [Fact]
    public void OnParametersSet_LeftAndRightBothNull_Throws()
    {
        // arrange
        var label = CreateLabel(top: 0);

        // act & assert
        Wrap.It(() => label.TriggerOnParametersSet()).Throws<ArgumentException>().ReportsAll(["Left", "Right"]);
    }

    /// <summary>
    /// Tests that OnParametersSet throws when neither Top nor Bottom is specified, even if Left is specified
    /// </summary>
    [Fact]
    public void OnParametersSet_TopAndBottomBothNull_Throws()
    {
        // arrange
        var label = CreateLabel(left: 0);

        // act & assert
        Wrap.It(() => label.TriggerOnParametersSet()).Throws<ArgumentException>().ReportsAll(["Top", "Bottom"]);
    }

    /// <summary>
    /// Tests that OnParametersSet does not throw when Left and Top are both specified
    /// </summary>
    [Fact]
    public void OnParametersSet_LeftAndTopSpecified_DoesNotThrow()
    {
        // arrange
        var label = CreateLabel(left: 0, top: 0);

        // act
        label.TriggerOnParametersSet();

        // assert - no exception thrown
        true.IsTrue();
    }

    /// <summary>
    /// Tests that OnParametersSet does not throw when Right and Bottom are both specified
    /// </summary>
    [Fact]
    public void OnParametersSet_RightAndBottomSpecified_DoesNotThrow()
    {
        // arrange
        var label = CreateLabel(right: 0, bottom: 0);

        // act
        label.TriggerOnParametersSet();

        // assert - no exception thrown
        true.IsTrue();
    }

    /// <summary>
    /// Creates an ExposedLabel and applies the given parameter values via ParameterView, the same mechanism Blazor
    /// uses internally to set [Parameter] properties, avoiding a direct out-of-component property assignment
    /// </summary>
    /// <param name="left">The Left position parameter to apply, or null to leave it unset</param>
    /// <param name="right">The Right position parameter to apply, or null to leave it unset</param>
    /// <param name="top">The Top position parameter to apply, or null to leave it unset</param>
    /// <param name="bottom">The Bottom position parameter to apply, or null to leave it unset</param>
    /// <returns>A configured ExposedLabel instance</returns>
    private static ExposedLabel<object> CreateLabel(
        OneOf<
            int,
            Func<object, int>,
            Func<IPaneContext, Instant, int>,
            Func<IPaneContext, Instant, object, int>
        >? left = null,
        OneOf<
            int,
            Func<object, int>,
            Func<IPaneContext, Instant, int>,
            Func<IPaneContext, Instant, object, int>
        >? right = null,
        OneOf<int, Func<object, int>, Func<IPaneContext, object, int>>? top = null,
        OneOf<int, Func<object, int>, Func<IPaneContext, object, int>>? bottom = null
    )
    {
        var values = new Dictionary<string, object?>();
        if (left is not null)
            values["Left"] = left;
        if (right is not null)
            values["Right"] = right;
        if (top is not null)
            values["Top"] = top;
        if (bottom is not null)
            values["Bottom"] = bottom;

        var label = new ExposedLabel<object>();
        ParameterView.FromDictionary(values).SetParameterProperties(label);

        return label;
    }

    /// <summary>
    /// A test subclass of LabelBase exposing the protected OnParametersSet method for direct testing
    /// </summary>
    /// <typeparam name="T">The data item type associated with the label</typeparam>
    private sealed class ExposedLabel<T> : LabelBase<T>
    {
        /// <summary>
        /// Invokes the protected OnParametersSet method
        /// </summary>
        public void TriggerOnParametersSet() => OnParametersSet();
    }
}
