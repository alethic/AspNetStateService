using System.Threading;

using Autofac;
using Autofac.Integration.ServiceFabric;

using Cogito.Autofac;

namespace AspNetStateService.Samples.Web.Host
{

    public static class Program
    {

        /// <summary>
        /// Main application entry-point.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            System.Diagnostics.Debugger.Launch();

            var builder = new ContainerBuilder();
            builder.RegisterAllAssemblyModules();
            builder.RegisterServiceFabricSupport();
            builder.RegisterStatelessService<WebService>("AspNetStateService.Samples.Web.Host");

            using (var container = builder.Build())
                while (true)
                    Thread.Sleep(Timeout.Infinite);
        }

    }

}
