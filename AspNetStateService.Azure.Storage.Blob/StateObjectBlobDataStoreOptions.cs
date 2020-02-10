using Cogito.Extensions.Options.ConfigurationExtensions.Autofac;

namespace AspNetStateService.Azure.Storage.Blob
{

    [RegisterOptions("AspNetStateService.Azure.Storage.Blob")]
    public class StateObjectBlobDataStoreOptions
    {

        /// <summary>
        /// Gets or sets the connection string to use for the state storage account.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the container to use for state storage.
        /// </summary>
        public string ContainerName { get; set; }

    }

}
