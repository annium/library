using System.Collections.Generic;
using System.Text;

namespace Xmg.Core.Models
{
    public class TableIndex
    {
        public string Name { get; }
        public IReadOnlyCollection<string> Columns { get; }
        public bool IsUnique { get; }

        public TableIndex(
            IReadOnlyCollection<string> columns,
            bool isUnique
        )
        {
            Columns = columns;
            IsUnique = isUnique;
            Name = BuildName();
        }

        private string BuildName()
        {
            var sb = new StringBuilder("IX_");

            sb.Append(string.Join('_', Columns));

            if (IsUnique)
                sb.Append("_unique");

            return sb.ToString();
        }
    }
}