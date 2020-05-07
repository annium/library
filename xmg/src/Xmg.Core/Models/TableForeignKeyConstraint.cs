using System.Data;
using System.Text;

namespace Xmg.Core.Models
{
    public class TableForeignKeyConstraint
    {
        public string Name { get; }
        public string? ForeignSchema { get; }
        public string ForeignTable { get; }
        public string ForeignColumn { get; }
        public string? PrimarySchema { get; }
        public string PrimaryTable { get; }
        public string PrimaryColumn { get; }
        public Rule Rule { get; }

        public TableForeignKeyConstraint(
            string? foreignSchema,
            string foreignTable,
            string foreignColumn,
            string? primarySchema,
            string primaryTable,
            string primaryColumn,
            Rule rule
        )
        {
            ForeignSchema = foreignSchema;
            ForeignTable = foreignTable;
            ForeignColumn = foreignColumn;
            PrimarySchema = primarySchema;
            PrimaryTable = primaryTable;
            PrimaryColumn = primaryColumn;
            Rule = rule;
            Name = BuildName();
        }

        public override string ToString() => Name;

        private string BuildName()
        {
            var sb = new StringBuilder("FK_");

            // for a while - don't use foreign schema in names
            // if (!string.IsNullOrWhiteSpace(ForeignSchema))
            //     sb.Append($"{ForeignSchema}_");

            sb.Append($"{ForeignTable}_{ForeignColumn}__");

            // append schema name, only if it differs
            if (!string.IsNullOrWhiteSpace(PrimarySchema) && ForeignSchema != PrimarySchema)
                sb.Append($"{PrimarySchema}_");

            sb.Append($"{PrimaryTable}_{PrimaryColumn}");

            return sb.ToString();
        }
    }
}