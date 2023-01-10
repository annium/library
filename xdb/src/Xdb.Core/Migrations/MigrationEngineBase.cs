using System;
using System.IO;
using System.Reflection;
using DbUp.Builder;

namespace Xdb.Core.Migrations;

public abstract class MigrationEngineBase<T>
    where T : MigrationEngineBase<T>
{
    protected readonly UpgradeEngineBuilder InitBuilder;
    protected readonly UpgradeEngineBuilder MigrationsBuilder;

    protected MigrationEngineBase(
        UpgradeEngineBuilder initBuilder,
        UpgradeEngineBuilder migrationsBuilder
    )
    {
        InitBuilder = initBuilder
            .WithTransactionPerScript()
            .LogToConsole();
        MigrationsBuilder = migrationsBuilder
            .WithTransactionPerScript()
            .LogToConsole();
    }

    public T WithScriptsFromDirectory(string folder)
    {
        InitBuilder.WithScriptsFromFileSystem(Path.Combine(folder, "Scripts", "Init"));
        MigrationsBuilder.WithScriptsFromFileSystem(Path.Combine(folder, "Scripts", "Migrations"));

        return (T) this;
    }

    public T WithScriptsFromAssembly(Assembly assembly)
    {
        InitBuilder.WithScriptsEmbeddedInAssembly(assembly, x => x.Contains(".Scripts.Init."));
        MigrationsBuilder.WithScriptsEmbeddedInAssembly(assembly, x => x.Contains(".Scripts.Migrations."));

        return (T) this;
    }

    public void Execute()
    {
        ExecuteBuilder(InitBuilder);
        ExecuteBuilder(MigrationsBuilder);

        static void ExecuteBuilder(UpgradeEngineBuilder builder)
        {
            var result = builder.Build().PerformUpgrade();
            if (!result.Successful)
                throw new ApplicationException($"{result.ErrorScript}: {result.Error}");
        }
    }
}