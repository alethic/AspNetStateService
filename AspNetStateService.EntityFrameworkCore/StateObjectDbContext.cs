using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AspNetStateService.EntityFrameworkCore
{

    /// <summary>
    /// Base <see cref="DbContext"/> for StateObjects. Must extend to implement specific connection configuration.
    /// </summary>
    public abstract class StateObjectDbContext :
        DbContext
    {

        readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="loggerFactory"></param>
        public StateObjectDbContext(ILoggerFactory loggerFactory) :
            base()
        {
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            OnConfiguringDatabase(optionsBuilder);
            optionsBuilder.UseLoggerFactory(loggerFactory);
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
