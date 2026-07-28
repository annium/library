using System;
using Annium.Testing;
using Xunit;

namespace Annium.Blazor.Css.Tests;

/// <summary>
/// Tests for <see cref="Rule"/> factory selectors and <see cref="CssRule"/> rendering (ToCss / Inline), the
/// implicit string conversions, and the flexbox fluent extensions. These tests are isolated — they use only
/// <see cref="Rule"/> / <see cref="CssRule"/> and never instantiate a <see cref="RuleSet"/>, so they do not touch
/// the process-global <c>StyleSheet.Instance</c> singleton.
///
/// CSS rendering differs by build configuration (<c>#if DEBUG</c>): DEBUG is pretty-printed (<c>"k: v;"</c>,
/// <c>"sel {"</c>, <c>" &gt; "</c> child combinator, empty rules kept); RELEASE is minified (<c>"k:v;"</c>,
/// <c>"sel{"</c>, <c>"&gt;"</c>, empty rules skipped). The suite runs under both configs (devs Debug, CI
/// <c>just test -c Release</c>), so format-sensitive assertions are config-gated via the constants below.
/// </summary>
public class CssRuleTest
{
#if DEBUG
    /// <summary>Property key/value separator in rendered CSS.</summary>
    private const string P = ": ";

    /// <summary>Selector-to-open-brace separator in rendered CSS.</summary>
    private const string O = " {";

    /// <summary>Child (&gt;) combinator as rendered.</summary>
    private const string C = " > ";
#else
    /// <summary>Property key/value separator in rendered CSS.</summary>
    private const string P = ":";

    /// <summary>Selector-to-open-brace separator in rendered CSS.</summary>
    private const string O = "{";

    /// <summary>Child (&gt;) combinator as rendered.</summary>
    private const string C = ">";
#endif

    /// <summary>
    /// Tests that each Rule factory produces the correct selector shape (class '.', id '#', tag, tag+class,
    /// tag+id, and verbatim custom). Selector text is config-independent.
    /// </summary>
    [Fact]
    public void Rule_Factory_SelectorShapes()
    {
        Rule.Class("a").ToString().Is(".a");
        Rule.Id("a").ToString().Is("#a");
        Rule.Tag("div").ToString().Is("div");
        Rule.TagClass("div", "a").ToString().Is("div.a");
        Rule.TagId("div", "a").ToString().Is("div#a");
        Rule.Custom("div>span").ToString().Is("div>span");
    }

    /// <summary>
    /// Tests the implicit conversions to string: a <see cref="CssRule"/> converts to its short Name (last segment
    /// after '.'/'#'), and an <see cref="Internal.ImplicitString{T}"/> value (e.g. a <see cref="FlexDirection"/>)
    /// converts to its CSS token.
    /// </summary>
    [Fact]
    public void ImplicitString_Conversions()
    {
        // CssRule -> Name
        string className = Rule.Class("foo");
        className.Is("foo");
        string tagClassName = Rule.TagClass("div", "bar");
        tagClassName.Is("bar");

        // ImplicitString<T> -> CSS token
        string direction = FlexDirection.RowReverse;
        direction.Is("row-reverse");
        string align = AlignItems.FlexStart;
        align.Is("flex-start");
    }

    /// <summary>
    /// Tests that ToCss renders the rule, its properties, and every nesting kind with the correct selector join:
    /// And appends with no separator, Child prefixes the child combinator, Inheritor prefixes a space, and Media
    /// wraps in an <c>@media</c> block.
    /// </summary>
    [Fact]
    public void CssRule_ToCss_RendersNestingAndMedia()
    {
        // arrange: a top-level rule with a property and one of each nested kind + a media query
        var rule = Rule.Custom(".a");
        rule.Set("color", "red");
        rule.And(".b", r => r.Set("x", "1"));
        rule.Child("span", r => r.Set("y", "2"));
        rule.Inheritor("em", r => r.Set("z", "3"));
        rule.Media("(min-width: 1px)", r => r.Set("w", "4"));

        // act
        var css = rule.ToCss();

        // assert: own rule + property
        css.Contains($".a{O}").IsTrue();
        css.Contains($"color{P}red;").IsTrue();
        // And: appended with no separator
        css.Contains($".a.b{O}").IsTrue();
        css.Contains($"x{P}1;").IsTrue();
        // Child: child-combinator prefix
        css.Contains($".a{C}span{O}").IsTrue();
        css.Contains($"y{P}2;").IsTrue();
        // Inheritor: single-space prefix (config-independent)
        css.Contains($".a em{O}").IsTrue();
        css.Contains($"z{P}3;").IsTrue();
        // Media: @media wrapper — the wrapped rule must retain its own selector inside the block
        css.Contains($"@media (min-width: 1px){O}").IsTrue();
        var mediaIndex = css.IndexOf("@media", StringComparison.Ordinal);
        (mediaIndex >= 0).IsTrue();
        var mediaBlock = css[mediaIndex..];
        mediaBlock.Contains($".a{O}").IsTrue();
        mediaBlock.Contains($"w{P}4;").IsTrue();
    }

    /// <summary>
    /// Tests the build-config-specific handling of rules with no properties: DEBUG keeps them (pretty output),
    /// RELEASE skips them (minified output omits empty selectors and empty nested rules).
    /// </summary>
    [Fact]
    public void CssRule_ToCss_EmptyRuleHandling()
    {
        // fully-empty top-level rule
        var empty = Rule.Custom(".empty");

        // parent with a property but an empty nested child
        var parent = Rule.Custom(".p");
        parent.Set("color", "red");
        parent.And(".child", _ => { });
        var parentCss = parent.ToCss();

        // parent's own populated rule always renders
        parentCss.Contains($".p{O}").IsTrue();
#if DEBUG
        empty.ToCss().Contains(".empty").IsTrue();
        parentCss.Contains(".p.child").IsTrue();
#else
        empty.ToCss().Is(string.Empty);
        parentCss.Contains(".p.child").IsFalse();
#endif

        // wrapper rule: no OWN properties but a populated nested child must still render in BOTH configs
        // (pins the RELEASE skip guard's compound `&&` — a `||` mutant would drop the whole subtree)
        var wrapper = Rule.Custom(".w");
        wrapper.And(".inner", r => r.Set("color", "red"));
        var wrapperCss = wrapper.ToCss();
        wrapperCss.Contains(".w.inner").IsTrue();
        wrapperCss.Contains($"color{P}red;").IsTrue();
    }

    /// <summary>
    /// Tests that Inline emits only the rule's own properties as a <c>prop:value;</c> string.
    /// </summary>
    [Fact]
    public void CssRule_Inline_EmitsProperties()
    {
        // arrange
        var rule = Rule.Custom("x");
        rule.Set("color", "red");
        rule.Set("width", "1px");

        // act
        var inline = rule.Inline();

        // assert: both properties present, no selector/braces
        inline.Contains($"color{P}red;").IsTrue();
        inline.Contains($"width{P}1px;").IsTrue();
        inline.Contains("{").IsFalse();
    }

    /// <summary>
    /// Tests that Inline throws when the rule contains nested rules or media queries (inline styles cannot express
    /// them) — pins the guard that the property-only case cannot.
    /// </summary>
    [Fact]
    public void CssRule_Inline_ThrowsOnNestedOrMedia()
    {
        // nested rule
        var nested = Rule.Custom("x");
        nested.And(".y", _ => { });
        Wrap.It(() => nested.Inline()).Throws<InvalidOperationException>();

        // media query
        var media = Rule.Custom("x");
        media.Media("(min-width: 1px)", _ => { });
        Wrap.It(() => media.Inline()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Tests the single-property flexbox setters emit the correct CSS property/value — including
    /// <c>FlexWrap</c>, which must set <c>flex-wrap</c> from the <see cref="FlexWrap"/> value (regression guard:
    /// it previously set <c>flex-basis</c> from an int).
    /// </summary>
    [Fact]
    public void FlexboxExtensions_SetCorrectProperties()
    {
        // arrange
        var rule = Rule.Custom("x");
        rule.FlexDirection(FlexDirection.RowReverse)
            .AlignItems(AlignItems.FlexStart)
            .JustifyContent(JustifyContent.SpaceBetween)
            .FlexGrow(2)
            .FlexShrink(3)
            .FlexBasis("auto")
            .FlexWrap(FlexWrap.Wrap);

        // act
        var css = rule.Inline();

        // assert
        css.Contains($"flex-direction{P}row-reverse;").IsTrue();
        css.Contains($"align-items{P}flex-start;").IsTrue();
        css.Contains($"justify-content{P}space-between;").IsTrue();
        css.Contains($"flex-grow{P}2;").IsTrue();
        css.Contains($"flex-shrink{P}3;").IsTrue();
        css.Contains($"flex-basis{P}auto;").IsTrue();
        // FlexWrap sets flex-wrap (not the old flex-basis bug)
        css.Contains($"flex-wrap{P}wrap;").IsTrue();
        css.Contains($"flex-basis{P}wrap;").IsFalse();

        // FlexBasis(int) overload emits a unitless numeric basis (distinct body from the string overload)
        var basisInt = Rule.Custom("fb");
        basisInt.FlexBasis(10);
        basisInt.Inline().Contains($"flex-basis{P}10;").IsTrue();
    }

    /// <summary>
    /// Tests the composite flexbox helpers: FlexRow/FlexColumn set display, direction, align, and justify at once,
    /// and the <c>inline</c> flag switches display to inline-flex; Flex sets grow/shrink/basis together.
    /// </summary>
    [Fact]
    public void FlexboxExtensions_CompositeHelpers()
    {
        // FlexRow (block flex)
        var row = Rule.Custom("x");
        row.FlexRow(AlignItems.Center, JustifyContent.End);
        var rowCss = row.Inline();
        rowCss.Contains($"display{P}flex;").IsTrue();
        rowCss.Contains($"flex-direction{P}row;").IsTrue();
        rowCss.Contains($"align-items{P}center;").IsTrue();
        rowCss.Contains($"justify-content{P}end;").IsTrue();

        // FlexColumn with inline => inline-flex + column
        var col = Rule.Custom("y");
        col.FlexColumn(AlignItems.Start, JustifyContent.Start, inline: true);
        var colCss = col.Inline();
        colCss.Contains($"display{P}inline-flex;").IsTrue();
        colCss.Contains($"flex-direction{P}column;").IsTrue();

        // Flex(growAndShrink) => grow == shrink, basis auto
        var flex = Rule.Custom("z");
        flex.Flex(1);
        var flexCss = flex.Inline();
        flexCss.Contains($"flex-grow{P}1;").IsTrue();
        flexCss.Contains($"flex-shrink{P}1;").IsTrue();
        flexCss.Contains($"flex-basis{P}auto;").IsTrue();

        // reverse-direction composites map to the reversed flex-direction tokens
        var rowRev = Rule.Custom("rr");
        rowRev.FlexRowReverse(AlignItems.End, JustifyContent.Left);
        rowRev.Inline().Contains($"flex-direction{P}row-reverse;").IsTrue();
        var colRev = Rule.Custom("cr");
        colRev.FlexColumnReverse(AlignItems.End, JustifyContent.Left);
        colRev.Inline().Contains($"flex-direction{P}column-reverse;").IsTrue();

        // Flex(grow, shrink, basis) => distinct grow/shrink (pins arg order) + explicit basis
        var flex3 = Rule.Custom("f3");
        flex3.Flex(2, 5, "10px");
        var flex3Css = flex3.Inline();
        flex3Css.Contains($"flex-grow{P}2;").IsTrue();
        flex3Css.Contains($"flex-shrink{P}5;").IsTrue();
        flex3Css.Contains($"flex-basis{P}10px;").IsTrue();

        // AlignSelf sets align-self (distinct from AlignItems' align-items)
        var alignSelf = Rule.Custom("as");
        alignSelf.AlignSelf(AlignItems.Center);
        alignSelf.Inline().Contains($"align-self{P}center;").IsTrue();
    }
}
