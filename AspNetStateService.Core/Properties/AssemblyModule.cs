using Autofac;

using Cogito.Autofac;
using Cogito.Autofac.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace AspNetStateService.Core
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
