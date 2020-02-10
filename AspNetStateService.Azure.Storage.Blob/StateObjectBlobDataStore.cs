using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Core;

using Autofac.Features.AttributeFilters;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Cogito.Autofac;

using Microsoft.Extensions.Options;

namespace AspNetStateService.Azure.Storage.Blob
{

    /// <summary>
    /// Implements a <see cref="IStateObjectDataStore"/> using Entity Framework Core.
    /// </summary>
    [RegisterAs(typeof(IStateObjectDataStore))]
    [RegisterSingleInstance]
    [RegisterWithAttributeFiltering]
    public class StateObjectBlobDataStore : IStateObjectDataStore
    {

        public const string TypeNameKey = "AspNetStateService.Azure.Storage.Blob";

        readonly BlobContainerClient client;
        readonly IStatePathProvider pather;
        readonly IOptions<StateObjectBlobDataStoreOptions> options;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pather"></param>
        /// <param name="options"></param>
        public StateObjectBlobDataStore([KeyFilter(TypeNameKey)] BlobContainerClient client, IStatePathProvider pather, IOptions<StateObjectBlobDataStoreOptions> options)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.pather = pather ?? throw new ArgumentNullException(nameof(pather));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Initializes the table store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task InitAsync(CancellationToken cancellationToken)
        {
            await client.CreateIfNotExistsAsync();
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

            var blb = client.GetBlobClient(pather.GetPath(id));
            if (await blb.ExistsAsync(cancellationToken) == false)
                return null;

            try
            {
                var rsl = await blb.DownloadAsync(cancellationToken);
                if (rsl.Value is BlobDownloadInfo ent)
                    return await JsonSerializer.DeserializeAsync<StateObjectEntity>(ent.Content, null, cancellationToken);
            }
            catch (RequestFailedException e) when (e.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                return null;
            }

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

            using var stm = new MemoryStream();
            using var wrt = new Utf8JsonWriter(stm);
            JsonSerializer.Serialize(wrt, entity);
            wrt.Dispose();
            stm.Position = 0;

            var blb = client.GetBlobClient(pather.GetPath(entity.Id));
            var rsl = await blb.UploadAsync(stm, true, cancellationToken);
            if (rsl.Value is BlobContentInfo ent)
                return;
        }

        public async Task<(byte[] data, uint? extraFlags, TimeSpan? timeout, DateTime? altered)> GetDataAsync(string id, CancellationToken cancellationToken)
        {
            var value = await GetStateObjectAsync(id, cancellationToken);
            return (value?.Data, value?.ExtraFlags, value?.Timeout, value?.Altered);
        }

        public async Task<(uint? cookie, DateTime? time)> GetLockAsync(string id, CancellationToken cancellationToken)
        {
            var value = await GetStateObjectAsync(id, cancellationToken);
            return (value?.LockCookie, value?.LockTime);
        }

        public async Task RemoveDataAsync(string id, CancellationToken cancellationToken)
        {
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
            var value = await GetStateObjectAsync(id, cancellationToken);
            if (value == null)
                value = new StateObjectEntity(id);

            value.Data = data;
            value.ExtraFlags = extraFlags;
            value.Timeout = timeout;
            value.Altered = DateTime.UtcNow;

            await SetStateObjectAsync(value, cancellationToken);
        }

        public async Task SetFlagAsync(string id, uint? extraFlags, CancellationToken cancellationToken)
        {
            var value = await GetStateObjectAsync(id, cancellationToken);
            if (value == null)
                value = new StateObjectEntity(id);

            value.ExtraFlags = extraFlags;

            await SetStateObjectAsync(value, cancellationToken);
        }

        public async Task SetLockAsync(string id, uint cookie, DateTime time, CancellationToken cancellationToken)
        {
            var value = await GetStateObjectAsync(id, cancellationToken);
            if (value == null)
                value = new StateObjectEntity(id);

            value.LockCookie = cookie;
            value.LockTime = time;

            await SetStateObjectAsync(value, cancellationToken);
        }

        public async Task SetTimeoutAsync(string id, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            var value = await GetStateObjectAsync(id, cancellationToken);
            if (value == null)
                value = new StateObjectEntity(id);

            value.Timeout = timeout;

            await SetStateObjectAsync(value, cancellationToken);
        }

    }

}
