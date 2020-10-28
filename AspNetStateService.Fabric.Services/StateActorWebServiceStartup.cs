using AspNetStateService.AspNetCore;
using AspNetStateService.Interfaces;

using Autofac;

namespace AspNetStateService.Fabric.Services
{

    public class StateActorWebServiceStartup : StateWebServiceStartup
    {

        protected override IStateObjectProvider GetStateObjectProvider(IComponentContext context)
        {
            return new StateObjectActorProvider();
        }

    }

}
