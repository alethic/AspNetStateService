using System.Threading.Tasks;

using AspNetStateService.AspNetCore;

using Autofac;

using Cogito.AspNetCore;
using Cogito.AspNetCore.Autofac;
using Cogito.Autofac;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

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
            builder.Register(ctx => new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

            using (var container = builder.Build())
            using (var hostScope = container.BeginLifetimeScope())
                await WebHost.CreateDefaultBuilder(args)
                    .UseUrls("http://localhost:42424")
                    .UseKestrel()
                    .UseStartup<StateWebService>(hostScope)
                    .BuildAndRunAsync();
        }

    }

}
