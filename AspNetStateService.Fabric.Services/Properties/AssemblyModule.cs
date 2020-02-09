using System.Fabric;

using Autofac;

using Cogito.Autofac;

namespace AspNetStateService.Fabric.Services
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterModule<AspNetStateService.Fabric.Core.AssemblyModule>();
            builder.RegisterModule<AspNetStateService.AspNetCore.Kestrel.AssemblyModule>();
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.Register(ctx => new FabricClient()).SingleInstance();
        }

    }

}
