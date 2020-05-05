using System.Data;
using System.Text;

namespace Xmg.Core.Models
{
    public class TableForeignKeyConstraint
    {
        public string Name { get; }
        public string PrimarySchema { get; }
        public string PrimaryTable { get; }
        public string PrimaryColumn { get; }
        public string ForeignSchema { get; }
        public string ForeignTable { get; }
        public string ForeignColumn { get; }
        public Rule Rule { get; }

        public TableForeignKeyConstraint(
            string primarySchema,
            string primaryTable,
            string primaryColumn,
            string foreignSchema,
            string foreignTable,
            string foreignColumn,
            Rule rule
        )
        {
            PrimarySchema = primarySchema;
            PrimaryTable = primaryTable;
            PrimaryColumn = primaryColumn;
            ForeignSchema = foreignSchema;
            ForeignTable = foreignTable;
            ForeignColumn = foreignColumn;
            Rule = rule;
            Name = BuildName();
        }

        private string BuildName()
        {
            var sb = new StringBuilder("FK_");

            if (!string.IsNullOrWhiteSpace(PrimarySchema))
                sb.Append($"{PrimarySchema}_");

            sb.Append($"{PrimaryTable}_{PrimaryColumn}__");

            if (!string.IsNullOrWhiteSpace(ForeignSchema))
                sb.Append($"{ForeignSchema}_");

            sb.Append($"{ForeignTable}_{ForeignColumn}");

            return sb.ToString();
        }
    }
}