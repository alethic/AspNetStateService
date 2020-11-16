using System.Fabric;

using Autofac;

using Cogito.Autofac;

namespace AspNetStateService.ServiceFabric.Actor.Services
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterModule<AspNetStateService.ServiceFabric.Actor.Core.AssemblyModule>();
            builder.RegisterModule<AspNetStateService.AspNetCore.Kestrel.AssemblyModule>();
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.Register(ctx => new FabricClient()).SingleInstance();
        }

    }

}
