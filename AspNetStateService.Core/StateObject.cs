using System;
using System.Threading.Tasks;

using AspNetStateService.Interfaces;

namespace AspNetStateService.Core
{

    /// <summary>
    /// Implements the business operations of a <see cref="IStateObject"/>.
    /// </summary>
    public class StateObject : IStateObject
    {

        readonly string id;
        readonly IStateObjectDataStore store;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="store"></param>
        public StateObject(string id, IStateObjectDataStore store)
        {
            this.id = id ?? throw new ArgumentNullException(nameof(id));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public async Task<DataResponse> Get()
        {
            var r = new DataResponse();

            // load current information
            var (l, c) = await store.GetLockAsync(id);
            var (d, f, t, u) = await store.GetDataAsync(id);

            // expire session if required
            if ((u + t) - DateTime.Now < TimeSpan.Zero)
            {
                await store.RemoveLockAsync(id);
                await store.RemoveDataAsync(id);

                // reload current state
                (l, c) = await store.GetLockAsync(id);
                (d, f, t, u) = await store.GetDataAsync(id);
            }

            // return lock information if present
            if (l != null)
            {
                r.LockCookie = l;
                r.LockTime = c;
                r.LockAge = DateTime.Now - r.LockTime;
            }

            // no data found
            if (d == null)
            {
                r.Status = ResponseStatus.NotFound;
                return r;
            }

            // invalid lock
            if (l != null)
            {
                r.Status = ResponseStatus.Locked;
                return r;
            }

            // validate access to data
            r.Status = ResponseStatus.Ok;
            r.Data = d;
            r.Timeout = (u + t) - DateTime.Now;

            // flag specified
            if (f == 1)
            {
                r.ActionFlags = 1;
                await store.SetFlagAsync(id, 0);
            }

            return r;
        }

        public async Task<DataResponse> GetExclusive()
        {
            var r = await Get();

            // process requested lock if data successfully retrieved
            if (r.Status == ResponseStatus.Ok)
            {
                r.LockCookie = (uint)Guid.NewGuid().GetHashCode();
                r.LockTime = DateTime.Now;
                r.LockAge = TimeSpan.Zero;
                await store.SetLockAsync(id, (uint)r.LockCookie, (DateTime)r.LockTime);
            }

            return r;
        }

        public async Task<Response> Set(uint? cookie, byte[] data, uint? flag, TimeSpan? time)
        {
            var r = new Response();

            var (l, c) = await store.GetLockAsync(id);
            var (d, f, t, u) = await store.GetDataAsync(id);

            // expire session if required
            if ((u + t) - DateTime.Now < TimeSpan.Zero)
            {
                await store.RemoveLockAsync(id);
                await store.RemoveDataAsync(id);

                // reload current state
                (l, c) = await store.GetLockAsync(id);
                (d, f, t, u) = await store.GetDataAsync(id);
            }

            // no data sent
            if (data == null)
            {
                r.Status = ResponseStatus.BadRequest;
                return r;
            }

            // return lock information if present
            if (l != null)
            {
                r.LockCookie = l;
                r.LockTime = c;
                r.LockAge = DateTime.Now - r.LockTime;
            }

            // flag is set
            if (d != null && flag == 1)
            {
                r.Status = ResponseStatus.Ok;
                return r;
            }

            // data exists and is locked
            if (d != null && l != null && l != cookie)
            {
                r.Status = ResponseStatus.Locked;
                return r;
            }

            // save new data
            await store.SetDataAsync(id, data, flag, time);
            (d, f, t, u) = await store.GetDataAsync(id);

            r.Status = ResponseStatus.Ok;
            r.Timeout = (u + t) - DateTime.Now;

            // ensure we auto expire
            if (r.Timeout > TimeSpan.Zero)
                await store.SetTimeoutAsync(id, r.Timeout);

            return r;
        }

        public async Task<Response> ReleaseExclusive(uint cookie)
        {
            var r = new Response();

            var (l, c) = await store.GetLockAsync(id);
            var (d, f, u, t) = await store.GetDataAsync(id);

            r.LockCookie = l;
            r.LockTime = c;
            r.LockAge = DateTime.Now - r.LockTime;

            if (d == null)
            {
                r.Status = ResponseStatus.NotFound;
                return r;
            }

            if (l != 0 && l != cookie)
            {
                r.Status = ResponseStatus.Locked;
                return r;
            }

            // remov any locks
            await store.RemoveLockAsync(id);
            r.Status = ResponseStatus.Ok;
            return r;
        }

        public async Task<Response> Remove(uint? cookie)
        {
            var r = new Response();

            var (l, c) = await store.GetLockAsync(id);
            var (d, f, u, t) = await store.GetDataAsync(id);

            r.LockCookie = l;
            r.LockTime = c;
            r.LockAge = DateTime.Now - r.LockTime;

            if (d == null)
            {
                r.Status = ResponseStatus.NotFound;
                return r;
            }

            if (l != 0 && l != cookie)
            {
                r.Status = ResponseStatus.Locked;
                return r;
            }

            await store.RemoveDataAsync(id);
            r.Status = ResponseStatus.Ok;
            return r;
        }

        public async Task<Response> ResetTimeout()
        {
            var r = new Response();

            var (l, c) = await store.GetLockAsync(id);
            var (d, f, t, u) = await store.GetDataAsync(id);

            r.LockCookie = l;
            r.LockTime = c;
            r.LockAge = DateTime.Now - r.LockTime;

            if (d == null)
            {
                r.Status = ResponseStatus.NotFound;
                return r;
            }

            // refresh data, which refreshes timeout
            await store.SetDataAsync(id, d, f, t);
            (d, f, t, u) = await store.GetDataAsync(id);

            r.Status = ResponseStatus.Ok;
            r.Timeout = (u + t) - DateTime.Now;

            // ensure we auto expire
            if (r.Timeout > TimeSpan.Zero)
                await store.SetTimeoutAsync(id, r.Timeout);

            return r;
        }

    }

}
