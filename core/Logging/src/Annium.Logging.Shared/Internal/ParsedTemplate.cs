using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Annium.Logging.Shared.Internal;

/// <summary>
/// A message template taken apart once: the literal text between its placeholders, and the placeholder
/// names in the order they appear. Depends on nothing but the template string, which is why it can be
/// cached and reused for every message produced from a given call site.
/// </summary>
/// <remarks>
/// <see cref="Literals"/> always has exactly one more element than <see cref="Names"/>, so rendering is
/// <c>Literals[0] + value0 + Literals[1] + value1 + … + Literals[N]</c>. Empty literals are kept rather
/// than elided, which is what keeps that invariant true for adjacent placeholders (<c>"{a}{b}"</c>) and
/// for a template that starts or ends with one.
/// </remarks>
internal sealed class ParsedTemplate
{
    /// <summary>
    /// Cache keyed on the template instance.
    /// </summary>
    /// <remarks>
    /// A template is a compile-time constant — the <c>LOG0001</c> analyzer rejects anything else at an
    /// Annium logging call site — so the live set is bounded by the number of call sites, and interning
    /// makes every call from one site hit the same entry. The table is weak for the callers the analyzer
    /// does not police: a bridge from another logging framework can hand over a freshly rendered string,
    /// and an entry keyed on one of those dies with it instead of accumulating.
    /// </remarks>
    private static readonly ConditionalWeakTable<string, ParsedTemplate> _cache = new();

    /// <summary>
    /// Held in a field so the method group is converted to a delegate once rather than per lookup.
    /// </summary>
    private static readonly ConditionalWeakTable<string, ParsedTemplate>.CreateValueCallback _parse = Parse;

    /// <summary>
    /// Literal text between placeholders, one more element than there are names.
    /// </summary>
    public string[] Literals { get; }

    /// <summary>
    /// Placeholder names in the order they appear in the template.
    /// </summary>
    public string[] Names { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParsedTemplate"/> class.
    /// </summary>
    /// <param name="literals">Literal text between placeholders.</param>
    /// <param name="names">Placeholder names, in order.</param>
    private ParsedTemplate(string[] literals, string[] names)
    {
        Literals = literals;
        Names = names;
    }

    /// <summary>
    /// Returns the parse of the given template, parsing it on first sight and reusing it afterwards.
    /// </summary>
    /// <param name="template">The template to take apart.</param>
    /// <returns>The cached parse.</returns>
    public static ParsedTemplate Get(string template) => _cache.GetValue(template, _parse);

    /// <summary>
    /// Walks the template once, cutting it into literals at each placeholder and collecting the names.
    /// </summary>
    /// <param name="template">The template to parse.</param>
    /// <returns>The parse.</returns>
    /// <remarks>
    /// This is the original scan with one change: where it appended a value, this cuts a literal. The
    /// brace handling is character for character the same, so every template the old code accepted
    /// renders the same way. A placeholder opens at the first <c>{</c> and closes only when the nesting
    /// depth returns to zero, which is what names <c>"{{nested} data}"</c> as <c>"{nested} data"</c>. A
    /// <c>}</c> that closes nothing is dropped rather than kept, and an unclosed <c>{</c> swallows the
    /// rest of the template — both reported by the <c>LOG0003</c> analyzer at a call site, and neither
    /// reachable from a well-formed one. Runs once per template, so it allocates freely.
    /// </remarks>
    private static ParsedTemplate Parse(string template)
    {
        var literals = new List<string>();
        var names = new List<string>();
        var literal = new StringBuilder();

        var depth = 0;
        var keyIndex = -1;

        for (var i = 0; i < template.Length; i++)
        {
            var ch = template[i];
            switch (ch)
            {
                case '{':
                    // ensure not opening nested template var
                    if (keyIndex == -1)
                        keyIndex = i;
                    depth++;
                    break;
                case '}':
                    depth--;
                    if (depth != 0)
                        break;

                    names.Add(template.Substring(keyIndex + 1, i - keyIndex - 1));
                    literals.Add(literal.ToString());
                    literal.Clear();
                    keyIndex = -1;
                    break;
                default:
                    // if not inside template var name - just add char
                    if (keyIndex == -1)
                        literal.Append(ch);
                    break;
            }
        }

        literals.Add(literal.ToString());

        return new ParsedTemplate(literals.ToArray(), names.ToArray());
    }
}
