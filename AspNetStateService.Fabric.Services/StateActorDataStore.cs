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

        const string DATA_FIELD = "Data";
        const string EXTRA_FLAGS_FIELD = "ExtraFlags";
        const string TIMEOUT_FIELD = "Timeout";
        const string TOUCH_FIELD = "Touch";
        const string LOCK_COOKIE_FIELD = "Lock-Cookie";
        const string LOCK_TIME_FIELD = "Lock-Time";

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
            var v = await actor.StateManager.TryGetStateAsync<T>(stateName);
            return v.HasValue ? v.Value : default;
        }

        /// <summary>
        /// Sets the given state value in the state manager.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task SetStateAsync<T>(string stateName, T value)
        {
            return actor.StateManager.SetStateAsync(stateName, value);
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
        /// <param name="data"></param>
        /// <param name="extraFlags"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task SetDataAsync(byte[] data, uint? extraFlags, TimeSpan? timeout)
        {
            await SetStateAsync(DATA_FIELD, data);
            await SetStateAsync(EXTRA_FLAGS_FIELD, extraFlags);
            await SetStateAsync(TIMEOUT_FIELD, timeout);
            await SetStateAsync(TOUCH_FIELD, DateTime.Now);
        }

        /// <summary>
        /// Sets the extra flags field.
        /// </summary>
        /// <param name="extraFlags"></param>
        /// <returns></returns>
        public async Task SetFlagAsync(uint? extraFlags)
        {
            await SetStateAsync(EXTRA_FLAGS_FIELD, extraFlags);
        }

        /// <summary>
        /// Gets the current data fields.
        /// </summary>
        /// <returns></returns>
        public async Task<(byte[] data, uint? extraFlags, TimeSpan? timeout, DateTime? touch)> GetDataAsync()
        {
            var d = await GetStateAsync<byte[]>(DATA_FIELD);
            var f = await GetStateAsync<uint?>(EXTRA_FLAGS_FIELD);
            var t = await GetStateAsync<TimeSpan?>(TIMEOUT_FIELD);
            var u = await GetStateAsync<DateTime?>(TOUCH_FIELD);
            return (d, f, t, u);
        }

        /// <summary>
        /// Removes the current data.
        /// </summary>
        /// <returns></returns>
        public async Task RemoveDataAsync()
        {
            await RemoveStateAsync(DATA_FIELD);
            await RemoveStateAsync(EXTRA_FLAGS_FIELD);
            await RemoveStateAsync(TIMEOUT_FIELD);
            await RemoveStateAsync(TOUCH_FIELD);
        }

        /// <summary>
        /// Gets the current lock fields.
        /// </summary>
        /// <returns></returns>
        public async Task<(uint? cookie, DateTime? time)> GetLockAsync()
        {
            var l = await GetStateAsync<uint?>(LOCK_COOKIE_FIELD);
            var c = await GetStateAsync<DateTime?>(LOCK_TIME_FIELD);
            return (l, c);
        }

        /// <summary>
        /// Sets the current lock fields.
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public async Task SetLockAsync(uint cookie, DateTime update)
        {
            await SetStateAsync(LOCK_COOKIE_FIELD, cookie);
            await SetStateAsync(LOCK_TIME_FIELD, update);
        }

        /// <summary>
        /// Removes the current lock.
        /// </summary>
        /// <returns></returns>
        public async Task RemoveLockAsync()
        {
            await RemoveStateAsync(LOCK_COOKIE_FIELD);
            await RemoveStateAsync(LOCK_TIME_FIELD);
        }

        /// <summary>
        /// Invoked to register the expire at time. The implementation can register a timer to trigger at this time to expire state.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task SetTimeoutAsync(TimeSpan? timeout)
        {
            await actor.SetTimeoutAsync(timeout);
        }

    }

}
