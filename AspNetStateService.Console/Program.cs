using System.Threading.Tasks;

using AspNetStateService.AspNetCore;

using Autofac;

using FileAndServe.AspNetCore;
using FileAndServe.Autofac;
using FileAndServe.Components.AspNetCore;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace AspNetStateService.Console
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

            using (var container = builder.Build())
            using (var hostScope = container.BeginLifetimeScope())
                await WebHost.CreateDefaultBuilder(args)
                    .ConfigureComponents<StateWebService>(hostScope)
                    .UseKestrel()
                    .BuildAndRunAsync();
        }

    }

}
