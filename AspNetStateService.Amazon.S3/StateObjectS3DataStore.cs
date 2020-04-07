using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Amazon.S3;
using Amazon.S3.Model;

using AspNetStateService.Core;

using Autofac.Features.AttributeFilters;

using Cogito.Autofac;
using Cogito.IO;
using Cogito.Threading;

using Microsoft.Extensions.Options;

using Serilog;

namespace AspNetStateService.Amazon.S3
{

    /// <summary>
    /// Implements a <see cref="IStateObjectDataStore"/> using Amazon S3.
    /// </summary>
    [RegisterNamed(typeof(IStateObjectDataStore), "Amazon.S3")]
    [RegisterSingleInstance]
    [RegisterWithAttributeFiltering]
    public class StateObjectS3DataStore : IStateObjectDataStore
    {

        public const string TypeNameKey = "AspNetStateService.Amazon S3";

        readonly IAmazonS3 client;
        readonly IStateKeyProvider keyer;
        readonly IOptions<StateObjectS3DataStoreOptions> options;
        readonly ILogger logger;
        readonly AsyncLock sync = new AsyncLock();

        bool init = true;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="keyer"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public StateObjectS3DataStore([KeyFilter(TypeNameKey)] IAmazonS3 client, IStateKeyProvider keyer, IOptions<StateObjectS3DataStoreOptions> options, ILogger logger)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.keyer = keyer ?? throw new ArgumentNullException(nameof(keyer));
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
            await client.EnsureBucketExistsAsync(options.Value.BucketName);
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
        /// <param name="collection"></param>
        /// <returns></returns>
        StateObjectMetadata GetMetadata(MetadataCollection collection)
        {
            var metadata = new StateObjectMetadata();
            metadata.ExtraFlags = collection["ExtraFlags"] is string fd && uint.TryParse(fd, out var fv) ? (uint?)fv : null;
            metadata.Timeout = collection["Timeout"] is string td && TimeSpan.TryParse(td, out var tv) ? (TimeSpan?)tv : null;
            metadata.Altered = collection["Altered"] is string ad && DateTime.TryParse(ad, out var av) ? (DateTime?)av : null;
            metadata.LockCookie = collection["LockCookie"] is string cd && uint.TryParse(cd, out var cv) ? (uint?)cv : null;
            metadata.LockTime = collection["LockTime"] is string ld && DateTime.TryParse(ld, out var lv) ? (DateTime?)lv : null;
            return metadata;
        }

        /// <summary>
        /// Sets the metadata on the dictionary.
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="collection"></param>
        void SetMetadata(StateObjectMetadata metadata, MetadataCollection collection)
        {
            collection["ExtraFlags"] = metadata.ExtraFlags?.ToString() ?? "";
            collection["Timeout"] = metadata.Timeout?.ToString() ?? "";
            collection["Altered"] = metadata.Altered?.ToString() ?? "";
            collection["LockCookie"] = metadata.LockCookie?.ToString() ?? "";
            collection["LockTime"] = metadata.LockTime?.ToString() ?? "";
        }

        /// <summary>
        /// Gets an objects existing metadata.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<StateObjectMetadata> GetObjectMetadataAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                var res = await client.GetObjectMetadataAsync(options.Value.BucketName, keyer.GetKey(id), cancellationToken);
                if (res != null)
                    return GetMetadata(res.Metadata);
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (Exception)
            {
                throw;
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Gets an objects existing metadata.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<StateObjectData> GetObjectAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                using var res = await client.GetObjectAsync(options.Value.BucketName, keyer.GetKey(id), cancellationToken);
                if (res != null)
                    return new StateObjectData(GetMetadata(res.Metadata), await res.ResponseStream.ReadAllBytesAsync());
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (Exception)
            {
                throw;
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Deletes an object.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task DeleteObjectAsync(string id, CancellationToken cancellationToken)
        {
            try
            {
                var res = await client.DeleteObjectAsync(options.Value.BucketName, keyer.GetKey(id), cancellationToken);
                if (res != null)
                    return;
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }
            catch (Exception)
            {
                throw;
            }

            throw new InvalidOperationException();
        }

        public async Task<(byte[] data, uint? extraFlags, TimeSpan? timeout, DateTime? altered)> GetDataAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("GetDataAsync({Id})", id);

            var dat = await GetObjectAsync(id, cancellationToken);
            if (dat == null)
                return default;

            return (dat.Buffer, dat.Metadata.ExtraFlags, dat.Metadata.Timeout, dat.Metadata.Altered);
        }

        public async Task<(uint? cookie, DateTime? time)> GetLockAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("GetLockAsync({Id})", id);

            var met = await GetObjectMetadataAsync(id, cancellationToken);
            if (met == null)
                return default;

            return (met.LockCookie, met.LockTime);
        }

        public async Task RemoveDataAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("RemoveDataAsync({Id})", id);

            await DeleteObjectAsync(id, cancellationToken);
        }

        public async Task RemoveLockAsync(string id, CancellationToken cancellationToken)
        {
            logger.Verbose("RemoveLockAsync({Id})", id);

            // get existing metadata
            var met = await GetObjectMetadataAsync(id, cancellationToken);
            if (met == null)
                return;

            // prepare copy request
            var req = new CopyObjectRequest()
            {
                SourceBucket = options.Value.BucketName,
                SourceKey = keyer.GetKey(id),
                DestinationBucket = options.Value.BucketName,
                DestinationKey = keyer.GetKey(id),
                MetadataDirective = S3MetadataDirective.COPY,
            };

            // build new metadata
            met.LockCookie = null;
            met.LockTime = null;
            SetMetadata(met, req.Metadata);

            // create new copy of object
            await client.CopyObjectAsync(req, cancellationToken);
        }

        public async Task SetDataAsync(string id, byte[] data, uint? extraFlags, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            logger.Verbose("SetDataAsync({Id}, {ExtraFlags}, {Timeout})", id, extraFlags, timeout);

            // get existing metadata
            var met = await GetObjectMetadataAsync(id, cancellationToken);
            if (met == null)
                met = new StateObjectMetadata();

            // prepare put request
            var req = new PutObjectRequest()
            {
                BucketName = options.Value.BucketName,
                Key = keyer.GetKey(id),
                InputStream = new MemoryStream(data),
            };

            // build new metadata
            met.ExtraFlags = extraFlags;
            SetMetadata(met, req.Metadata);

            // put new object
            await client.PutObjectAsync(req, cancellationToken);
        }

        public async Task SetFlagAsync(string id, uint? extraFlags, CancellationToken cancellationToken)
        {
            logger.Verbose("SetFlagAsync({Id}, {ExtraFlags})", id, extraFlags);

            // get existing metadata
            var met = await GetObjectMetadataAsync(id, cancellationToken);
            if (met == null)
            {
                // initialize with empty data
                await SetDataAsync(id, new byte[0], extraFlags, null, cancellationToken);
                return;
            }

            // prepare copy request
            var req = new CopyObjectRequest()
            {
                SourceBucket = options.Value.BucketName,
                SourceKey = keyer.GetKey(id),
                DestinationBucket = options.Value.BucketName,
                DestinationKey = keyer.GetKey(id),
                MetadataDirective = S3MetadataDirective.REPLACE,
            };

            // build new metadata
            met.ExtraFlags = extraFlags;
            SetMetadata(met, req.Metadata);

            // create new copy of object
            await client.CopyObjectAsync(req, cancellationToken);
        }

        public async Task SetLockAsync(string id, uint cookie, DateTime time, CancellationToken cancellationToken)
        {
            logger.Verbose("SetLockAsync({Id}, {Cookie}, {Time})", id, cookie, time);

            // get existing metadata
            var met = await GetObjectMetadataAsync(id, cancellationToken);
            if (met == null)
            {
                // initialize with empty data
                await SetDataAsync(id, new byte[0], null, null, cancellationToken);
                return;
            }

            // prepare copy request
            var req = new CopyObjectRequest()
            {
                SourceBucket = options.Value.BucketName,
                SourceKey = keyer.GetKey(id),
                DestinationBucket = options.Value.BucketName,
                DestinationKey = keyer.GetKey(id),
                MetadataDirective = S3MetadataDirective.REPLACE,
            };

            // build new metadata
            met.LockCookie = cookie;
            met.LockTime = time;
            SetMetadata(met, req.Metadata);

            // create new copy of object
            await client.CopyObjectAsync(req, cancellationToken);
        }

        public async Task SetTimeoutAsync(string id, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            logger.Verbose("SetTimeoutAsync({Id}, {Timeout})", id, timeout);

            // get existing metadata
            var met = await GetObjectMetadataAsync(id, cancellationToken);
            if (met == null)
            {
                // initialize with empty data
                await SetDataAsync(id, new byte[0], null, null, cancellationToken);
                return;
            }

            // prepare copy request
            var req = new CopyObjectRequest()
            {
                SourceBucket = options.Value.BucketName,
                SourceKey = keyer.GetKey(id),
                DestinationBucket = options.Value.BucketName,
                DestinationKey = keyer.GetKey(id),
                MetadataDirective = S3MetadataDirective.REPLACE,
            };

            // build new metadata
            met.Timeout = timeout;
            SetMetadata(met, req.Metadata);

            // create new copy of object
            await client.CopyObjectAsync(req, cancellationToken);
        }

    }

}
