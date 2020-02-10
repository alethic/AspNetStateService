using Cogito.Extensions.Options.ConfigurationExtensions.Autofac;

namespace AspNetStateService.Redis
{

    [RegisterOptions("AspNetStateService.Redis")]
    public class StateObjectRedisDataStoreOptions
    {

        public string Configuration { get; set; }

    }

}
