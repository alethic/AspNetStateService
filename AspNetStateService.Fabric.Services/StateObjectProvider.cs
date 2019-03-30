using System.Threading.Tasks;

using AspNetStateService.Fabric.Core;
using AspNetStateService.Fabric.Interfaces;
using AspNetStateService.Interfaces;

using Cogito.Autofac;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace AspNetStateService.Fabric.Services
{

    /// <summary>
    /// Provides <see cref="IStateObject"/> instances from Service Fabric Actors.
    /// </summary>
    [RegisterAs(typeof(IStateObjectProvider))]
    public class StateObjectProvider : IStateObjectProvider
    {

        /// <summary>
        /// Gets a new state object by resolving an <see cref="ActorProxy"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<IStateObject> GetStateObjectAsync(string id)
        {
            return Task.FromResult<IStateObject>(new StateActorProxyObject(ActorProxy.Create<IStateActor>(new ActorId(id))));
        }

    }

}
