using System;
using System.Threading.Tasks;

using AspNetStateService.Core;
using AspNetStateService.Fabric.Interfaces;
using AspNetStateService.Interfaces;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace AspNetStateService.Fabric.Services
{

    /// <summary>
    /// Hosts the <see cref="StateObject"/> in a Service Fabric Actor, using the Actor state.
    /// </summary>
    [StatePersistence(StatePersistence.Volatile)]
    public class StateActor : Actor, IStateActor
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

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="actorService"></param>
        /// <param name="actorId"></param>
        public StateActor(ActorService actorService, ActorId actorId) :
            base(actorService, actorId)
        {
            this.state = new StateObject(actorId.GetStringId(), new StateActorDataStore(this));
        }

        /// <summary>
        /// Gets the given state object from the state manager.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public async Task<T> GetStateAsync<T>(string stateName)
        {
            var v = await StateManager.TryGetStateAsync<T>(stateName);
            return v.HasValue ? v.Value : default;
        }

        /// <summary>
        /// Sets the given state value in the state manager.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task SetStateAsync<T>(string stateName, T value)
        {
            return StateManager.SetStateAsync(stateName, value);
        }

        /// <summary>
        /// Processes a non-exclusive get request.
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public Task<DataResponse> Get()
        {
            return state.Get();
        }

        /// <summary>
        /// Processes an exclusive get request.
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public Task<DataResponse> GetExclusive()
        {
            return state.GetExclusive();
        }

        public Task<Response> Set(uint? cookie, byte[] data, uint? extraFlags, TimeSpan? timeout)
        {
            return state.Set(cookie, data, extraFlags, timeout);
        }

        public Task<Response> ReleaseExclusive(uint cookie)
        {
            return state.ReleaseExclusive(cookie);
        }

        public Task<Response> Remove(uint? cookie)
        {
            return state.Remove(cookie);
        }

        public Task<Response> ResetTimeout()
        {
            return state.ResetTimeout();
        }

        public async Task<bool> IsExpired()
        {
            var altered = await GetStateAsync<DateTime?>(ALTERED_FIELD) ?? DateTime.MinValue;
            var timeout = await GetStateAsync<TimeSpan?>(TIMEOUT_FIELD) ?? DEFAULT_TIMEOUT;
            return altered < DateTime.UtcNow - timeout || altered < DateTime.UtcNow - MAXIMUM_TIMEOUT;
        }

    }

}
