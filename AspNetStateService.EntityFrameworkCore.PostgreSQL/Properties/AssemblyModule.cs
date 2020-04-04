using Autofac;

using Cogito.Autofac;

namespace AspNetStateService.EntityFrameworkCore.PostgreSQL
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterModule<AspNetStateService.EntityFrameworkCore.AssemblyModule>();
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
        }

    }

}
