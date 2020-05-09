using System.Text;
using Xmg.Core.Models;

namespace Xmg.Migration.FluentMigrator.Views
{
    internal class CreateTableForeignKeyOperation : IMigrationOperation
    {
        private readonly TableForeignKeyConstraint _key;

        public CreateTableForeignKeyOperation(
            TableForeignKeyConstraint key
        )
        {
            _key = key;
        }

        public override string ToString()
        {
            var space = new string(' ', 16);

            var sb = new StringBuilder();
            sb.AppendLine($"Create.ForeignKey(\"{_key.Name}\")");

            sb.Append($"{space}.FromTable(\"{_key.ForeignTable}\")");
            if (!string.IsNullOrWhiteSpace(_key.ForeignSchema))
                sb.Append($".InSchema(\"{_key.ForeignSchema}\")");
            sb.AppendLine($".ForeignColumn(\"{_key.ForeignColumn}\")");

            sb.Append($"{space}.ToTable(\"{_key.PrimaryTable}\")");
            if (!string.IsNullOrWhiteSpace(_key.PrimarySchema))
                sb.Append($".InSchema(\"{_key.PrimarySchema}\")");
            sb.AppendLine($".PrimaryColumn(\"{_key.PrimaryColumn}\")");

            return sb.ToString().Trim();
        }
    }
}