using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Annium.Logging.Shared.Internal;

/// <summary>
/// Helper class for processing log message templates
/// </summary>
internal static class Helper
{
    /// <summary>
    /// How many parts <see cref="Render"/> assembles without touching the heap. A template with N
    /// placeholders has 2N+1 parts, so this covers up to four — every logging overload in practice, and
    /// far past what a readable message uses.
    /// </summary>
    private const int InlineParts = 9;

    /// <summary>
    /// Processes a message template with data items to produce a formatted message and data dictionary
    /// </summary>
    /// <param name="messageTemplate">The message template with placeholder variables</param>
    /// <param name="dataItems">The data items to substitute into the template</param>
    /// <returns>A tuple containing the formatted message and data dictionary</returns>
    /// <exception cref="InvalidOperationException">Thrown when more data items are supplied than the template has placeholders.</exception>
    public static (string, IReadOnlyDictionary<string, object?>) Process(
        string messageTemplate,
        IReadOnlyList<object?> dataItems
    )
    {
        // Nothing to bind, so nothing to render and nothing to name. Skipping the parse is not only
        // cheaper, it is the whole cost on the path a bridge from another logging framework takes: it
        // hands over an already-rendered message and an empty argument list. Note this returns the
        // template verbatim where the scan would have dropped a brace that closes nothing, or swallowed
        // the tail after a '{' that never closes - for text nobody wrote as a template, verbatim is the
        // only defensible answer, and at a real call site LOG0003 rejects both shapes anyway.
        if (dataItems.Count == 0)
            return (messageTemplate, LogMessageData.Empty);

        var parsed = ParsedTemplate.Get(messageTemplate);

        var extra = dataItems.Count - parsed.Names.Length;
        if (extra > 0)
            throw new InvalidOperationException($"Unexpected {extra} data item(s). Template: {messageTemplate}.");

        // the dictionary is built only if a sink reads it - see LogMessageData
        return (Render(parsed, dataItems), new LogMessageData(parsed.Names, dataItems));
    }

    /// <summary>
    /// Assembles the message from the template's literals and the bound values.
    /// </summary>
    /// <param name="parsed">The cached template parse.</param>
    /// <param name="dataItems">The values to bind, positionally.</param>
    /// <returns>The rendered message.</returns>
    /// <remarks>
    /// One concatenation rather than a <see cref="System.Text.StringBuilder"/> and a copy out of it: the
    /// parts are known up front, and for any realistic placeholder count they are gathered in an inline
    /// array that never leaves the stack. A placeholder with no value re-emits itself in braces, which
    /// is what the original scan did.
    /// </remarks>
    private static string Render(ParsedTemplate parsed, IReadOnlyList<object?> dataItems)
    {
        var literals = parsed.Literals;
        var names = parsed.Names;
        var count = 2 * names.Length + 1;

        if (count > InlineParts)
        {
            var heap = new string[count];
            Fill(heap, literals, names, dataItems);

            return string.Concat(heap);
        }

        var parts = default(PartsBuffer);
        Fill(parts, literals, names, dataItems);

        return string.Concat(((ReadOnlySpan<string>)parts)[..count]);
    }

    /// <summary>
    /// Writes the alternating literal / value sequence into the given buffer.
    /// </summary>
    /// <param name="parts">Destination, at least <c>2 * names.Length + 1</c> long.</param>
    /// <param name="literals">Literal text between placeholders.</param>
    /// <param name="names">Placeholder names, in order.</param>
    /// <param name="dataItems">Values to bind, positionally.</param>
    private static void Fill(Span<string> parts, string[] literals, string[] names, IReadOnlyList<object?> dataItems)
    {
        var at = 0;

        for (var i = 0; i < names.Length; i++)
        {
            parts[at++] = literals[i];
            parts[at++] = i < dataItems.Count ? Stringify(dataItems[i]) : $"{{{names[i]}}}";
        }

        parts[at] = literals[names.Length];
    }

    /// <summary>
    /// Renders a bound value the way appending it to a string builder did: its <c>ToString</c>, and
    /// nothing at all for a null value or a null <c>ToString</c>.
    /// </summary>
    /// <param name="value">The value to render.</param>
    /// <returns>The rendered value.</returns>
    private static string Stringify(object? value) => value?.ToString() ?? string.Empty;

    /// <summary>
    /// Stack buffer for the parts of a rendered message.
    /// </summary>
    [InlineArray(InlineParts)]
    private struct PartsBuffer
    {
        /// <summary>
        /// The first of <see cref="InlineParts"/> elements the compiler lays out from this field.
        /// </summary>
        private string _element0;
    }
}
