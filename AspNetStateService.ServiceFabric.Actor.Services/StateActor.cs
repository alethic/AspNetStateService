using System;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Core;
using AspNetStateService.Interfaces;
using AspNetStateService.ServiceFabric.Actor.Interfaces;

using Cogito.ServiceFabric.Actors.Autofac;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

using Serilog;

namespace AspNetStateService.ServiceFabric.Actor.Services
{

    /// <summary>
    /// Hosts the <see cref="StateObject"/> in a Service Fabric Actor, using the Actor state.
    /// </summary>
    [RegisterActor(typeof(StateActorService))]
    [StatePersistence(StatePersistence.Volatile)]
    public class StateActor : Microsoft.ServiceFabric.Actors.Runtime.Actor, IStateActor
    {

        public static readonly TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromMinutes(20);
        public static readonly TimeSpan MAXIMUM_TIMEOUT = TimeSpan.FromHours(4);

        public const string DATA_FIELD = "Data";
        public const string EXTRA_FLAGS_FIELD = "ExtraFlags";
        public const string TIMEOUT_FIELD = "Timeout";
        public const string ALTERED_FIELD = "Altered";
        public const string LOCK_COOKIE_FIELD = "Lock-Cookie";
        public const string LOCK_TIME_FIELD = "Lock-Time";

        readonly IStateObject state;
        readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="actorService"></param>
        /// <param name="actorId"></param>
        /// <param name="logger"></param>
        public StateActor(ActorService actorService, ActorId actorId, ILogger logger) :
            base(actorService, actorId)
        {
            this.state = new StateObject(actorId.GetStringId(), new StateObjectActorDataStore(this), logger);
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the given state object from the state manager.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> GetStateAsync<T>(string stateName, CancellationToken cancellationToken)
        {
            var v = await StateManager.TryGetStateAsync<T>(stateName, cancellationToken);
            return v.HasValue ? v.Value : default;
        }

        /// <summary>
        /// Sets the given state value in the state manager.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateName"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetStateAsync<T>(string stateName, T value, CancellationToken cancellationToken)
        {
            return StateManager.SetStateAsync(stateName, value, cancellationToken);
        }

        /// <summary>
        /// Processes a non-exclusive get request.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<DataResponse> Get(CancellationToken cancellationToken)
        {
            return state.Get(cancellationToken);
        }

        /// <summary>
        /// Processes an exclusive get request.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<DataResponse> GetExclusive(CancellationToken cancellationToken)
        {
            return state.GetExclusive(cancellationToken);
        }

        public Task<Response> Set(uint? cookie, byte[] data, uint? extraFlags, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            return state.Set(cookie, data, extraFlags, timeout, cancellationToken);
        }

        public Task<Response> ReleaseExclusive(uint cookie, CancellationToken cancellationToken)
        {
            return state.ReleaseExclusive(cookie, cancellationToken);
        }

        public Task<Response> Remove(uint? cookie, CancellationToken cancellationToken)
        {
            return state.Remove(cookie, cancellationToken);
        }

        public Task<Response> ResetTimeout(CancellationToken cancellationToken)
        {
            return state.ResetTimeout(cancellationToken);
        }

        public async Task<bool> IsExpired(CancellationToken cancellationToken)
        {
            var altered = await GetStateAsync<DateTime?>(ALTERED_FIELD, cancellationToken) ?? DateTime.MinValue;
            var timeout = await GetStateAsync<TimeSpan?>(TIMEOUT_FIELD, cancellationToken) ?? DEFAULT_TIMEOUT;
            return altered < DateTime.UtcNow - timeout || altered < DateTime.UtcNow - MAXIMUM_TIMEOUT;
        }

    }

}
