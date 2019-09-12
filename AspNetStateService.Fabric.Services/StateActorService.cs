﻿using System;
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
        public StateActorService(FabricClient fabric, StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorService, ActorId, ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null) :
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
        /// Executed when the actor changes role.
        /// </summary>
        /// <param name="newRole"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task OnChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken)
        {
            await base.OnChangeRoleAsync(newRole, cancellationToken);

            // zero out load
            if (newRole != ReplicaRole.Primary)
                Partition.ReportLoad(new[] { new LoadMetric("ActiveSessionCount", 0) });
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
            var nact = 0;

            do
            {
                // get a page of actors
                var page = await StateProvider.GetActorsAsync(128, more, cancellationToken);
                more = page.ContinuationToken;

                // potentially purge actors
                foreach (var actorId in page.Items)
                    if (await TryPurgeActorAsync(actorId, cancellationToken) == false)
                        nact++;
            }
            while (more != null && cancellationToken.IsCancellationRequested == false);

            // report count of known active actors
            Partition.ReportLoad(new[] { new LoadMetric("ActiveSessionCount", nact) });
        }

        /// <summary>
        /// Attempts to purge an actor, if expired.
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<bool> TryPurgeActorAsync(ActorId actorId, CancellationToken cancellationToken)
        {
            try
            {
                return await PurgeActorAsync(actorId, cancellationToken);
            }
            catch
            {

            }

            return false;
        }

        /// <summary>
        /// Purges the given actor if expired.
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<bool> PurgeActorAsync(ActorId actorId, CancellationToken cancellationToken)
        {
            if (await IsExpiredAsync(actorId, cancellationToken))
            {
                await ((IActorService)this).DeleteActorAsync(actorId, cancellationToken);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns <c>true</c> if the given actor is expired.
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<bool> IsExpiredAsync(ActorId actorId, CancellationToken cancellationToken)
        {
            // check state directly before bothering to contact actor itself
            var altered = await StateProvider.LoadStateAsync<DateTime?>(actorId, StateActor.ALTERED_FIELD, cancellationToken) ?? DateTime.MinValue;
            var timeout = await StateProvider.LoadStateAsync<TimeSpan?>(actorId, StateActor.TIMEOUT_FIELD, cancellationToken) ?? StateActor.DEFAULT_TIMEOUT;
            if (altered < DateTime.UtcNow - timeout || altered < DateTime.UtcNow - StateActor.MAXIMUM_TIMEOUT)
                if (await ActorProxy.Create<IStateActor>(actorId).IsExpired(cancellationToken))
                    return true;

            return false;
        }

    }

}
