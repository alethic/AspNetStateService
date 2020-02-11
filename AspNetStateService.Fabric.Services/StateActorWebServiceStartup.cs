using System;

using AspNetStateService.AspNetCore;
using AspNetStateService.Interfaces;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Cogito.Autofac;

using Microsoft.Extensions.DependencyInjection;

namespace AspNetStateService.Fabric.Services
{

    [RegisterAs(typeof(StateActorWebServiceStartup))]
    public class StateActorWebServiceStartup : StateWebServiceStartup
    {

        readonly ILifetimeScope parent;
        ILifetimeScope scope;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parent"></param>
        public StateActorWebServiceStartup(ILifetimeScope parent)
        {
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        protected override IStateObjectProvider GetStateObjectProvider(IComponentContext context)
        {
            return new StateObjectActorProvider();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return new AutofacServiceProvider(scope = parent.BeginLifetimeScope(builder => builder.Populate(services)));
        }

    }

}
