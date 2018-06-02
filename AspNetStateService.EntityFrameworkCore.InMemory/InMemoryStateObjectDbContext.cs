using FileAndServe.Autofac;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AspNetStateService.EntityFrameworkCore.InMemory
{

    [RegisterAs(typeof(InMemoryStateObjectDbContext))]
    public class InMemoryStateObjectDbContext : StateObjectDbContext
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="loggerFactory"></param>
        public InMemoryStateObjectDbContext(ILoggerFactory loggerFactory) :
            base(loggerFactory)
        {

        }

        protected override void OnConfiguringDatabase(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("AspNetStateService");
            optionsBuilder.EnableSensitiveDataLogging(true);
        }

    }

}
