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

    }

}
