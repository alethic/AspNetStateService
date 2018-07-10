using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.AspNetCore;

using Autofac;
using Autofac.Integration.ServiceFabric;

using Cogito.Autofac;

using FileAndServe.Components.ServiceFabric.AspNetCore;

namespace AspNetStateService.Fabric.Services
{

    public static class Program
    {

        /// <summary>
        /// Main application entry point.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterAllAssemblyModules();
            builder.RegisterServiceFabricSupport();
            builder.RegisterStatelessKestrelWebService<StateWebService>("StateWebService", "HttpServiceEndpoint");
            builder.RegisterActor<StateActor>();

            using (builder.Build())
                await Task.Delay(Timeout.Infinite);
        }

    }

}
