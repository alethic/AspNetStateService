using System.Threading.Tasks;

using AspNetStateService.AspNetCore.Kestrel;

using Autofac.Extensions.DependencyInjection;

using Cogito.Autofac;
using Cogito.Extensions.Configuration;
using Cogito.Extensions.Configuration.Autofac;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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
        public static async Task Main(string[] args) =>
            await Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory(b => b.RegisterAllAssemblyModules()))
                .ConfigureWebHost(w => w
                    .UseKestrelStateServer(o => o.ListenLocalhost(42424)))
                .RunConsoleAsync();

    }

}
