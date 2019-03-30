using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Fabric.Interfaces;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Query;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace AspNetStateService.Fabric.Services
{

    public class StateActorService : ActorService
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="actorTypeInfo"></param>
        /// <param name="actorFactory"></param>
        /// <param name="stateManagerFactory"></param>
        /// <param name="stateProvider"></param>
        /// <param name="settings"></param>
        public StateActorService(StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorService, ActorId, ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null) :
            base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {
            
        }

        /// <summary>
        /// Executed while the actor service is running.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await base.RunAsync(cancellationToken);

            while (cancellationToken.IsCancellationRequested == false)
            {
                await TryPurgeActorsAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }

        /// <summary>
        /// Attempts to check each actor for an expired condition and deletes.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task TryPurgeActorsAsync(CancellationToken cancellationToken)
        {
            try
            {
                await PurgeActorsAsync(cancellationToken);
            }
            catch
            {
                // we tried
            }
        }

        /// <summary>
        /// Checks each actor for expiration and purges if allowed.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task PurgeActorsAsync(CancellationToken cancellationToken)
        {
            var more = (ContinuationToken)null;

            do
            {
                // get a page of actors
                var page = await StateProvider.GetActorsAsync(128, more, cancellationToken);
                more = page.ContinuationToken;

                // potentially purge actors
                foreach (var actorId in page.Items)
                    await TryPurgeActorAsync(actorId, cancellationToken);
            }
            while (more != null && cancellationToken.IsCancellationRequested == false);
        }

        /// <summary>
        /// Attempts to purge an actor, if expired.
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task TryPurgeActorAsync(ActorId actorId, CancellationToken cancellationToken)
        {
            try
            {
                await PurgeActorAsync(actorId, cancellationToken);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Purges the given actor if expired.
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task PurgeActorAsync(ActorId actorId, CancellationToken cancellationToken)
        {
            if (await ActorProxy.Create<IStateActor>(actorId).IsExpired())
                await ((IActorService)this).DeleteActorAsync(actorId, cancellationToken);
        }

    }

}
