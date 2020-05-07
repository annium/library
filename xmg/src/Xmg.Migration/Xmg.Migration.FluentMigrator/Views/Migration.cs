using System.Collections.Generic;
using Xmg.Core.Views;

namespace Xmg.Migration.FluentMigrator.Views
{
    internal class Migration : IMigration
    {
        public IReadOnlyDictionary<string, string> Files { get; }

        public Migration(
            IReadOnlyDictionary<string, string> files
        )
        {
            Files = files;
        }
    }
}