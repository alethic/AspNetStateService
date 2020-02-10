using Cogito.Extensions.Options.ConfigurationExtensions.Autofac;

namespace AspNetStateService.Redis
{

    [RegisterOptions("AspNetStateService.Redis")]
    public class StateObjectRedisDataStoreOptions
    {

        /// <summary>
        /// Gets the StackExchange Redis configuration string.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// Gets the database ID to connect to.
        /// </summary>
        public int? DatabaseId { get; set; }

    }

}
