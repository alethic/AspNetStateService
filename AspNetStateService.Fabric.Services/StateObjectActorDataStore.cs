using System;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Core;

using Cogito.Autofac;

using Microsoft.ServiceFabric.Actors.Runtime;

namespace AspNetStateService.Fabric.Services
{

    /// <summary>
    /// Implements a <see cref="IStateObjectDataStore"/> against a <see cref="IActorStateManager"/>.
    /// </summary>
    class StateObjectActorDataStore : IStateObjectDataStore
    {

        readonly StateActor actor;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="actor"></param>
        public StateObjectActorDataStore(StateActor actor)
        {
            this.actor = actor ?? throw new ArgumentNullException(nameof(actor));
        }

        /// <summary>
        /// Gets the given state object from the state manager.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<T> GetStateAsync<T>(string stateName, CancellationToken cancellationToken)
        {
            return await actor.GetStateAsync<T>(stateName, cancellationToken);
        }

        /// <summary>
        /// Sets the given state value in the state manager.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateName"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task SetStateAsync<T>(string stateName, T value, CancellationToken cancellationToken)
        {
            await actor.SetStateAsync<T>(stateName, value, cancellationToken);
        }

        /// <summary>
        /// Removes the given state value in the state manager.
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RemoveStateAsync(string stateName, CancellationToken cancellationToken)
        {
            return actor.StateManager.TryRemoveStateAsync(stateName, cancellationToken);
        }

        /// <summary>
        /// Initializes the data store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task InitAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the data fields.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="extraFlags"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SetDataAsync(string id, byte[] data, uint? extraFlags, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            await SetStateAsync(StateActor.DATA_FIELD, data, cancellationToken);
            await SetStateAsync(StateActor.EXTRA_FLAGS_FIELD, extraFlags, cancellationToken);
            await SetStateAsync(StateActor.TIMEOUT_FIELD, timeout, cancellationToken);
            await SetStateAsync(StateActor.ALTERED_FIELD, DateTime.UtcNow, cancellationToken);
        }

        /// <summary>
        /// Sets the extra flags field.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="extraFlags"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SetFlagAsync(string id, uint? extraFlags, CancellationToken cancellationToken)
        {
            await SetStateAsync(StateActor.EXTRA_FLAGS_FIELD, extraFlags, cancellationToken);
            await SetStateAsync(StateActor.ALTERED_FIELD, DateTime.UtcNow, cancellationToken);
        }

        /// <summary>
        /// Gets the current data fields.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<(byte[] data, uint? extraFlags, TimeSpan? timeout, DateTime? altered)> GetDataAsync(string id, CancellationToken cancellationToken)
        {
            var d = await GetStateAsync<byte[]>(StateActor.DATA_FIELD, cancellationToken);
            var f = await GetStateAsync<uint?>(StateActor.EXTRA_FLAGS_FIELD, cancellationToken);
            var t = await GetStateAsync<TimeSpan?>(StateActor.TIMEOUT_FIELD, cancellationToken);
            var u = await GetStateAsync<DateTime?>(StateActor.ALTERED_FIELD, cancellationToken);
            return (d, f, t, u);
        }

        /// <summary>
        /// Removes the current data.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task RemoveDataAsync(string id, CancellationToken cancellationToken)
        {
            await RemoveStateAsync(StateActor.DATA_FIELD, cancellationToken);
            await RemoveStateAsync(StateActor.EXTRA_FLAGS_FIELD, cancellationToken);
            await RemoveStateAsync(StateActor.TIMEOUT_FIELD, cancellationToken);
            await RemoveStateAsync(StateActor.ALTERED_FIELD, cancellationToken);
        }

        /// <summary>
        /// Gets the current lock fields.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<(uint? cookie, DateTime? time)> GetLockAsync(string id, CancellationToken cancellationToken)
        {
            var l = await GetStateAsync<uint?>(StateActor.LOCK_COOKIE_FIELD, cancellationToken);
            var c = await GetStateAsync<DateTime?>(StateActor.LOCK_TIME_FIELD, cancellationToken);
            return (l, c);
        }

        /// <summary>
        /// Sets the current lock fields.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cookie"></param>
        /// <param name="time"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SetLockAsync(string id, uint cookie, DateTime time, CancellationToken cancellationToken)
        {
            await SetStateAsync(StateActor.LOCK_COOKIE_FIELD, cookie, cancellationToken);
            await SetStateAsync(StateActor.LOCK_TIME_FIELD, time, cancellationToken);
        }

        /// <summary>
        /// Removes the current lock.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task RemoveLockAsync(string id, CancellationToken cancellationToken)
        {
            await RemoveStateAsync(StateActor.LOCK_COOKIE_FIELD, cancellationToken);
            await RemoveStateAsync(StateActor.LOCK_TIME_FIELD, cancellationToken);
        }

        /// <summary>
        /// Invoked to register the expire at time. The implementation can register a timer to trigger at this time to expire state.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetTimeoutAsync(string id, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            // no implementation, we scan in the actor service
            return Task.CompletedTask;
        }

    }

}
