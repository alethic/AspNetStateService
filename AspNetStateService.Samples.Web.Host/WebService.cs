using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Xml.Linq;

using Cogito.HostedWebCore;
using Cogito.HostedWebCore.ServiceFabric;
using Cogito.ServiceFabric;
using Cogito.Web.Configuration;

using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace AspNetStateService.Samples.Web.Host
{

    public class WebService : StatelessService
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public WebService(StatelessServiceContext context) :
            base(context)
        {

        }

        /// <summary>
        /// Loads the XML resource with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        XDocument LoadXmlResource(string name)
        {
            using (var stm = typeof(WebService).Assembly.GetManifestResourceStream($"AspNetStateService.Samples.Web.Host.{name}"))
                return XDocument.Load(stm);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            yield return new ServiceInstanceListener(ctx =>
                new AppHostCommunicationListener(ctx, "ServiceEndpoint", (protocol, bindingInformation, path, listener) =>
                    new AppHostBuilder()
                        .ConfigureWeb(LoadXmlResource("Web.config"), w => w
                            .SystemWeb(s => s
                                .Configure("sessionState", c => c.SetAttributeValue("mode", FabricEnvironment.IsFabric ? "StateServer" : "StateServer"))))
                        .ConfigureApp(LoadXmlResource("ApplicationHost.config"), c => c
                            .Site(1, s => s
                                .RemoveBindings()
                                .AddBinding(protocol, bindingInformation)
                                .Application(path, a => a
                                    .VirtualDirectory("/", v => v.UsePhysicalPath(Path.Combine(Path.GetDirectoryName(typeof(WebService).Assembly.Location), "site"))))))
                        .Build()));
        }

    }

}
