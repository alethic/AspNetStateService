using System;
using System.Threading.Tasks;

using Cogito.Autofac;

using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace AspNetStateService.Redis
{

    /// <summary>
    /// Default provider of <see cref="ConnectionMultiplexer"/> instances.
    /// </summary>
    [RegisterAs(typeof(IRedisConnectionProvider))]
    public class DefaultRedisConnectionProvider : IRedisConnectionProvider
    {

        readonly IOptions<StateObjectRedisDataStoreOptions> options;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="options"></param>
        public DefaultRedisConnectionProvider(IOptions<StateObjectRedisDataStoreOptions> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<ConnectionMultiplexer> GetConnectionAsync()
        {
            return await ConnectionMultiplexer.ConnectAsync(options.Value.Configuration ?? "localhost");
        }

    }

}
