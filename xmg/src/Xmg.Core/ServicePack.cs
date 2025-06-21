using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Xmg.Core.Tools;
using Xmg.Core.Tools.Implementations;

namespace Xmg.Core;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // tools
        container.Add<ITemplateWriter, TemplateWriter>().Singleton();

        // externals
        container.AddResourceLoader();
    }
}
