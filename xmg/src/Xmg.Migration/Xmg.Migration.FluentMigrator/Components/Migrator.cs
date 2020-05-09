using System.Collections.Generic;
using System.Linq;
using Xmg.Core.Models;
using Xmg.Core.Tools;
using Xmg.Core.Views;
using Xmg.Migration.Abstractions;
using Xmg.Migration.Abstractions.Components;
using Xmg.Migration.Abstractions.Views;
using Xmg.Migration.FluentMigrator.Views;

namespace Xmg.Migration.FluentMigrator.Components
{
    internal class Migrator : IMigrator
    {
        private readonly IMigrationOrganizer _organizer;
        private readonly ITemplateWriter _templateWriter;
        public MigrationProvider Provider => MigrationProvider.FluentMigrator;

        public Migrator(
            IMigrationOrganizer organizer,
            ITemplateWriter templateWriter
        )
        {
            _organizer = organizer;
            _templateWriter = templateWriter;
        }

        public IMigration CreateMigration(Database database, Config cfg)
        {
            var files = new Dictionary<string, string>();
            files[$"{cfg.MigrationVersion}_{cfg.MigrationName}.cs"] = RenderMigration(database, cfg);

            return new Views.Migration(files);
        }

        private string RenderMigration(Database database, Config cfg)
        {
            var data = new
            {
                Namespace = cfg.MigrationNamespace,
                Name = cfg.MigrationName,
                Version = cfg.MigrationVersion,
                Up = _organizer.Organize(GetUpOperations(database).ToArray()),
                Down = _organizer.Organize(GetDownOperations(database).ToArray()),
            };

            var result = _templateWriter.Write("Templates.Migration", data);

            return result;
        }

        private IEnumerable<IOperation> GetUpOperations(Database database)
        {
            foreach (var schema in database.Schemas)
            {
                // create schema
                if (!string.IsNullOrWhiteSpace(schema.Name))
                    yield return new CreateSchemaOperation(schema.Name);

                // create tables in schema
                foreach (var table in schema.Tables)
                foreach (var operation in CreateTableOperations(schema.Name, table).ToArray())
                    yield return operation;
            }
        }

        private IEnumerable<IOperation> CreateTableOperations(string? schema, Table table)
        {
            yield return new CreateTableOperation(schema, table);

            if (table.PrimaryKey != null)
                yield return new CreateTablePrimaryKeyOperation(schema, table.Name, table.PrimaryKey);

            foreach (var foreignKey in table.ForeignKeys)
                yield return new CreateTableForeignKeyOperation(foreignKey);
        }

        private IEnumerable<IOperation> GetDownOperations(Database database)
        {
            foreach (var schema in database.Schemas)
            {
                // delete tables in schema
                foreach (var table in schema.Tables)
                    yield return new DeleteTableOperation(schema.Name, table.Name);

                // delete schema
                if (!string.IsNullOrWhiteSpace(schema.Name))
                    yield return new DeleteSchemaOperation(schema.Name);
            }
        }
    }
}