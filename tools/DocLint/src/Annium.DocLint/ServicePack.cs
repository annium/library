using System;
using Annium.Core.DependencyInjection;
using Annium.DocLint.Internal.Services;

namespace Annium.DocLint;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddArguments();
        container.AddLogging();
        container.AddMapper();

        container.Add<LintService>().AsSelf().Singleton();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}
