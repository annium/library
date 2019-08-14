using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Connection.Abstract
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, System.IServiceProvider provider)
        {
            services.AddSingleton<ConnectionFactory>();

            services.AddConsoleLogger();
            services.AddShell();
        }
    }
}