using System;

using Cogito.Autofac;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetStateService.EntityFrameworkCore.PostgreSQL
{

    [RegisterAs(typeof(PostgreSQLStateObjectDbContext))]
    [RegisterInstancePerLifetimeScope]
    public class PostgreSQLStateObjectDbContext : StateObjectDbContext
    {

        readonly IOptions<PostgreSQLStateObjectDbContextDataStoreOptions> npgsqlOptions;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="options"></param>
        /// <param name="npgsqlOptions"></param>
        public PostgreSQLStateObjectDbContext(ILoggerFactory loggerFactory, IOptions<StateObjectDbContextDataStoreOptions> options, IOptions<PostgreSQLStateObjectDbContextDataStoreOptions> npgsqlOptions) :
            base(loggerFactory, options)
        {
            this.npgsqlOptions = npgsqlOptions ?? throw new ArgumentNullException(nameof(options));
        }

        protected override void OnConfiguringDatabase(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(npgsqlOptions.Value.ConnectionString, b => b
                .CommandTimeout(npgsqlOptions.Value.CommandTimeout));
        }

    }

}
