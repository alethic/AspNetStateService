using System;

using Cogito.Autofac;

using Microsoft.Extensions.Options;

using MongoDB.Driver;

namespace AspNetStateService.MongoDB
{

    /// <summary>
    /// Default provider of <see cref="MongoClient"/> instances.
    /// </summary>
    [RegisterAs(typeof(IMongoClientProvider))]
    public class DefaultMongoClientProvider : IMongoClientProvider
    {

        readonly IOptions<StateObjectMongoDataStoreOptions> options;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="options"></param>
        public DefaultMongoClientProvider(IOptions<StateObjectMongoDataStoreOptions> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IMongoClient CreateClient()
        {
            if (options.Value == null ||
                options.Value.ConnectionString == null)
                throw new InvalidOperationException("Missing configuration for MongoDB connection string.");

            return new MongoClient(options.Value.ConnectionString);
        }

    }

}
