﻿using System.Fabric;

using AspNetStateService.AspNetCore.Kestrel;

using Autofac;

using Cogito.ServiceFabric;
using Cogito.ServiceFabric.AspNetCore.Kestrel.Autofac;
using Cogito.ServiceFabric.Services.Autofac;

using Microsoft.AspNetCore.Hosting;

namespace AspNetStateService.ServiceFabric.Actor.Services
{

    [RegisterStatelessService("StateWebService", DefaultEndpointName = "HttpServiceEndpoint")]
    public class StateActorWebService : StatelessKestrelWebService<StateActorWebServiceStartup>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="scope"></param>
        /// <param name="endpoint"></param>
        public StateActorWebService(StatelessServiceContext context, ILifetimeScope scope, DefaultServiceEndpoint endpoint = null) :
            base(context, scope, endpoint)
        {

        }

        protected override IWebHostBuilder ConfigureWebHostBuilder(IWebHostBuilder builder)
        {
            return base.ConfigureWebHostBuilder(builder).UseKestrelStateServer<StateActorWebServiceStartup>();
        }

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            base.ConfigureContainer(builder);
        }

    }

}
