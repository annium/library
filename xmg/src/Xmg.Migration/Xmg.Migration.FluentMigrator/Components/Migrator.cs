using System.Collections.Generic;
using Xmg.Core.Models;
using Xmg.Core.Tools;
using Xmg.Core.Views;
using Xmg.Migration.Abstractions;
using Xmg.Migration.Abstractions.Components;

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
                Up = "up",
                Down = "down"
            };

            var result = _templateWriter.Write("Templates.Migration", data);

            return result;
        }
    }
}