using Cogito.Extensions.Options.ConfigurationExtensions.Autofac;

namespace AspNetStateService.EntityFrameworkCore
{

    [RegisterOptions("AspNetStateService.EntityFrameworkCore")]
    public class StateObjectDbContextDataStoreOptions
    {

        /// <summary>
        /// Gets or sets the name of the Entity Framework Provider.
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Gets whether logging of sensitive data is enabled.
        /// </summary>
        public bool? EnableSensitiveDataLogging { get; set; }

    }

}
