using System.Linq;
using System.Text;
using Xmg.Core.Models;
using Xmg.Migration.Abstractions.Views;

namespace Xmg.Migration.FluentMigrator.Views
{
    internal class CreateTablePrimaryKeyOperation : ICreateTablePrimaryKeyOperation
    {
        private readonly string? _schema;
        private readonly string _table;
        private readonly TablePrimaryKeyConstraint _primaryKey;

        public CreateTablePrimaryKeyOperation(
            string? schema,
            string table,
            TablePrimaryKeyConstraint primaryKey
        )
        {
            _schema = schema;
            _table = table;
            _primaryKey = primaryKey;
        }

        public override string ToString()
        {
            var space = new string(' ', 16);

            var sb = new StringBuilder();
            sb.AppendLine($"Create.PrimaryKey(\"{_primaryKey}\")");

            sb.Append($"{space}.OnTable(\"{_table}\")");

            if (!string.IsNullOrWhiteSpace(_schema))
                sb.Append($".WithSchema(\"{_schema}\")");

            if (_primaryKey.Columns.Count > 1)
                sb.AppendLine($".Columns({string.Join(", ", _primaryKey.Columns.Select(x => $"\"{x}\""))})");
            else
                sb.AppendLine($".Column(\"{_primaryKey.Columns.Single()}\")");

            return sb.ToString().Trim();
        }
    }
}