using AspNetStateService.AspNetCore;
using AspNetStateService.Core;
using AspNetStateService.Interfaces;

using Autofac;

namespace AspNetStateService.ServiceFabric.KeyShift.Services
{

    public class StateKeyShiftServiceStartup : StateWebServiceStartup
    {

        protected override IStateObjectProvider GetStateObjectProvider(IComponentContext context)
        {
            return context.Resolve<IStateObjectProvider>(TypedParameter.From(context.ResolveNamed<IStateObjectDataStore>("KeyShift")));
        }

    }

}
