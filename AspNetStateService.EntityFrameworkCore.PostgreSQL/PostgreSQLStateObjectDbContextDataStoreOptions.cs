using Cogito.Extensions.Options.ConfigurationExtensions.Autofac;

namespace AspNetStateService.EntityFrameworkCore.PostgreSQL
{

    [RegisterOptions("AspNetStateService.EntityFrameworkCore.PostgreSQL")]
    public class PostgreSQLStateObjectDbContextDataStoreOptions
    {

        /// <summary>
        /// Gets the connection string for the SQL Server database.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets the command timeout for SQL commands.
        /// </summary>
        public int? CommandTimeout { get; set; }

    }

}
