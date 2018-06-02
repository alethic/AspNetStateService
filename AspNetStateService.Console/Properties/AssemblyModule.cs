using Autofac;

using FileAndServe.Autofac;

namespace AspNetStateService.Console
{

    public class AssemblyModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
        }

    }

}
