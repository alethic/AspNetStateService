using System.Threading;
using System.Threading.Tasks;

using Autofac;
using Autofac.Integration.ServiceFabric;

using FileAndServe.Autofac;
using FileAndServe.Components.ServiceFabric.AspNetCore;

namespace AspNetStateService.Service
{

    public static class Program
    {

        public static async Task Main()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAllAssemblyModules();
            builder.RegisterServiceFabricSupport();
            builder.RegisterStatelessKestrelWebService<StateWebService>("StateWebService");
            builder.RegisterActor<StateObjectActor>();

            using (builder.Build())
                await Task.Delay(Timeout.Infinite);
        }

    }

}
