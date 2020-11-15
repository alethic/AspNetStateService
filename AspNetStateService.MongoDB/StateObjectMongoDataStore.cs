using System;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Core;

using Cogito.Autofac;
using Cogito.Threading;

using Microsoft.Extensions.Options;

using MongoDB.Bson;
using MongoDB.Driver;

using Serilog;

namespace AspNetStateService.MongoDB
{

    /// <summary>
    /// Implements a <see cref="IStateObjectDataStore"/> using Redis.
    /// </summary>
    [RegisterNamed(typeof(IStateObjectDataStore), "MongoDB")]
    [RegisterSingleInstance]
    [RegisterWithAttributeFiltering]
    public class StateObjectMongoDataStore : IStateObjectDataStore
    {

        const string ID_KEY = "Id";
        const string DATA_KEY = "Data";
        const string EXTRA_FLAGS_KEY = "ExtraFlags";
        const string TIMEOUT_KEY = "Timeout";
        const string ALTERED_KEY = "Altered";
        const string LOCK_COOKIE_KEY = "LockCookie";
        const string LOCK_TIME_KEY = "LockTime";
        const string EXPIRE_AT_KEY = "ExpireAt";

        readonly IMongoClientProvider clients;
        readonly IOptions<StateObjectMongoDataStoreOptions> options;
        readonly ILogger logger;
        readonly AsyncLock sync = new AsyncLock();

        IMongoClient client;
        IMongoDatabase database;
        IMongoCollection<BsonDocument> collection;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="clients"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public StateObjectMongoDataStore(IMongoClientProvider clients, IOptions<StateObjectMongoDataStoreOptions> options, ILogger logger)
        {
            this.clients = clients ?? throw new ArgumentNullException(nameof(clients));
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

            // initialize connection objects
            client = clients.CreateClient();
            database = client.GetDatabase(options.Value.DatabaseName ?? "AspNetState");
            collection = database.GetCollection<BsonDocument>(options.Value.CollectionName ?? "StateObject");

            // create an index on the ExpireAt field
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<BsonDocument>(new BsonDocument(EXPIRE_AT_KEY, 1), new CreateIndexOptions() { ExpireAfter = new TimeSpan(0, 20, 0) }),
                new CreateOneIndexOptions(),
                cancellationToken);
        }

        /// <summary>
        /// Stops the store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Verbose("StopAsync()");

            if (collection != null)
            {
                if (collection is IDisposable d)
                    d.Dispose();

                collection = null;
            }

            if (database != null)
            {
                if (database is IDisposable d)
                    d.Dispose();

                database = null;
            }

            if (client != null)
            {
                if (client is IDisposable d)
                    d.Dispose();

                client = null;
            }

            return Task.CompletedTask;
        }

        public async Task<(byte[] data, uint? extraFlags, TimeSpan? timeout, DateTime? altered)> GetDataAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("GetDataAsync({Id})", id);

            var hs = await collection
                .Find(Builders<BsonDocument>.Filter.Eq(ID_KEY, id))
                .Project(Builders<BsonDocument>.Projection.Include(DATA_KEY).Include(EXTRA_FLAGS_KEY).Include(TIMEOUT_KEY).Include(ALTERED_KEY))
                .FirstOrDefaultAsync(cancellationToken);

            return (hs?.GetValue(DATA_KEY).AsByteArray, (uint?)hs?.GetValue(EXTRA_FLAGS_KEY).AsNullableInt32, hs?.GetValue(TIMEOUT_KEY).AsNullableInt32 is int timeout ? (TimeSpan?)TimeSpan.FromSeconds(timeout) : null, hs?.GetValue(ALTERED_KEY)?.ToNullableUniversalTime());
        }

        public async Task<(uint? cookie, DateTime? time)> GetLockAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("GetLockAsync({Id})", id);

            var hs = await collection
                .Find(Builders<BsonDocument>.Filter.Eq(ID_KEY, id))
                .Project(Builders<BsonDocument>.Projection.Include(LOCK_COOKIE_KEY).Include(LOCK_TIME_KEY))
                .FirstOrDefaultAsync(cancellationToken);

            return ((uint?)hs?.GetValue(LOCK_COOKIE_KEY)?.AsNullableInt32, hs?.GetValue(LOCK_TIME_KEY)?.ToNullableUniversalTime());
        }

        public async Task RemoveDataAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("RemoveDataAsync({Id})", id);

            await collection.UpdateOneAsync(
                Builders<BsonDocument>.Filter.Eq(ID_KEY, id),
                Builders<BsonDocument>.Update
                    .Unset(DATA_KEY)
                    .Unset(EXTRA_FLAGS_KEY)
                    .Unset(TIMEOUT_KEY)
                    .Unset(ALTERED_KEY)
                    .Unset(EXPIRE_AT_KEY),
                cancellationToken: cancellationToken);
        }

        public async Task RemoveLockAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("RemoveLockAsync({Id})", id);

            await collection.UpdateOneAsync(
                Builders<BsonDocument>.Filter.Eq(ID_KEY, id),
                Builders<BsonDocument>.Update
                    .Unset(LOCK_COOKIE_KEY)
                    .Unset(LOCK_TIME_KEY),
                cancellationToken: cancellationToken);
        }

        public async Task SetDataAsync(string id, byte[] data, uint? extraFlags, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            logger.Verbose("SetDataAsync({Id}, {ExtraFlags}, {Timeout})", id, extraFlags, timeout);

            await collection.UpdateOneAsync(
                Builders<BsonDocument>.Filter.Eq(ID_KEY, id),
                Builders<BsonDocument>.Update
                    .Set(DATA_KEY, data)
                    .Set(EXTRA_FLAGS_KEY, extraFlags)
                    .Set(TIMEOUT_KEY, timeout?.TotalSeconds)
                    .Set(ALTERED_KEY, DateTime.UtcNow)
                    .Set(EXPIRE_AT_KEY, timeout != null ? (DateTime?)DateTime.UtcNow.Add(timeout.Value) : null),
                cancellationToken: cancellationToken);
        }

        public async Task SetFlagAsync(string id, uint? extraFlags, CancellationToken cancellationToken)
        {
            logger.Verbose("SetFlagAsync({Id}, {ExtraFlags})", id, extraFlags);

            await collection.UpdateOneAsync(
                Builders<BsonDocument>.Filter.Eq(ID_KEY, id),
                Builders<BsonDocument>.Update
                    .Set(EXTRA_FLAGS_KEY, extraFlags),
                cancellationToken: cancellationToken);
        }

        public async Task SetLockAsync(string id, uint cookie, DateTime time, CancellationToken cancellationToken)
        {
            logger.Verbose("SetLockAsync({Id}, {Cookie}, {Time})", id, cookie, time);

            await collection.UpdateOneAsync(
                Builders<BsonDocument>.Filter.Eq(ID_KEY, id),
                Builders<BsonDocument>.Update
                    .Set(LOCK_COOKIE_KEY, cookie)
                    .Set(LOCK_TIME_KEY, time),
                cancellationToken: cancellationToken);
        }

        public async Task SetTimeoutAsync(string id, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            logger.Verbose("SetTimeoutAsync({Id}, {Timeout})", id, timeout);

            await collection.UpdateOneAsync(
                Builders<BsonDocument>.Filter.Eq(ID_KEY, id),
                Builders<BsonDocument>.Update
                    .Set(TIMEOUT_KEY, timeout)
                    .Set(EXPIRE_AT_KEY, timeout != null ? (DateTime?)DateTime.UtcNow.Add(timeout.Value) : null),
                cancellationToken: cancellationToken);
        }

    }

}
