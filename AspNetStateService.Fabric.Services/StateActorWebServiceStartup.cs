using AspNetStateService.AspNetCore;
using AspNetStateService.Interfaces;

using Autofac;

using Cogito.Autofac;

using Microsoft.Extensions.Options;

namespace AspNetStateService.Fabric.Services
{

    [RegisterAs(typeof(StateActorWebServiceStartup))]
    public class StateActorWebServiceStartup : StateWebServiceStartup
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="options"></param>
        public StateActorWebServiceStartup(ILifetimeScope parent, IOptions<StateWebServiceOptions> options) :
            base(parent, options)
        {

        }

        protected override IStateObjectProvider GetStateObjectProvider(IComponentContext context)
        {
            return new StateObjectActorProvider();
        }

    }

}
