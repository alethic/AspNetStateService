using System;
using System.Threading.Tasks;

using Cogito.Autofac;

namespace AspNetStateService.EntityFrameworkCore.SqlServer
{

    [RegisterNamed(typeof(IStateObjectDbContextProvider), "SqlServer")]
    [RegisterInstancePerLifetimeScope]
    public class SqlServerStateObjectDbContextProvider : IStateObjectDbContextProvider
    {

        readonly Func<SqlServerStateObjectDbContext> dbContextFactory;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="dbContextFactory"></param>
        public SqlServerStateObjectDbContextProvider(Func<SqlServerStateObjectDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public Task<StateObjectDbContext> GetDbContextAsync()
        {
            return Task.FromResult<StateObjectDbContext>(dbContextFactory());
        }

    }

}
