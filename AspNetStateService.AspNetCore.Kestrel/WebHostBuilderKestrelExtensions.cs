using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

using System;

namespace AspNetStateService.AspNetCore.Kestrel
{

    public static class WebHostBuilderKestrelExtensions
    {

        /// <summary>
        /// Attaches the Kestrel State Server host to the web builder.
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseKestrelStateServer(this IWebHostBuilder hostBuilder)
        {
            return hostBuilder.UseKestrelStateServer<StateWebServiceStartup>();
        }

        /// <summary>
        /// Attaches the Kestrel State Server host to the web builder.
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseKestrelStateServer(this IWebHostBuilder hostBuilder, Action<KestrelServerOptions> options)
        {
            return hostBuilder.UseKestrelStateServer<StateWebServiceStartup>(options);
        }

        /// <summary>
        /// Attaches the Kestrel State Server host to the web builder.
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseKestrelStateServer<TStartup>(this IWebHostBuilder hostBuilder)
            where TStartup : StateWebServiceStartup
        {
            return hostBuilder.UseStartup<TStartup>().UseKestrel();
        }

        /// <summary>
        /// Attaches the Kestrel State Server host to the web builder.
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseKestrelStateServer<TStartup>(this IWebHostBuilder hostBuilder, Action<KestrelServerOptions> options)
            where TStartup : StateWebServiceStartup
        {
            return hostBuilder.UseStartup<TStartup>().UseKestrel(options);
        }

    }

}
