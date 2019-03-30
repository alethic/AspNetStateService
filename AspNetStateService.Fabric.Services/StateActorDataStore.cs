using System;
using System.Threading.Tasks;

using AspNetStateService.Core;

using Microsoft.ServiceFabric.Actors.Runtime;

namespace AspNetStateService.Fabric.Services
{

    /// <summary>
    /// Implements a <see cref="IStateObjectDataStore"/> against a <see cref="IActorStateManager"/>.
    /// </summary>
    public class StateActorDataStore : IStateObjectDataStore
    {

        readonly StateActor actor;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="actor"></param>
        public StateActorDataStore(StateActor actor)
        {
            this.actor = actor ?? throw new ArgumentNullException(nameof(actor));
        }

        /// <summary>
        /// Gets the given state object from the state manager.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateName"></param>
        /// <returns></returns>
        async Task<T> GetStateAsync<T>(string stateName)
        {
            return await actor.GetStateAsync<T>(stateName);
        }

        /// <summary>
        /// Sets the given state value in the state manager.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        async Task SetStateAsync<T>(string stateName, T value)
        {
            await actor.SetStateAsync<T>(stateName, value);
        }

        /// <summary>
        /// Removes the given state value in the state manager.
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        Task RemoveStateAsync(string stateName)
        {
            return actor.StateManager.TryRemoveStateAsync(stateName);
        }

        /// <summary>
        /// Sets the data fields.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="extraFlags"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task SetDataAsync(string id, byte[] data, uint? extraFlags, TimeSpan? timeout)
        {
            await SetStateAsync(StateActor.DATA_FIELD, data);
            await SetStateAsync(StateActor.EXTRA_FLAGS_FIELD, extraFlags);
            await SetStateAsync(StateActor.TIMEOUT_FIELD, timeout);
            await SetStateAsync(StateActor.ALTERED_FIELD, DateTime.UtcNow);
        }

        /// <summary>
        /// Sets the extra flags field.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="extraFlags"></param>
        /// <returns></returns>
        public async Task SetFlagAsync(string id, uint? extraFlags)
        {
            await SetStateAsync(StateActor.EXTRA_FLAGS_FIELD, extraFlags);
            await SetStateAsync(StateActor.ALTERED_FIELD, DateTime.UtcNow);
        }

        /// <summary>
        /// Gets the current data fields.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(byte[] data, uint? extraFlags, TimeSpan? timeout, DateTime? altered)> GetDataAsync(string id)
        {
            var d = await GetStateAsync<byte[]>(StateActor.DATA_FIELD);
            var f = await GetStateAsync<uint?>(StateActor.EXTRA_FLAGS_FIELD);
            var t = await GetStateAsync<TimeSpan?>(StateActor.TIMEOUT_FIELD);
            var u = await GetStateAsync<DateTime?>(StateActor.ALTERED_FIELD);
            return (d, f, t, u);
        }

        /// <summary>
        /// Removes the current data.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task RemoveDataAsync(string id)
        {
            await RemoveStateAsync(StateActor.DATA_FIELD);
            await RemoveStateAsync(StateActor.EXTRA_FLAGS_FIELD);
            await RemoveStateAsync(StateActor.TIMEOUT_FIELD);
            await RemoveStateAsync(StateActor.ALTERED_FIELD);
        }

        /// <summary>
        /// Gets the current lock fields.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(uint? cookie, DateTime? time)> GetLockAsync(string id)
        {
            var l = await GetStateAsync<uint?>(StateActor.LOCK_COOKIE_FIELD);
            var c = await GetStateAsync<DateTime?>(StateActor.LOCK_TIME_FIELD);
            return (l, c);
        }

        /// <summary>
        /// Sets the current lock fields.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cookie"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public async Task SetLockAsync(string id, uint cookie, DateTime time)
        {
            await SetStateAsync(StateActor.LOCK_COOKIE_FIELD, cookie);
            await SetStateAsync(StateActor.LOCK_TIME_FIELD, time);
        }

        /// <summary>
        /// Removes the current lock.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task RemoveLockAsync(string id)
        {
            await RemoveStateAsync(StateActor.LOCK_COOKIE_FIELD);
            await RemoveStateAsync(StateActor.LOCK_TIME_FIELD);
        }

        /// <summary>
        /// Invoked to register the expire at time. The implementation can register a timer to trigger at this time to expire state.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Task SetTimeoutAsync(string id, TimeSpan? timeout)
        {
            // no implementation, we scan in the actor service
            return Task.CompletedTask;
        }

    }

}
