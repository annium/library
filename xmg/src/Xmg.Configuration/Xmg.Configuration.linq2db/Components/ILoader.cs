using Annium.linq2db.Extensions;

namespace Xmg.Configuration.linq2db.Components;

internal interface ILoader
{
    DatabaseMetadata LoadMetadata(string assemblyPath);
}
