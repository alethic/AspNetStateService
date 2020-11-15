using System;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Core;

using Cogito.Autofac;
using Cogito.Threading;

using Microsoft.Extensions.Options;

using Serilog;

using StackExchange.Redis;

namespace AspNetStateService.Redis
{

    /// <summary>
    /// Implements a <see cref="IStateObjectDataStore"/> using Redis.
    /// </summary>
    [RegisterNamed(typeof(IStateObjectDataStore), "Redis")]
    [RegisterSingleInstance]
    [RegisterWithAttributeFiltering]
    public class StateObjectRedisDataStore : IStateObjectDataStore
    {

        const string DATA_KEY = "Data";
        const string EXTRA_FLAGS_KEY = "ExtraFlags";
        const string TIMEOUT_KEY = "Timeout";
        const string ALTERED_KEY = "Altered";
        const string LOCK_COOKIE_KEY = "LockCookie";
        const string LOCK_TIME_KEY = "LockTime";

        static readonly RedisValue[] GETDATA_KEYS = new RedisValue[] { DATA_KEY, EXTRA_FLAGS_KEY, TIMEOUT_KEY, ALTERED_KEY };
        static readonly RedisValue[] GETLOCK_KEYS = new RedisValue[] { LOCK_COOKIE_KEY, LOCK_TIME_KEY };
        static readonly RedisValue[] REMOVEDATA_KEYS = new RedisValue[] { DATA_KEY, EXTRA_FLAGS_KEY, TIMEOUT_KEY, ALTERED_KEY };
        static readonly RedisValue[] REMOVELOCK_KEYS = new RedisValue[] { LOCK_COOKIE_KEY, LOCK_TIME_KEY };

        readonly IRedisConnectionProvider connections;
        readonly IOptions<StateObjectRedisDataStoreOptions> options;
        readonly ILogger logger;
        readonly AsyncLock sync = new AsyncLock();

        bool started;
        IConnectionMultiplexer connection;
        IDatabase database;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="connections"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public StateObjectRedisDataStore(IRedisConnectionProvider connections, IOptions<StateObjectRedisDataStoreOptions> options, ILogger logger)
        {
            this.connections = connections ?? throw new ArgumentNullException(nameof(connections));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Starts the table store.
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

            connection = await connections.GetConnectionAsync();
            database = connection.GetDatabase(options.Value.DatabaseId ?? -1);
            started = true;
        }

        /// <summary>
        /// Stops the table store.
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
        /// Does the actual work of stopping the store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task StopImplAsync(CancellationToken cancellationToken)
        {
            logger.Verbose("StopImplAsync()");

            if (connection != null)
                connection.Dispose();

            connection = null;
            database = null;
            started = false;
        }

        public async Task<(byte[] data, uint? extraFlags, TimeSpan? timeout, DateTime? altered)> GetDataAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("GetDataAsync({Id})", id);

            var hs = await database.HashGetAsync(id, GETDATA_KEYS);
            return (hs[0], (uint?)hs[1], !hs[2].IsNullOrEmpty ? (TimeSpan?)TimeSpan.FromTicks((long)hs[2]) : null, !hs[3].IsNullOrEmpty ? (DateTime?)new DateTime((long)hs[3]) : null);
        }

        public async Task<(uint? cookie, DateTime? time)> GetLockAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("GetLockAsync({Id})", id);

            var hs = await database.HashGetAsync(id, GETLOCK_KEYS);
            return ((uint?)hs[0], !hs[1].IsNullOrEmpty ? (DateTime?)new DateTime((long)hs[1]) : null);
        }

        public async Task RemoveDataAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("RemoveDataAsync({Id})", id);

            var batch = database.CreateBatch();
            var t1 = database.HashDeleteAsync(id, REMOVEDATA_KEYS);
            var t2 = database.KeyExpireAsync(id, TimeSpan.FromMinutes(1)); batch.Execute();
            await Task.WhenAll(t1, t2);
        }

        public async Task RemoveLockAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("RemoveLockAsync({Id})", id);

            await database.HashDeleteAsync(id, REMOVELOCK_KEYS);
        }

        public async Task SetDataAsync(string id, byte[] data, uint? extraFlags, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            logger.Verbose("SetDataAsync({Id}, {ExtraFlags}, {Timeout})", id, extraFlags, timeout);

            var batch = database.CreateBatch();
            var t1 = batch.HashSetAsync(id, new HashEntry[] { new HashEntry(DATA_KEY, data), new HashEntry(EXTRA_FLAGS_KEY, extraFlags), new HashEntry(TIMEOUT_KEY, timeout?.Ticks), new HashEntry(ALTERED_KEY, DateTime.UtcNow.Ticks) });
            var t2 = batch.KeyExpireAsync(id, timeout + TimeSpan.FromMinutes(1) ?? TimeSpan.FromMinutes(20));
            batch.Execute();
            await Task.WhenAll(t1, t2);
        }

        public async Task SetFlagAsync(string id, uint? extraFlags, CancellationToken cancellationToken)
        {
            logger.Verbose("SetFlagAsync({Id}, {ExtraFlags})", id, extraFlags);

            await database.HashSetAsync(id, new HashEntry[] { new HashEntry(EXTRA_FLAGS_KEY, extraFlags) });

        }

        public async Task SetLockAsync(string id, uint cookie, DateTime time, CancellationToken cancellationToken)
        {
            logger.Verbose("SetLockAsync({Id}, {Cookie}, {Time})", id, cookie, time);

            await database.HashSetAsync(id, new HashEntry[] { new HashEntry(LOCK_COOKIE_KEY, cookie), new HashEntry(LOCK_TIME_KEY, time.Ticks) });
        }

        public async Task SetTimeoutAsync(string id, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            logger.Verbose("SetTimeoutAsync({Id}, {Timeout})", id, timeout);

            var batch = database.CreateBatch();
            var t1 = database.HashSetAsync(id, TIMEOUT_KEY, timeout?.Ticks);
            var t2 = database.KeyExpireAsync(id, timeout + TimeSpan.FromMinutes(1) ?? TimeSpan.FromMinutes(20));
            batch.Execute();
            await Task.WhenAll(t1, t2);
        }

    }

}
