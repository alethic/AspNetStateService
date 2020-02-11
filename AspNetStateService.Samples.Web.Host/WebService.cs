using System.Collections.Generic;
using System.Fabric;

using Cogito.HostedWebCore.ServiceFabric;
using Cogito.ServiceFabric.Services.Autofac;

using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace AspNetStateService.Samples.Web.Host
{

    [RegisterStatelessService("AspNetStateService.Samples.Web")]
    public class WebService : Cogito.ServiceFabric.Services.StatelessService
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public WebService(StatelessServiceContext context) :
            base(context)
        {

        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            yield return new ServiceInstanceListener(ctx =>
                new AppHostCommunicationListener(ctx, "ServiceEndpoint", (bindings, path, _) =>
                    AppHostUtil.BuildHost(bindings, path)));
        }

    }

}
