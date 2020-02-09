using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Autofac;
using Autofac.Integration.ServiceFabric;

using Cogito.Autofac;
using Cogito.HostedWebCore;
using Cogito.ServiceFabric;
using Cogito.Web.Configuration;

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
            return FabricEnvironment.IsFabric ? RunFabric(args) : Task.Run(() => RunConsole(args));
        }

        /// <summary>
        /// Loads the XML resource with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static XDocument LoadXmlResource(string name)
        {
            using (var stm = typeof(WebService).Assembly.GetManifestResourceStream($"AspNetStateService.Samples.Web.Host.{name}"))
                return XDocument.Load(stm);
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

            await new AppHostBuilder()
                .ConfigureWeb(LoadXmlResource("Web.config"), w => w
                    .SystemWeb(s => s
                        .SessionState(z => z
                            .Mode(WebSystemWebSessionStateMode.StateServer)
                            .StateNetworkTimeout(TimeSpan.FromMinutes(2))
                            .Timeout(TimeSpan.FromMinutes(20)))))
                .ConfigureApp(LoadXmlResource("ApplicationHost.config"), c => c
                    .Site(1, s => s
                        .RemoveBindings()
                        .AddBinding("http", "*:4521:")
                        .Application("/", a => a
                            .VirtualDirectory("/", v => v.UsePhysicalPath(Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "site"))))))
                .Build()
                .RunAsync();

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
            builder.RegisterServiceFabricSupport();
            builder.RegisterStatelessService<WebService>("AspNetStateService.Samples.Web.Host");

            using (var container = builder.Build())
                await Task.Delay(Timeout.Infinite);
        }

    }

}
