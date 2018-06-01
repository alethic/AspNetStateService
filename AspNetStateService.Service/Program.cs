using System.Threading;
using System.Threading.Tasks;

using Autofac;
using Autofac.Integration.ServiceFabric;

using FileAndServe.AspNetCore;
using FileAndServe.Autofac;
using FileAndServe.Components.AspNetCore;
using FileAndServe.Components.ServiceFabric.AspNetCore;
using FileAndServe.ServiceFabric;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace AspNetStateService.Service
{

    public static class Program
    {

        /// <summary>
        /// Main application entry point.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Task Main(string[] args)
        {
            return FabricEnvironment.IsFabric ? RunFabric(args) : RunConsole(args);
        }

        /// <summary>
        /// Runs the application in Console mode.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task RunConsole(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterAllAssemblyModules();
            builder.RegisterFromAttributes(typeof(Program).Assembly);

            using (var container = builder.Build())
            using (var hostScope = container.BeginLifetimeScope())
                await WebHost.CreateDefaultBuilder(args)
                    .ConfigureComponents<StateWebService>(hostScope)
                    .UseKestrel()
                    .BuildAndRunAsync();
        }

        /// <summary>
        /// Runs the application in Service Fabric mode.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task RunFabric(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterAllAssemblyModules();
            builder.RegisterServiceFabricSupport();
            builder.RegisterActor<StateObjectActor>();
            builder.RegisterStatelessKestrelWebService<StateWebService>("StateWebService", "HttpServiceEndpoint");

            using (builder.Build())
                await Task.Delay(Timeout.Infinite);
        }

    }

}
