using System.Linq;
using System.Reflection;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetStateService.AspNetCore.Kestrel
{

    public static class WebHostBuilderKestrelExtensions
    {

        /// <summary>
        /// Applies the Kestrel server patch for out-of-spec HTTP requests from the Microsoft Session State module.
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseKestrelPatch(this IWebHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(s =>
                {
                    s.Remove(s.First(i => i.ImplementationType == typeof(KestrelServer)));
                    s.AddSingleton<KestrelServer>();
                    s.AddSingleton<IServer>(p => PatchKestrel(p.GetRequiredService<KestrelServer>()));
                });
        }

        /// <summary>
        /// Patches the <see cref="KestrelServer"/> instance by replacing the HttpParser.
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        static KestrelServer PatchKestrel(KestrelServer server)
        {
            var ctx = typeof(KestrelServer).GetProperty("ServiceContext", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(server);
            typeof(ServiceContext).GetProperty(nameof(ServiceContext.HttpParser)).SetValue(ctx, new HttpParserWithPatch<Http1ParsingHandler>());
            return server;
        }

    }

}
