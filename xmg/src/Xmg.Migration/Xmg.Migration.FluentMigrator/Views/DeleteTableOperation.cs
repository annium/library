using System.Text;
using Xmg.Migration.Abstractions.Views;

namespace Xmg.Migration.FluentMigrator.Views;

internal class DeleteTableOperation : Abstractions.Views.DeleteTableOperation
{
    private readonly string? _schema;
    private readonly string _table;

    public DeleteTableOperation(
        string? schema,
        string table
    )
    {
        _schema = schema;
        _table = table;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append($"Delete.Table(\"{_table}\")");
        if (string.IsNullOrWhiteSpace(_schema))
            sb.AppendLine();
        else
            sb.Append($".InSchema(\"{_schema}\")");

        return sb.ToString();
    }
}