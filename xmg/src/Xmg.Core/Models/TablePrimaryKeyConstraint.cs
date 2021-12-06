using System.Collections.Generic;
using System.Text;

namespace Xmg.Core.Models;

public class TablePrimaryKeyConstraint
{
    public string Name { get; }
    public IReadOnlyCollection<string> Columns { get; }
    private readonly string? _schema;
    private readonly string _table;

    public TablePrimaryKeyConstraint(
        string? schema,
        string table,
        IReadOnlyCollection<string> columns
    )
    {
        _schema = schema;
        _table = table;
        Columns = columns;
        Name = BuildName();
    }

    public override string ToString() => Name;

    private string BuildName()
    {
        var sb = new StringBuilder("PK");

        if (!string.IsNullOrWhiteSpace(_schema))
            sb.Append($"_{_schema}");
        sb.Append($"_{_table}");

        return sb.ToString();
    }
}