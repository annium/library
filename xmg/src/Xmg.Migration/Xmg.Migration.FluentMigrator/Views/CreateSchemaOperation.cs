using Xmg.Migration.Abstractions.Views;

namespace Xmg.Migration.FluentMigrator.Views;

internal class CreateSchemaOperation : ICreateSchemaOperation
{
    public string Name { get; }

    public CreateSchemaOperation(string name)
    {
        Name = name;
    }

    public override string ToString() => $"Create.Schema(\"{Name}\")";
}
