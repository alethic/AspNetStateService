using System;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Core;

using Autofac;

using Cogito.Autofac;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AspNetStateService.EntityFrameworkCore
{

    /// <summary>
    /// Implements a <see cref="IStateObjectDataStore"/> using Entity Framework Core.
    /// </summary>
    [RegisterNamed(typeof(IStateObjectDataStore), "EntityFrameworkCore")]
    [RegisterInstancePerLifetimeScope]
    [RegisterWithAttributeFiltering]
    public class StateObjectDbContextDataStore : IStateObjectDataStore
    {

        readonly IComponentContext context;
        readonly Lazy<IStateObjectDbContextProvider> dbContextProvider;
        readonly IOptions<StateObjectDbContextDataStoreOptions> options;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        public StateObjectDbContextDataStore(IComponentContext context, IOptions<StateObjectDbContextDataStoreOptions> options)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            dbContextProvider = new Lazy<IStateObjectDbContextProvider>(GetDbContextProvider);
        }

        /// <summary>
        /// Gets the DbContextProvider.
        /// </summary>
        /// <returns></returns>
        IStateObjectDbContextProvider GetDbContextProvider()
        {
            return context.ResolveNamed<IStateObjectDbContextProvider>(options.Value.Provider ?? "InMemory");
        }

        /// <summary>
        /// Gets the DbContextProvider.
        /// </summary>
        IStateObjectDbContextProvider DbContextProvider => dbContextProvider.Value;

        /// <summary>
        /// Initializes the data store.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task InitAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task<(byte[] data, uint? extraFlags, TimeSpan? timeout, DateTime? altered)> GetDataAsync(string id, CancellationToken cancellationToken)
        {
            var db = await DbContextProvider.GetDbContextAsync();
            var ob = await db.Sessions.Include(i => i.Data).FirstOrDefaultAsync(i => i.Key == id, cancellationToken);
            return (ob?.Data?.Value, ob?.ExtraFlags, ob?.Timeout, ob?.Altered);
        }

        public async Task<(uint? cookie, DateTime? time)> GetLockAsync(string id, CancellationToken cancellationToken)
        {
            var db = await DbContextProvider.GetDbContextAsync();
            var ob = await db.Sessions.FirstOrDefaultAsync(i => i.Key == id, cancellationToken);
            return (ob?.LockCookie, ob?.LockTime);
        }

        public async Task RemoveDataAsync(string id, CancellationToken cancellationToken)
        {
            var db = await DbContextProvider.GetDbContextAsync();
            var ob = await db.Sessions.FirstOrDefaultAsync(i => i.Key == id, cancellationToken);
            if (ob != null)
            {
                // schedule deletion of data
                if (ob.Data != null)
                    db.SessionData.Remove(ob.Data);

                ob.Data = null;
                ob.ExtraFlags = null;
                ob.Timeout = null;
                ob.Altered = null;
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveLockAsync(string id, CancellationToken cancellationToken)
        {
            var db = await DbContextProvider.GetDbContextAsync();
            var ob = await db.Sessions.FirstOrDefaultAsync(i => i.Key == id, cancellationToken);
            if (ob != null)
            {
                ob.LockCookie = null;
                ob.LockTime = null;
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task SetDataAsync(string id, byte[] data, uint? extraFlags, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            var db = await DbContextProvider.GetDbContextAsync();

            var ob = await db.Sessions.FirstOrDefaultAsync(i => i.Key == id, cancellationToken);
            if (ob == null)
                db.Sessions.Add(ob = new SessionObject() { Key = id });

            ob.ExtraFlags = extraFlags;
            ob.Timeout = timeout;
            ob.Altered = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);

            var dt = await db.SessionData.FirstOrDefaultAsync(i => i.Id == ob.Id, cancellationToken);
            if (dt == null)
                db.SessionData.Add(dt = new SessionDataObject() { Id = ob.Id });

            dt.Value = data;
            ob.Data = dt;
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task SetFlagAsync(string id, uint? extraFlags, CancellationToken cancellationToken)
        {
            var db = await DbContextProvider.GetDbContextAsync();
            var ob = await db.Sessions.FirstOrDefaultAsync(i => i.Key == id, cancellationToken);
            if (ob == null)
                db.Sessions.Add(ob = new SessionObject() { Key = id });

            ob.ExtraFlags = extraFlags;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task SetLockAsync(string id, uint cookie, DateTime time, CancellationToken cancellationToken)
        {
            var db = await DbContextProvider.GetDbContextAsync();
            var ob = await db.Sessions.FirstOrDefaultAsync(i => i.Key == id, cancellationToken);
            if (ob == null)
                db.Sessions.Add(ob = new SessionObject() { Key = id });

            ob.LockCookie = cookie;
            ob.LockTime = time;

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task SetTimeoutAsync(string id, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            var db = await DbContextProvider.GetDbContextAsync();
            var ob = await db.Sessions.FirstOrDefaultAsync(i => i.Key == id, cancellationToken);
            if (ob == null)
                db.Sessions.Add(ob = new SessionObject() { Key = id });

            ob.Timeout = timeout;

            await db.SaveChangesAsync(cancellationToken);
        }

    }

}
