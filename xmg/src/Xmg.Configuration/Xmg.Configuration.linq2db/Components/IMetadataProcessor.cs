using Xmg.Core.Models;
using LDatabase = Annium.linq2db.Extensions.Models.Database;

namespace Xmg.Configuration.linq2db.Components;

internal interface IMetadataProcessor
{
    Database Process(LDatabase database);
}