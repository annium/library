using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xmg.Migration.Abstractions.Components;
using Xmg.Migration.Abstractions.Views;

namespace Xmg.Migration.Components
{
    internal class MigrationOrganizer : IMigrationOrganizer
    {
        public IReadOnlyCollection<IOperation> Organize(IReadOnlyCollection<IOperation> operations)
        {
            var list = operations.ToList();

            var result = new List<IOperation>();

            // delete foreign keys
            result.AddRange(list.PullOfType<IDeleteTableForeignKeyOperation>());

            // delete unique constraints
            result.AddRange(list.PullOfType<IDeleteTableUniqueConstraintOperation>());

            // delete indexes
            result.AddRange(list.PullOfType<IDeleteTableIndexOperation>());

            // delete primary keys
            result.AddRange(list.PullOfType<IDeleteTablePrimaryKeyOperation>());

            // delete columns
            result.AddRange(list.PullOfType<IDeleteTableColumnOperation>());

            // delete tables
            result.AddRange(list.PullOfType<IDeleteTableOperation>());

            // delete schemas
            result.AddRange(list.PullOfType<IDeleteSchemaOperation>());

            // alter columns
            result.AddRange(list.PullOfType<IAlterTableColumnOperation>());

            // alter tables
            result.AddRange(list.PullOfType<IAlterTableColumnOperation>());

            // rename columns
            result.AddRange(list.PullOfType<IRenameTableColumnOperation>());

            // rename tables
            result.AddRange(list.PullOfType<IRenameTableOperation>());

            // create schemas
            result.AddRange(list.PullOfType<ICreateSchemaOperation>());

            // create tables
            result.AddRange(list.PullOfType<ICreateTableOperation>());

            // create columns
            result.AddRange(list.PullOfType<ICreateTableColumnOperation>());

            // create primary keys
            result.AddRange(list.PullOfType<ICreateTablePrimaryKeyOperation>());

            // create indexes
            result.AddRange(list.PullOfType<ICreateTableIndexOperation>());

            // create unique constraints
            result.AddRange(list.PullOfType<ICreateTableUniqueConstraintOperation>());

            // create foreign keys
            result.AddRange(list.PullOfType<ICreateTableForeignKeyOperation>());

            return result;
        }
    }

    internal static class ListExtensions
    {
        public static IReadOnlyCollection<T> PullOfType<T>(this IList collection)
        {
            var result = collection.OfType<T>().ToArray();

            foreach (var element in result)
                collection.Remove(element);

            return result;
        }
    }
}