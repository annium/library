using System.Collections.Generic;

namespace Xmg.Core.Models;

public class Database
{
    public IReadOnlyCollection<Schema> Schemas { get; }

    public Database(IReadOnlyCollection<Schema> schemas)
    {
        Schemas = schemas;
    }
}