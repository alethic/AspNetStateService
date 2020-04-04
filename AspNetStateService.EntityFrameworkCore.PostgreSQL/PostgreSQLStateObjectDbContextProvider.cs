using System;
using System.Threading.Tasks;

using Cogito.Autofac;

namespace AspNetStateService.EntityFrameworkCore.PostgreSQL
{

    [RegisterNamed(typeof(IStateObjectDbContextProvider), "PostgreSQL")]
    [RegisterInstancePerLifetimeScope]
    public class PostgreSQLStateObjectDbContextProvider : IStateObjectDbContextProvider
    {

        readonly Func<PostgreSQLStateObjectDbContext> dbContextFactory;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="dbContextFactory"></param>
        public PostgreSQLStateObjectDbContextProvider(Func<PostgreSQLStateObjectDbContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public Task<StateObjectDbContext> GetDbContextAsync()
        {
            return Task.FromResult<StateObjectDbContext>(dbContextFactory());
        }

    }

}
