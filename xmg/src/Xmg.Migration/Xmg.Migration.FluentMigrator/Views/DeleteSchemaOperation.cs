using Xmg.Migration.Abstractions.Views;

namespace Xmg.Migration.FluentMigrator.Views;

internal class DeleteSchemaOperation : IDeleteSchemaOperation
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