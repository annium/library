using System.Collections.Generic;

namespace Xmg.Core.Models
{
    public class Table
    {
        public string Name { get; }
        public IReadOnlyCollection<TableColumn> Columns { get; }
        public TablePrimaryKeyConstraint? PrimaryKey { get; }
        public IReadOnlyCollection<TableIndex> Indexes { get; }
        public IReadOnlyCollection<TableForeignKeyConstraint> ForeignKeys { get; }

        public Table(
            string name,
            IReadOnlyCollection<TableColumn> columns,
            TablePrimaryKeyConstraint? primaryKey,
            IReadOnlyCollection<TableIndex> indexes,
            IReadOnlyCollection<TableForeignKeyConstraint> foreignKeys
        )
        {
            Name = name;
            Columns = columns;
            PrimaryKey = primaryKey;
            Indexes = indexes;
            ForeignKeys = foreignKeys;
        }

        public override string ToString() => Name;
    }
}