using Autofac;

using Cogito.Autofac;

namespace AspNetStateService.EntityFrameworkCore.SqlServer
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
