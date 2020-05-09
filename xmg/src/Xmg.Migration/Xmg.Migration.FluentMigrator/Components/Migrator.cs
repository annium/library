using System.Collections.Generic;
using System.Linq;
using Xmg.Core.Models;
using Xmg.Core.Tools;
using Xmg.Core.Views;
using Xmg.Migration.Abstractions;
using Xmg.Migration.Abstractions.Components;
using Xmg.Migration.FluentMigrator.Views;

namespace Xmg.Migration.FluentMigrator.Components
{
    internal class Migrator : IMigrator
    {
        private readonly ITemplateWriter _templateWriter;
        public MigrationProvider Provider => MigrationProvider.FluentMigrator;

        public Migrator(
            ITemplateWriter templateWriter
        )
        {
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
                Up = GetUpOperations(database).ToArray(),
                Down = GetDownOperations(database).ToArray(),
            };

            var result = _templateWriter.Write("Templates.Migration", data);

            return result;
        }

        private IEnumerable<MigrationOperationGroup> GetUpOperations(Database database)
        {
            foreach (var schema in database.Schemas)
            {
                // create schema
                if (!string.IsNullOrWhiteSpace(schema.Name))
                    yield return new MigrationOperationGroup(
                        $"Create schema {schema.Name}",
                        new CreateSchemaOperation(schema.Name)
                    );

                // create tables in schema
                foreach (var table in schema.Tables)
                    yield return new MigrationOperationGroup(
                        $"Create table {table.Name}",
                        CreateTableOperations(schema.Name, table).ToArray()
                    );
            }
        }

        private IEnumerable<IMigrationOperation> CreateTableOperations(string? schema, Table table)
        {
            yield return new CreateTableOperation(schema, table);

            if (table.PrimaryKey != null)
                yield return new CreateTablePrimaryKeyOperation(schema, table.Name, table.PrimaryKey);

            foreach (var foreignKey in table.ForeignKeys)
                yield return new CreateTableForeignKeyOperation(foreignKey);
        }

        private IEnumerable<MigrationOperationGroup> GetDownOperations(Database database)
        {
            foreach (var schema in database.Schemas)
            {
                // delete tables in schema
                foreach (var table in schema.Tables)
                    yield return new MigrationOperationGroup(
                        $"Delete table {table.Name}",
                        new DeleteTableOperation(schema.Name, table.Name)
                    );

                // delete schema
                if (!string.IsNullOrWhiteSpace(schema.Name))
                    yield return new MigrationOperationGroup(
                        $"Delete schema {schema.Name}",
                        new DeleteSchemaOperation(schema.Name)
                    );
            }
        }
    }
}