using System;
using Xmg.Core.Models;
using Xmg.Core.Views;
using Xmg.Migration.Abstractions;
using Xmg.Migration.Abstractions.Components;

namespace Xmg.Migration.FluentMigrator.Components
{
    internal class Migrator : IMigrator
    {
        public MigrationProvider Provider => MigrationProvider.FluentMigrator;

        public IMigration CreateMigration(Database database, Config cfg)
        {
            // var migrationVersion = _getInstant().ToDateTimeUtc().ToString("yyyyMMdd");
            // var migrationName = "Init";
            throw new NotImplementedException();
        }
    }
}