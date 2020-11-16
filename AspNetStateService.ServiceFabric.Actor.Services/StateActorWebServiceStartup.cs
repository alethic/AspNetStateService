using AspNetStateService.AspNetCore;
using AspNetStateService.Interfaces;

using Autofac;

namespace AspNetStateService.ServiceFabric.Actor.Services
{

    public class StateActorWebServiceStartup : StateWebServiceStartup
    {

        protected override IStateObjectProvider GetStateObjectProvider(IComponentContext context)
        {
            return new StateObjectActorProvider();
        }

    }

}
