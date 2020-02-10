using Autofac;

using Cogito.Autofac;

namespace AspNetStateService.Redis
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterModule<AspNetStateService.Core.AssemblyModule>();
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
        }

    }

}
