using System;

using Cogito.Autofac;

using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;

namespace AspNetStateService.Azure.Storage.Table
{

    /// <summary>
    /// Default provider of <see cref="CloudStorageAccount"/> instances.
    /// </summary>
    [RegisterAs(typeof(ICloudStorageAccountProvider))]
    [RegisterWithAttributeFiltering]
    public class DefaultCloudStorageAccountProvider : ICloudStorageAccountProvider
    {

        readonly IOptions<StateObjectTableDataStoreOptions> options;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="options"></param>
        public DefaultCloudStorageAccountProvider(IOptions<StateObjectTableDataStoreOptions> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public CloudStorageAccount GetStorageAccount()
        {
            if (options.Value == null ||
                options.Value.ConnectionString == null)
                throw new InvalidOperationException("Missing configuration for Azure Table Storage connection string.");

            return CloudStorageAccount.Parse(options.Value.ConnectionString);
        }
    }

}
