using System.Text;
using Xmg.Core.Models;
using Xmg.Migration.FluentMigrator.Views.Syntax;

namespace Xmg.Migration.FluentMigrator.Views
{
    internal class CreateTableOperation : IMigrationOperation
    {
        private readonly string? _schema;
        private readonly Table _table;

        public CreateTableOperation(string? schema, Table table)
        {
            _schema = schema;
            _table = table;
        }

        public override string ToString()
        {
            var space = new string(' ', 16);

            var sb = new StringBuilder();
            sb.Append($"Create.Table(\"{_table}\")");

            if (string.IsNullOrWhiteSpace(_schema))
                sb.AppendLine();
            else
                sb.AppendLine($".InSchema(\"{_schema}\")");

            foreach (var column in _table.Columns)
                sb.AppendLine($"{space}.WithColumn(\"{column.Name}\"){new ColumnSyntax(column)}");

            return sb.ToString().Trim();
        }
    }
}