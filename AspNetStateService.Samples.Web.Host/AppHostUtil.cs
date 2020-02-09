using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using Cogito.HostedWebCore;
using Cogito.IIS.Configuration;
using Cogito.Web.Configuration;

namespace AspNetStateService.Samples.Web.Host
{

    /// <summary>
    /// Static methods to generate <see cref="AppHost"/>.
    /// </summary>
    static class AppHostUtil
    {

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

        public static AppHost BuildHost(IEnumerable<BindingData> bindings, string path)
        {
            return new AppHostBuilder()
                .ConfigureWeb(LoadXmlResource("Web.config"), w => w
                    .SystemWeb(s => s
                        .SessionState(z => z
                            .Mode(WebSystemWebSessionStateMode.StateServer)
                            .StateNetworkTimeout(TimeSpan.FromMinutes(2))
                            .Timeout(TimeSpan.FromMinutes(20)))))
                .ConfigureApp(LoadXmlResource("ApplicationHost.config"), c => c
                    .Site(1, s => s
                        .RemoveBindings()
                        .AddBindings(bindings)
                        .Application(path, a => a
                            .VirtualDirectory("/", v => v.UsePhysicalPath(Path.Combine(Path.GetDirectoryName(typeof(AppHostUtil).Assembly.Location), "site"))))))
                .Build();
        }

    }

}
