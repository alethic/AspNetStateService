using Autofac;

using Cogito.Autofac;

namespace AspNetStateService.Samples.Web.Host
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
        }

    }

}
