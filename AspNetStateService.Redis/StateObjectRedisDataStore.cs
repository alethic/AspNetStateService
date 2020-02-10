using System;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Core;

using Cogito.Autofac;

using Microsoft.Extensions.Options;

using Serilog;

using StackExchange.Redis;

namespace AspNetStateService.Redis
{

    /// <summary>
    /// Implements a <see cref="IStateObjectDataStore"/> using Redis.
    /// </summary>
    [RegisterAs(typeof(IStateObjectDataStore))]
    [RegisterWithAttributeFiltering]
    public class StateObjectRedisDataStore : IStateObjectDataStore
    {

        readonly IRedisConnectionProvider connections;
        readonly IOptions<StateObjectRedisDataStoreOptions> options;
        readonly ILogger logger;

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
        /// Initializes the table store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task InitAsync(CancellationToken cancellationToken)
        {
            logger.Verbose("InitAsync()");

            connection = await connections.GetConnectionAsync();
            database = connection.GetDatabase(options.Value.DatabaseId ?? -1);
        }

        public async Task<(byte[] data, uint? extraFlags, TimeSpan? timeout, DateTime? altered)> GetDataAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("GetDataAsync({Id})", id);

            var hs = await database.HashGetAsync(id, new RedisValue[] { "Data", "ExtraFlags", "Timeout", "Altered" });
            return (hs[0], (uint?)hs[1], !hs[2].IsNullOrEmpty ? (TimeSpan?)TimeSpan.FromTicks((long)hs[2]) : null, !hs[3].IsNullOrEmpty ? (DateTime?)new DateTime((long)hs[3]) : null);
        }

        public async Task<(uint? cookie, DateTime? time)> GetLockAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("GetLockAsync({Id})", id);

            var hs = await database.HashGetAsync(id, new RedisValue[] { "LockCookie", "LockTime" });
            return ((uint?)hs[0], !hs[1].IsNullOrEmpty ? (DateTime?)new DateTime((long)hs[1]) : null);
        }

        public async Task RemoveDataAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("RemoveDataAsync({Id})", id);

            var batch = database.CreateBatch();
            var t1 = database.HashDeleteAsync(id, new RedisValue[] { "Data", "ExtraFlags", "Timeout", "Altered" });
            var t2 = database.KeyExpireAsync(id, TimeSpan.FromMinutes(1)); batch.Execute();
            await Task.WhenAll(t1, t2);
        }

        public async Task RemoveLockAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("RemoveLockAsync({Id})", id);

            await database.HashDeleteAsync(id, new RedisValue[] { "LockCookie", "LockTime" });
        }

        public async Task SetDataAsync(string id, byte[] data, uint? extraFlags, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            logger.Verbose("SetDataAsync({Id}, {ExtraFlags}, {Timeout})", id, extraFlags, timeout);

            var batch = database.CreateBatch();
            var t1 = batch.HashSetAsync(id, new HashEntry[] { new HashEntry("Data", data), new HashEntry("ExtraFlags", extraFlags), new HashEntry("Timeout", timeout?.Ticks), new HashEntry("Altered", DateTime.UtcNow.Ticks) });
            var t2 = batch.KeyExpireAsync(id, timeout + TimeSpan.FromMinutes(1) ?? TimeSpan.FromMinutes(20));
            batch.Execute();
            await Task.WhenAll(t1, t2);
        }

        public async Task SetFlagAsync(string id, uint? extraFlags, CancellationToken cancellationToken)
        {
            logger.Verbose("SetFlagAsync({Id}, {ExtraFlags})", id, extraFlags);

            await database.HashSetAsync(id, new HashEntry[] { new HashEntry("ExtraFlags", extraFlags) });

        }

        public async Task SetLockAsync(string id, uint cookie, DateTime time, CancellationToken cancellationToken)
        {
            logger.Verbose("SetLockAsync({Id}, {Cookie}, {Time})", id, cookie, time);

            await database.HashSetAsync(id, new HashEntry[] { new HashEntry("LockCookie", cookie), new HashEntry("LockTime", time.Ticks) });
        }

        public async Task SetTimeoutAsync(string id, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            logger.Verbose("SetTimeoutAsync({Id}, {Timeout})", id, timeout);

            var batch = database.CreateBatch();
            var t1 = database.HashSetAsync(id, new HashEntry[] { new HashEntry("Timeout", timeout?.Ticks) });
            var t2 = database.KeyExpireAsync(id, timeout + TimeSpan.FromMinutes(1) ?? TimeSpan.FromMinutes(20));
            batch.Execute();
            await Task.WhenAll(t1, t2);
        }

    }

}
