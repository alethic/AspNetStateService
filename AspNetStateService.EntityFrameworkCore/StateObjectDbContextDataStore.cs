using System;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Core;

using Cogito.Autofac;

using Microsoft.EntityFrameworkCore;

namespace AspNetStateService.EntityFrameworkCore
{

    /// <summary>
    /// Implements a <see cref="IStateObjectDataStore"/> using Entity Framework Core.
    /// </summary>
    [RegisterAs(typeof(IStateObjectDataStore))]
    public class StateObjectDbContextDataStore : IStateObjectDataStore
    {

        readonly IStateObjectDbContextProvider dbContextProvider;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="dbContextProvider"></param>
        public StateObjectDbContextDataStore(IStateObjectDbContextProvider dbContextProvider)
        {
            this.dbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
        }

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
            using (var db = await dbContextProvider.CreateDbContextAsync())
            {
                var o = await db.Set<StateObjectData>().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
                return (o?.Data, o?.ExtraFlags, o?.Timeout, o?.Altered);
            }
        }

        public async Task<(uint? cookie, DateTime? time)> GetLockAsync(string id, CancellationToken cancellationToken)
        {
            using (var db = await dbContextProvider.CreateDbContextAsync())
            {
                var o = await db.Set<StateObjectData>().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
                return (o?.LockCookie, o?.LockTime);
            }
        }

        public async Task RemoveDataAsync(string id, CancellationToken cancellationToken)
        {
            using (var db = await dbContextProvider.CreateDbContextAsync())
            {
                var o = await db.Set<StateObjectData>().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
                if (o != null)
                {
                    o.Data = null;
                    o.ExtraFlags = null;
                    o.Timeout = null;
                    o.Altered = null;
                }

                await db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RemoveLockAsync(string id, CancellationToken cancellationToken)
        {
            using (var db = await dbContextProvider.CreateDbContextAsync())
            {
                var o = await db.Set<StateObjectData>().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
                if (o != null)
                {
                    o.LockCookie = null;
                    o.LockTime = null;
                }

                await db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task SetDataAsync(string id, byte[] data, uint? extraFlags, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            using (var db = await dbContextProvider.CreateDbContextAsync())
            {
                var o = await db.Set<StateObjectData>().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
                if (o == null)
                    db.StateObjects.Add(o = new StateObjectData() { Id = id });

                o.Data = data;
                o.ExtraFlags = extraFlags;
                o.Timeout = timeout;
                o.Altered = DateTime.UtcNow;

                await db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task SetFlagAsync(string id, uint? extraFlags, CancellationToken cancellationToken)
        {
            using (var db = await dbContextProvider.CreateDbContextAsync())
            {
                var o = await db.Set<StateObjectData>().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
                if (o == null)
                    db.StateObjects.Add(o = new StateObjectData() { Id = id });

                o.ExtraFlags = extraFlags;

                await db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task SetLockAsync(string id, uint cookie, DateTime time, CancellationToken cancellationToken)
        {
            using (var db = await dbContextProvider.CreateDbContextAsync())
            {
                var o = await db.Set<StateObjectData>().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
                if (o == null)
                    db.StateObjects.Add(o = new StateObjectData() { Id = id });

                o.LockCookie = cookie;
                o.LockTime = time;

                await db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task SetTimeoutAsync(string id, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            using (var db = await dbContextProvider.CreateDbContextAsync())
            {
                var o = await db.Set<StateObjectData>().FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
                if (o == null)
                    db.StateObjects.Add(o = new StateObjectData() { Id = id });

                o.Timeout = timeout;

                await db.SaveChangesAsync(cancellationToken);
            }
        }

    }

}
