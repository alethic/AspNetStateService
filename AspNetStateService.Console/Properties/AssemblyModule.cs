using Autofac;

using Cogito.Autofac;
using Cogito.Autofac.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace AspNetStateService.Console
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.Populate(s => s.AddHttpClient());
        }

    }

}
