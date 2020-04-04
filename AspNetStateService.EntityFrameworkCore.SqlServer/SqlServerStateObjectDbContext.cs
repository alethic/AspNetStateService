using System;

using Cogito.Autofac;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetStateService.EntityFrameworkCore.SqlServer
{

    [RegisterAs(typeof(SqlServerStateObjectDbContext))]
    [RegisterInstancePerLifetimeScope]
    public class SqlServerStateObjectDbContext : StateObjectDbContext
    {

        readonly IOptions<SqlServerStateObjectDbContextDataStoreOptions> sqlServerOptions;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="options"></param>
        /// <param name="sqlServerOtions"></param>
        public SqlServerStateObjectDbContext(ILoggerFactory loggerFactory, IOptions<StateObjectDbContextDataStoreOptions> options, IOptions<SqlServerStateObjectDbContextDataStoreOptions> sqlServerOtions) :
            base(loggerFactory, options)
        {
            this.sqlServerOptions = sqlServerOtions ?? throw new ArgumentNullException(nameof(options));
        }

        protected override void OnConfiguringDatabase(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(sqlServerOptions.Value.ConnectionString, b => b
                .CommandTimeout(sqlServerOptions.Value.CommandTimeout));
        }

    }

}
