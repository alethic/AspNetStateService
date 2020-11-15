using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Alethic.Kademlia;
using Alethic.KeyShift;

using AspNetStateService.Core;

using Cogito.Autofac;
using Cogito.Threading;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using Serilog;

namespace AspNetStateService.KeyShift
{

    /// <summary>
    /// Implements a <see cref="IStateObjectDataStore"/> using KeyShift.
    /// </summary>
    [RegisterNamed(typeof(IStateObjectDataStore), "KeyShift")]
    [RegisterSingleInstance]
    [RegisterWithAttributeFiltering]
    public class StateObjectKeyShiftDataStore : IStateObjectDataStore
    {

        readonly KHostedService kademlia;
        readonly IKsHost<string> keyshift;
        readonly IOptions<StateObjectKeyShiftDataStoreOptions> options;
        readonly ILogger logger;
        readonly AsyncLock sync = new AsyncLock();

        bool started;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="kademlia"></param>
        /// <param name="keyshift"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public StateObjectKeyShiftDataStore(KHostedService kademlia, IKsHost<string> keyshift, IOptions<StateObjectKeyShiftDataStoreOptions> options, ILogger logger)
        {
            this.kademlia = kademlia ?? throw new ArgumentNullException(nameof(kademlia));
            this.keyshift = keyshift ?? throw new ArgumentNullException(nameof(keyshift));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Starts the store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.Verbose("StartAsync()");

            if (started == false)
                using (await sync.LockAsync())
                    if (started == false)
                        await StartImplAsync(cancellationToken);
        }

        /// <summary>
        /// Does the actual work of starting the store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task StartImplAsync(CancellationToken cancellationToken)
        {
            logger.Verbose("StartImplAsync()");
            await kademlia.StartAsync(cancellationToken);
            started = true;
        }

        /// <summary>
        /// Stops the store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Verbose("StopAsync()");

            if (started)
                using (await sync.LockAsync())
                    if (started)
                        await StopImplAsync(cancellationToken);
        }

        /// <summary>
        /// Does the actual work of stoping the store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task StopImplAsync(CancellationToken cancellationToken)
        {
            logger.Verbose("StopImplAsync()");
            await kademlia.StopAsync(cancellationToken);
            started = false;
        }

        /// <summary>
        /// Gets the current state.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<StateObjectEntity> GetStateObjectAsync(string id, CancellationToken cancellationToken)
        {
            var o = await keyshift.GetAsync(id, cancellationToken);
            if (o == null)
                return null;

            using var r = new JsonTextReader(new StreamReader(new MemoryStream(o)));
            return JsonSerializer.CreateDefault().Deserialize<StateObjectEntity>(r);
        }

        /// <summary>
        /// Sets the current state.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task SetStateObjectAsync(string id, StateObjectEntity state, CancellationToken cancellationToken)
        {
            using (var s = new MemoryStream())
            using (var w = new JsonTextWriter(new StreamWriter(s)))
            {
                JsonSerializer.CreateDefault().Serialize(w, state);
                await w.FlushAsync(cancellationToken);
                await keyshift.SetAsync(id, s.ToArray(), cancellationToken);
            }
        }

        public async Task<(byte[] data, uint? extraFlags, TimeSpan? timeout, DateTime? altered)> GetDataAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("GetDataAsync({Id})", id);

            var value = await GetStateObjectAsync(id, cancellationToken);
            return (value?.Data, value?.ExtraFlags, value?.Timeout, value?.Altered);
        }

        public async Task<(uint? cookie, DateTime? time)> GetLockAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("GetLockAsync({Id})", id);

            var value = await GetStateObjectAsync(id, cancellationToken);
            return (value?.LockCookie, value?.LockTime);
        }

        public async Task RemoveDataAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("RemoveDataAsync({Id})", id);

            var value = await GetStateObjectAsync(id, cancellationToken);
            if (value != null)
            {
                value.Data = null;
                value.ExtraFlags = null;
                value.Timeout = null;
                value.Altered = null;
            }

            await SetStateObjectAsync(id, value, cancellationToken);
        }

        public async Task RemoveLockAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("RemoveLockAsync({Id})", id);

            var value = await GetStateObjectAsync(id, cancellationToken);
            if (value != null)
            {
                value.LockCookie = null;
                value.LockTime = null;
            }

            await SetStateObjectAsync(id, value, cancellationToken);
        }

        public async Task SetDataAsync(string id, byte[] data, uint? extraFlags, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            logger.Verbose("SetDataAsync({Id}, {ExtraFlags}, {Timeout})", id, extraFlags, timeout);

            var value = await GetStateObjectAsync(id, cancellationToken);
            if (value == null)
                value = new StateObjectEntity();

            value.Data = data;
            value.ExtraFlags = extraFlags;
            value.Timeout = timeout;
            value.Altered = DateTime.UtcNow;

            await SetStateObjectAsync(id, value, cancellationToken);
        }

        public async Task SetFlagAsync(string id, uint? extraFlags, CancellationToken cancellationToken)
        {
            logger.Verbose("SetFlagAsync({Id}, {ExtraFlags})", id, extraFlags);

            var value = await GetStateObjectAsync(id, cancellationToken);
            if (value == null)
                value = new StateObjectEntity();

            value.ExtraFlags = extraFlags;

            await SetStateObjectAsync(id, value, cancellationToken);

        }

        public async Task SetLockAsync(string id, uint cookie, DateTime time, CancellationToken cancellationToken)
        {
            logger.Verbose("SetLockAsync({Id}, {Cookie}, {Time})", id, cookie, time);

            var value = await GetStateObjectAsync(id, cancellationToken);
            if (value == null)
                value = new StateObjectEntity();

            value.LockCookie = cookie;
            value.LockTime = time;

            await SetStateObjectAsync(id, value, cancellationToken);
        }

        public async Task SetTimeoutAsync(string id, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            logger.Verbose("SetTimeoutAsync({Id}, {Timeout})", id, timeout);

            var value = await GetStateObjectAsync(id, cancellationToken);
            if (value == null)
                value = new StateObjectEntity();

            value.Timeout = timeout;

            await SetStateObjectAsync(id, value, cancellationToken);
        }

    }

}
