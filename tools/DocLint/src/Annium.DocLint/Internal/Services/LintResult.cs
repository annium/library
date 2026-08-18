using System.Collections.Generic;
using System.Linq;

namespace Annium.DocLint.Internal.Services;

/// <summary>
/// What linting one source file found, plus what that file contributes to the run as a whole:
/// a partial type is documented once, in whichever file its author chose, so its other declarations
/// can only be judged after every file has been read.
/// </summary>
/// <param name="Findings">The findings, in declaration order.</param>
/// <param name="DocumentedPartialTypes">
/// Keys of the partial types this file documents, matching <see cref="LintFinding.PartialType"/>.
/// </param>
internal sealed record LintResult(
    IReadOnlyList<LintFinding> Findings,
    IReadOnlyCollection<string> DocumentedPartialTypes
)
{
    public IReadOnlyList<string> Errors => Findings.Select(x => x.Message).ToArray();
}
