using Cogito.Autofac;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetStateService.EntityFrameworkCore.InMemory
{

    [RegisterAs(typeof(InMemoryStateObjectDbContext))]
    [RegisterInstancePerLifetimeScope]
    public class InMemoryStateObjectDbContext : StateObjectDbContext
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="options"></param>
        public InMemoryStateObjectDbContext(ILoggerFactory loggerFactory, IOptions<StateObjectDbContextDataStoreOptions> options) :
            base(loggerFactory, options)
        {

        }

        protected override void OnConfiguringDatabase(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("AspNetStateService");
        }

    }

}
