using Autofac;

using FileAndServe.Autofac;

namespace AspNetStateService.EntityFrameworkCore
{

    public class AssemblyModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
        }

    }

}
