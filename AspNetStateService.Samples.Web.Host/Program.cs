using System;
using System.Threading;
using System.Threading.Tasks;

using Autofac;

using Cogito.Autofac;
using Cogito.IIS.Configuration;
using Cogito.ServiceFabric;

namespace AspNetStateService.Samples.Web.Host
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

            var binding = new BindingData("http", "*:4521:");
            await AppHostUtil.BuildHost(new[] { binding }, "/").RunAsync();

#if DEBUG
            Console.ReadLine();
#endif
        }

        /// <summary>
        /// Main application entry-point.
        /// </summary>
        /// <param name="args"></param>
        static async Task RunFabric(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterAllAssemblyModules();

            using (var container = builder.Build())
                await Task.Delay(Timeout.Infinite);
        }

    }

}
