using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Fabric.Core;
using AspNetStateService.Fabric.Interfaces;
using AspNetStateService.Interfaces;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace AspNetStateService.Fabric.Services
{

    /// <summary>
    /// Provides <see cref="IStateObject"/> instances from Service Fabric Actors.
    /// </summary>
    class StateObjectActorProvider : IStateObjectProvider
    {

        /// <summary>
        /// Starts the provider.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the provider.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets a new state object by resolving an <see cref="ActorProxy"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<IStateObject> GetStateObjectAsync(string id, CancellationToken cancellationToken)
        {
            return Task.FromResult<IStateObject>(new StateActorProxyObject(ActorProxy.Create<IStateActor>(new ActorId(id))));
        }

    }

}
