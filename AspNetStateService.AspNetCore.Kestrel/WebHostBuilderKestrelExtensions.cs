using System;
using System.Linq;
using System.Reflection;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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
            var ctxType = typeof(KestrelServer).Assembly.GetType("Microsoft.AspNetCore.Server.Kestrel.Core.Internal.ServiceContext");
            var hndType = typeof(KestrelServer).Assembly.GetType("Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.Http1ParsingHandler");
            var prsType = typeof(HttpParserWithPatch<>).MakeGenericType(hndType);
            var ctxProp = typeof(KestrelServer).GetProperty("ServiceContext", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(server);
            ctxType.GetProperty("HttpParser").SetValue(ctxProp, Activator.CreateInstance(prsType));
            return server;
        }

    }

}
