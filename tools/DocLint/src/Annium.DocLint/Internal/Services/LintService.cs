using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.DocLint.Internal.Services;

internal class LintService(ILogger logger) : ILogSubject
{
    public ILogger Logger { get; } = logger;

    public async Task<IReadOnlyList<string>> LintAsync(string file)
    {
        this.Trace<string>("Lint {file}", file);

        return [];
    }
}
