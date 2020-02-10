using System.Fabric;

using AspNetStateService.AspNetCore.Kestrel;

using Autofac;

using Cogito.Autofac;
using Cogito.ServiceFabric.AspNetCore;
using Cogito.ServiceFabric.AspNetCore.Kestrel.Autofac;

using Microsoft.AspNetCore.Hosting;

namespace AspNetStateService.Fabric.Services
{

    [RegisterAs(typeof(StateActorWebService))]
    public class StateActorWebService : StatelessKestrelWebService<StateActorWebServiceStartup>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="scope"></param>
        /// <param name="endpoint"></param>
        public StateActorWebService(StatelessServiceContext context, ILifetimeScope scope, WebServiceEndpoint endpoint = null) :
            base(context, scope, endpoint)
        {

        }

        protected override IWebHostBuilder ConfigureWebHostBuilder(IWebHostBuilder builder)
        {
            return base.ConfigureWebHostBuilder(builder).UseKestrelPatch();
        }

    }

}
