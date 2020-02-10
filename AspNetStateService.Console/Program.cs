using System.Threading.Tasks;

using AspNetStateService.AspNetCore;
using AspNetStateService.AspNetCore.Kestrel;

using Autofac;

using Cogito.AspNetCore;
using Cogito.AspNetCore.Autofac;
using Cogito.Autofac;
using Cogito.Extensions.Configuration;
using Cogito.Extensions.Configuration.Autofac;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace AspNetStateService.Console
{

    public static class Program
    {

        /// <summary>
        /// Applies debug configuration for the console.
        /// </summary>
        [RegisterAs(typeof(IConfigurationBuilderConfiguration))]
        public class DebugConfigurationBuilderConfiguration : IConfigurationBuilderConfiguration
        {

            public IConfigurationBuilder Apply(IConfigurationBuilder builder)
            {
                return builder.AddParentJsonFiles("App.config.json");
            }

        }

        /// <summary>
        /// Main application entry point.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterAllAssemblyModules();

            using (var container = builder.Build())
            using (var hostScope = container.BeginLifetimeScope())
                await new WebHostBuilder()
                    .UseStartup<StateWebServiceStartup>(hostScope)
                    .UseKestrel(o => o.ListenLocalhost(42424))
                    .UseKestrelPatch()
                    .BuildAndRunAsync();
        }


    }

}
