using Autofac;

using Cogito.Autofac;

namespace AspNetStateService.EntityFrameworkCore.InMemory
{

    public class AssemblyModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
        }

    }

}
