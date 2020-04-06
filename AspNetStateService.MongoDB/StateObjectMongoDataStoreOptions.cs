using Cogito.Extensions.Options.ConfigurationExtensions.Autofac;

namespace AspNetStateService.MongoDB
{

    [RegisterOptions("AspNetStateService.MongoDB")]
    public class StateObjectMongoDataStoreOptions
    {

        /// <summary>
        /// Gets or sets the connection string to use for the MongoDB instance.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the database.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the name of the collection of state objects.
        /// </summary>
        public string CollectionName { get; set; }

    }

}
