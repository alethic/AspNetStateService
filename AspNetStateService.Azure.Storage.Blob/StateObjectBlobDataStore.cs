using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Core;

using Autofac.Features.AttributeFilters;

using Azure.Storage.Blobs;

using Cogito.Autofac;
using Cogito.Threading;

using Microsoft.Extensions.Options;

using Serilog;

namespace AspNetStateService.Azure.Storage.Blob
{

    /// <summary>
    /// Implements a <see cref="IStateObjectDataStore"/> using Azure Storage Blobs.
    /// </summary>
    [RegisterNamed(typeof(IStateObjectDataStore), "Azure.Storage.Blob")]
    [RegisterSingleInstance]
    [RegisterWithAttributeFiltering]
    public class StateObjectBlobDataStore : IStateObjectDataStore
    {

        public const string TypeNameKey = "AspNetStateService.Azure.Storage.Blob";

        readonly BlobContainerClient client;
        readonly IStatePathProvider pather;
        readonly IOptions<StateObjectBlobDataStoreOptions> options;
        readonly ILogger logger;
        readonly AsyncLock sync = new AsyncLock();

        bool init = true;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pather"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public StateObjectBlobDataStore([KeyFilter(TypeNameKey)] BlobContainerClient client, IStatePathProvider pather, IOptions<StateObjectBlobDataStoreOptions> options, ILogger logger)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.pather = pather ?? throw new ArgumentNullException(nameof(pather));
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
            await client.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
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
        /// Attempts to parse the given metadata values.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        StateObjectMetadata GetMetadata(IDictionary<string, string> properties)
        {
            var metadata = new StateObjectMetadata();
            metadata.ExtraFlags = properties.TryGetValue("ExtraFlags", out var fd) && uint.TryParse(fd, out var fv) ? (uint?)fv : null;
            metadata.Timeout = properties.TryGetValue("Timeout", out var td) && TimeSpan.TryParse(td, out var tv) ? (TimeSpan?)tv : null;
            metadata.Altered = properties.TryGetValue("Altered", out var ad) && DateTime.TryParse(ad, out var av) ? (DateTime?)av : null;
            metadata.LockCookie = properties.TryGetValue("LockCookie", out var cd) && uint.TryParse(cd, out var cv) ? (uint?)cv : null;
            metadata.LockTime = properties.TryGetValue("LockTime", out var ld) && DateTime.TryParse(ld, out var lv) ? (DateTime?)lv : null;
            return metadata;
        }

        /// <summary>
        /// Sets the metadata on the dictionary.
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="properties"></param>
        void SetMetadata(StateObjectMetadata metadata, IDictionary<string, string> properties)
        {
            properties["ExtraFlags"] = metadata.ExtraFlags?.ToString() ?? "";
            properties["Timeout"] = metadata.Timeout?.ToString() ?? "";
            properties["Altered"] = metadata.Altered?.ToString() ?? "";
            properties["LockCookie"] = metadata.LockCookie?.ToString() ?? "";
            properties["LockTime"] = metadata.LockTime?.ToString() ?? "";
        }

        public async Task<(byte[] data, uint? extraFlags, TimeSpan? timeout, DateTime? altered)> GetDataAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("GetDataAsync({Id})", id);

            var blb = client.GetBlobClient(pather.GetPath(id));
            if (await blb.ExistsAsync(cancellationToken) == false)
                return default;

            var stm = new MemoryStream();
            var prp = await blb.GetPropertiesAsync(null, cancellationToken);
            if (prp.Value.ContentLength > 0)
                await blb.DownloadToAsync(stm, cancellationToken);

            var met = GetMetadata(prp.Value.Metadata);
            return (stm.ToArray(), met.ExtraFlags, met.Timeout, met.Altered);
        }

        public async Task<(uint? cookie, DateTime? time)> GetLockAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("GetLockAsync({Id})", id);

            var blb = client.GetBlobClient(pather.GetPath(id));
            if (await blb.ExistsAsync(cancellationToken) == false)
                return default;

            var prp = await blb.GetPropertiesAsync(null, cancellationToken);
            var met = GetMetadata(prp.Value.Metadata);
            return (met.LockCookie, met.LockTime);
        }

        public async Task RemoveDataAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("RemoveDataAsync({Id})", id);

            var blb = client.GetBlobClient(pather.GetPath(id));
            if (await blb.ExistsAsync(cancellationToken) == false)
                return;

            var prp = await blb.GetPropertiesAsync(null, cancellationToken);
            if (prp.Value.ContentLength > 0)
                await blb.UploadAsync(new MemoryStream(), true, cancellationToken);

            var met = GetMetadata(prp.Value.Metadata);
            met.ExtraFlags = null;
            met.Timeout = null;
            met.Altered = null;

            var dir = new Dictionary<string, string>();
            SetMetadata(met, dir);
            await blb.SetMetadataAsync(dir, null, cancellationToken);
        }

        public async Task RemoveLockAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("RemoveLockAsync({Id})", id);

            var blb = client.GetBlobClient(pather.GetPath(id));
            if (await blb.ExistsAsync(cancellationToken) == false)
                return;

            var prp = await blb.GetPropertiesAsync(null, cancellationToken);
            var met = GetMetadata(prp.Value.Metadata);
            met.LockCookie = null;
            met.LockTime = null;

            var dir = new Dictionary<string, string>();
            SetMetadata(met, dir);
            await blb.SetMetadataAsync(dir, null, cancellationToken);
        }

        public async Task SetDataAsync(string id, byte[] data, uint? extraFlags, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            logger.Verbose("SetDataAsync({Id}, {ExtraFlags}, {Timeout})", id, extraFlags, timeout);

            var blb = client.GetBlobClient(pather.GetPath(id));
            await blb.UploadAsync(new MemoryStream(data), true, cancellationToken);

            var prp = await blb.GetPropertiesAsync(null, cancellationToken);
            var met = GetMetadata(prp.Value.Metadata);
            met.ExtraFlags = extraFlags;
            met.Timeout = timeout;
            met.Altered = DateTime.UtcNow;

            var dir = new Dictionary<string, string>();
            SetMetadata(met, dir);
            await blb.SetMetadataAsync(dir, null, cancellationToken);
        }

        public async Task SetFlagAsync(string id, uint? extraFlags, CancellationToken cancellationToken)
        {
            logger.Verbose("SetFlagAsync({Id}, {ExtraFlags})", id, extraFlags);

            var blb = client.GetBlobClient(pather.GetPath(id));
            if (await blb.ExistsAsync(cancellationToken) == false)
                await blb.UploadAsync(new MemoryStream(), true, cancellationToken);

            var prp = await blb.GetPropertiesAsync(null, cancellationToken);
            var met = GetMetadata(prp.Value.Metadata);
            met.ExtraFlags = extraFlags;

            var dir = new Dictionary<string, string>();
            SetMetadata(met, dir);
            await blb.SetMetadataAsync(dir, null, cancellationToken);
        }

        public async Task SetLockAsync(string id, uint cookie, DateTime time, CancellationToken cancellationToken)
        {
            logger.Verbose("SetLockAsync({Id}, {Cookie}, {Time})", id, cookie, time);

            var blb = client.GetBlobClient(pather.GetPath(id));
            if (await blb.ExistsAsync(cancellationToken) == false)
                await blb.UploadAsync(new MemoryStream(), true, cancellationToken);

            var prp = await blb.GetPropertiesAsync(null, cancellationToken);
            var met = GetMetadata(prp.Value.Metadata);
            met.LockCookie = cookie;
            met.LockTime = time;
            var dir = new Dictionary<string, string>();
            SetMetadata(met, dir);
            await blb.SetMetadataAsync(dir, null, cancellationToken);
        }

        public async Task SetTimeoutAsync(string id, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            logger.Verbose("SetTimeoutAsync({Id}, {Timeout})", id, timeout);

            var blb = client.GetBlobClient(pather.GetPath(id));
            if (await blb.ExistsAsync(cancellationToken) == false)
                await blb.UploadAsync(new MemoryStream(), true, cancellationToken);

            var prp = await blb.GetPropertiesAsync(null, cancellationToken);
            var met = GetMetadata(prp.Value.Metadata);
            met.Timeout = timeout;

            var dir = new Dictionary<string, string>();
            SetMetadata(met, dir);
            await blb.SetMetadataAsync(dir, null, cancellationToken);
        }

    }

}
