using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backuper.Connection.PostgreSQL
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, System.IServiceProvider provider)
        {
            services.AddSingleton<Abstract.ConnectionManager<Configuration>, ConnectionManager>();
        }
    }
}