using System;

using Azure.Storage.Blobs;

using Cogito.Autofac;

using Microsoft.Extensions.Options;

namespace AspNetStateService.Azure.Storage.Blob
{

    /// <summary>
    /// Default provider of <see cref="BlobContainerClient"/> instances.
    /// </summary>
    [RegisterAs(typeof(IBlobContainerClientProvider))]
    [RegisterSingleInstance]
    public class DefaultBlobContainerClientProvider : IBlobContainerClientProvider
    {

        readonly IOptions<StateObjectBlobDataStoreOptions> options;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="account"></param>
        public DefaultBlobContainerClientProvider(IOptions<StateObjectBlobDataStoreOptions> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public BlobContainerClient GetBlobClient()
        {
            if (options.Value == null ||
                options.Value.ConnectionString == null)
                throw new InvalidOperationException("Missing configuration for Azure Table Storage connection string.");

            return new BlobContainerClient(options.Value.ConnectionString, options.Value.ContainerName ?? "state");
        }

    }

}
