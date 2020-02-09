using System.Fabric;

using Autofac;

using Cogito.Autofac;

namespace AspNetStateService.Service.Properties
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.Register(ctx => new FabricClient()).SingleInstance();
        }

    }

}
