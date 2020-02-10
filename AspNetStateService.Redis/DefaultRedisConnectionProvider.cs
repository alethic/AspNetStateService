using System;
using System.Threading.Tasks;

using Cogito.Autofac;

using Microsoft.Extensions.Options;
using Serilog;
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
        readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public DefaultRedisConnectionProvider(IOptions<StateObjectRedisDataStoreOptions> options, ILogger logger)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ConnectionMultiplexer> GetConnectionAsync()
        {
            var configuration = options.Value.Configuration ?? "localhost";
            logger.Verbose("Opening Redis connection with {Configuration}.", configuration);

            var connection = await ConnectionMultiplexer.ConnectAsync(configuration);
            connection.ConnectionFailed += (s, a) => logger.Error(a.Exception, "Redis ConnectionFailed event.");
            connection.ConnectionRestored += (s, a) => logger.Information(a.Exception, "Redis ConnectionRestored event.");
            connection.ErrorMessage += (s, a) => logger.Error(a.Message);
            connection.InternalError += (s, a) => logger.Error(a.Exception, "Redis InternalError event.");
            return connection;
        }

    }

}
