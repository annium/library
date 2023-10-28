using System.Collections.Generic;

namespace Xmg.Core.Models;

public class Schema
{
    public string Name { get; }
    public IReadOnlyCollection<Table> Tables { get; }

    public Schema(string name, IReadOnlyCollection<Table> tables)
    {
        Name = name;
        Tables = tables;
    }

    public override string ToString() => Name;
}
