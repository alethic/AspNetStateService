using System;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Core;

using Autofac.Features.AttributeFilters;

using Cogito.Autofac;
using Cogito.Threading;

using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;

using Serilog;

namespace AspNetStateService.Azure.Storage.Table
{

    /// <summary>
    /// Implements a <see cref="IStateObjectDataStore"/> using Azure Storage Tables.
    /// </summary>
    [RegisterNamed(typeof(IStateObjectDataStore), "Azure.Storage.Table")]
    [RegisterSingleInstance]
    [RegisterWithAttributeFiltering]
    public class StateObjectTableDataStore : IStateObjectDataStore
    {

        public const string TypeNameKey = "AspNetStateService.Azure.Storage.Table";

        readonly CloudTableClient client;
        readonly IStateKeyProvider partitioner;
        readonly IOptions<StateObjectTableDataStoreOptions> options;
        readonly ILogger logger;
        readonly AsyncLock sync = new AsyncLock();

        bool init = true;
        CloudTable table;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="partitioner"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public StateObjectTableDataStore([KeyFilter(TypeNameKey)] CloudTableClient client, IStateKeyProvider partitioner, IOptions<StateObjectTableDataStoreOptions> options, ILogger logger)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.partitioner = partitioner ?? throw new ArgumentNullException(nameof(partitioner));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Does the actual work of initialization.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task InitInternalAsync(CancellationToken cancellationToken)
        {
            logger.Verbose("InitInternalAsync()");

            var n = options.Value.TableName ?? "state";
            logger.Verbose("Creating storage table {TableName}.", n);

            table = client.GetTableReference(n);
            await table.CreateIfNotExistsAsync(cancellationToken);

            init = false;
        }

        /// <summary>
        /// Initializes the table store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task InitAsync(CancellationToken cancellationToken)
        {
            logger.Verbose("InitAsync()");

            if (init)
                using (await sync.LockAsync())
                    if (init)
                        await InitInternalAsync(cancellationToken);
        }

        /// <summary>
        /// Attempts to retrieve the state object from the table.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<StateObjectEntity> GetStateObjectAsync(string id, CancellationToken cancellationToken)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            var get = TableOperation.Retrieve<StateObjectEntity>(partitioner.GetPartitionKey(id), partitioner.GetRowKey(id));
            var rsl = await table.ExecuteAsync(get, cancellationToken);
            if (rsl.Result is StateObjectEntity ent)
                return ent;

            return null;
        }

        /// <summary>
        /// Attempts to update the state object in the table.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task SetStateObjectAsync(StateObjectEntity entity, CancellationToken cancellationToken)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            var set = TableOperation.InsertOrReplace(entity);
            await table.ExecuteAsync(set, cancellationToken);
        }

        TimeSpan? FromLong(long? value)
        {
            return value != null ? (TimeSpan?)TimeSpan.FromTicks((long)value) : null;
        }

        public async Task<(byte[] data, uint? extraFlags, TimeSpan? timeout, DateTime? altered)> GetDataAsync(string id, CancellationToken cancellationToken)
        {
            if (table == null)
                throw new InvalidOperationException();

            var value = await GetStateObjectAsync(id, cancellationToken);
            return (value?.Data, (uint?)value?.ExtraFlags, FromLong(value?.Timeout), value?.Altered);
        }

        public async Task<(uint? cookie, DateTime? time)> GetLockAsync(string id, CancellationToken cancellationToken)
        {
            if (table == null)
                throw new InvalidOperationException();

            var value = await GetStateObjectAsync(id, cancellationToken);
            return ((uint?)value?.LockCookie, value?.LockTime);
        }

        public async Task RemoveDataAsync(string id, CancellationToken cancellationToken)
        {
            if (table == null)
                throw new InvalidOperationException();

            var value = await GetStateObjectAsync(id, cancellationToken);
            if (value != null)
            {
                value.Data = null;
                value.ExtraFlags = null;
                value.Timeout = null;
                value.Altered = null;
            }

            await SetStateObjectAsync(value, cancellationToken);
        }

        public async Task RemoveLockAsync(string id, CancellationToken cancellationToken)
        {
            if (table == null)
                throw new InvalidOperationException();

            var value = await GetStateObjectAsync(id, cancellationToken);
            if (value != null)
            {
                value.LockCookie = null;
                value.LockTime = null;
            }

            await SetStateObjectAsync(value, cancellationToken);
        }

        public async Task SetDataAsync(string id, byte[] data, uint? extraFlags, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            if (table == null)
                throw new InvalidOperationException();

            var value = await GetStateObjectAsync(id, cancellationToken);
            if (value == null)
                value = new StateObjectEntity(partitioner.GetPartitionKey(id), partitioner.GetRowKey(id), id);

            value.Data = data;
            value.ExtraFlags = (int?)extraFlags;
            value.Timeout = timeout?.Ticks;
            value.Altered = DateTime.UtcNow;

            await SetStateObjectAsync(value, cancellationToken);
        }

        public async Task SetFlagAsync(string id, uint? extraFlags, CancellationToken cancellationToken)
        {
            if (table == null)
                throw new InvalidOperationException();

            var value = await GetStateObjectAsync(id, cancellationToken);
            if (value == null)
                value = new StateObjectEntity(partitioner.GetPartitionKey(id), partitioner.GetRowKey(id), id);

            value.ExtraFlags = (int?)extraFlags;

            await SetStateObjectAsync(value, cancellationToken);
        }

        public async Task SetLockAsync(string id, uint cookie, DateTime time, CancellationToken cancellationToken)
        {
            if (table == null)
                throw new InvalidOperationException();

            var value = await GetStateObjectAsync(id, cancellationToken);
            if (value == null)
                value = new StateObjectEntity(partitioner.GetPartitionKey(id), partitioner.GetRowKey(id), id);

            value.LockCookie = (int)cookie;
            value.LockTime = time;

            await SetStateObjectAsync(value, cancellationToken);
        }

        public async Task SetTimeoutAsync(string id, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            if (table == null)
                throw new InvalidOperationException();

            var value = await GetStateObjectAsync(id, cancellationToken);
            if (value == null)
                value = new StateObjectEntity(partitioner.GetPartitionKey(id), partitioner.GetRowKey(id), id);

            value.Timeout = timeout?.Ticks;

            await SetStateObjectAsync(value, cancellationToken);
        }

    }

}
