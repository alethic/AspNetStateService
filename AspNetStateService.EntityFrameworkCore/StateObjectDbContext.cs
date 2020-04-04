using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetStateService.EntityFrameworkCore
{

    /// <summary>
    /// Base <see cref="DbContext"/> for StateObjects. Must extend to implement specific connection configuration.
    /// </summary>
    public abstract class StateObjectDbContext :
        DbContext
    {

        readonly ILoggerFactory loggerFactory;
        readonly IOptions<StateObjectDbContextDataStoreOptions> options;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="loggerFactory"></param>
        public StateObjectDbContext(ILoggerFactory loggerFactory, IOptions<StateObjectDbContextDataStoreOptions> options) :
            base()
        {
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            OnConfiguringDatabase(optionsBuilder);

            // tie into logging infrastructure
            optionsBuilder.UseLoggerFactory(loggerFactory);

            // user might want to log sensntive data
            if (options.Value.EnableSensitiveDataLogging != null)
                optionsBuilder.EnableSensitiveDataLogging((bool)options.Value.EnableSensitiveDataLogging);
        }

        /// <summary>
        /// Override this method to configure the database.
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected abstract void OnConfiguringDatabase(DbContextOptionsBuilder optionsBuilder);

        /// <summary>
        /// Invoked to configure the model.
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SessionObject>()
                .HasIndex(i => i.Key)
                .IsUnique();
        }

        /// <summary>
        /// Gets the set of all available state objects data.
        /// </summary>
        public DbSet<SessionObject> Sessions { get; set; }

        /// <summary>
        /// Gets the set of all available state objects data.
        /// </summary>
        public DbSet<SessionDataObject> SessionData { get; set; }

    }

}
