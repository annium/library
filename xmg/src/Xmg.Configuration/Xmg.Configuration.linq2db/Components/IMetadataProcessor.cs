using Annium.linq2db.Extensions;
using Xmg.Core.Models;

namespace Xmg.Configuration.linq2db.Components;

internal interface IMetadataProcessor
{
    Database Process(DatabaseMetadata database);
}