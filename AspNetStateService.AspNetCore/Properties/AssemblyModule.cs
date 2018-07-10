using Autofac;

using Cogito.Autofac;

namespace AspNetStateService.AspNetCore
{

    public class AssemblyModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
        }

    }

}
