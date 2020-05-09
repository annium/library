namespace Xmg.Migration.FluentMigrator.Views
{
    internal class DeleteSchemaOperation : IMigrationOperation
    {
        private readonly string _schema;

        public DeleteSchemaOperation(
            string schema
        )
        {
            _schema = schema;
        }

        public override string ToString() => $"Delete.Schema(\"{_schema}\")";
    }
}