using System.Collections.Generic;
using System.Text;

namespace Xmg.Core.Models
{
    public class TablePrimaryKeyConstraint
    {
        public string Name { get; }
        public IReadOnlyCollection<string> Columns { get; }

        public TablePrimaryKeyConstraint(
            IReadOnlyCollection<string> columns
        )
        {
            Columns = columns;
            Name = BuildName();
        }

        public override string ToString() => Name;

        private string BuildName()
        {
            var sb = new StringBuilder("PK_");

            sb.Append(string.Join('_', Columns));

            return sb.ToString();
        }
    }
}