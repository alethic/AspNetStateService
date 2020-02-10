using Cogito.Extensions.Options.ConfigurationExtensions.Autofac;

namespace AspNetStateService.Azure.Storage.Blob
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
