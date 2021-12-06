using LDatabase = Annium.linq2db.Extensions.Models.Database;

namespace Xmg.Configuration.linq2db.Components;

internal interface ILoader
{
    LDatabase LoadMetadata(string assemblyPath);
}