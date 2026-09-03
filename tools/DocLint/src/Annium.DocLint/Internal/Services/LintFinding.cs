namespace Annium.DocLint.Internal.Services;

/// <summary>
/// A single missing-documentation report.
/// </summary>
/// <param name="Message">The message shown to the user.</param>
/// <param name="PartialType">
/// Set only on the type-level report of a <c>partial</c> type declaration, to the type's
/// namespace-qualified key. Such a report is dropped when another file in the same run documents
/// that type — the documentation belongs to the type, not to each of its declarations.
/// </param>
internal sealed record LintFinding(string Message, string? PartialType = null);
