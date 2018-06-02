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

        readonly IStateObjectDataStore store;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="store"></param>
        public StateObject(IStateObjectDataStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public async Task<DataResponse> Get()
        {
            var r = new DataResponse();

            // load current information
            var (l, c) = await store.GetLockAsync();
            var (d, f, t, u) = await store.GetDataAsync();

            // expire session if required
            if ((u + t) - DateTime.Now < TimeSpan.Zero)
            {
                await store.RemoveLockAsync();
                await store.RemoveDataAsync();

                // reload current state
                (l, c) = await store.GetLockAsync();
                (d, f, t, u) = await store.GetDataAsync();
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
                await store.SetFlagAsync(0);
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
                await store.SetLockAsync((uint)r.LockCookie, (DateTime)r.LockTime);
            }

            return r;
        }

        public async Task<Response> Set(uint? cookie, byte[] data, uint? flag, TimeSpan? time)
        {
            var r = new Response();

            var (l, c) = await store.GetLockAsync();
            var (d, f, t, u) = await store.GetDataAsync();

            // expire session if required
            if ((u + t) - DateTime.Now < TimeSpan.Zero)
            {
                await store.RemoveLockAsync();
                await store.RemoveDataAsync();

                // reload current state
                (l, c) = await store.GetLockAsync();
                (d, f, t, u) = await store.GetDataAsync();
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
            await store.SetDataAsync(data, flag, time);
            (d, f, t, u) = await store.GetDataAsync();

            r.Status = ResponseStatus.Ok;
            r.Timeout = (u + t) - DateTime.Now;

            // ensure we auto expire
            if (r.Timeout > TimeSpan.Zero)
                await store.SetTimeoutAsync(r.Timeout);

            return r;
        }

        public async Task<Response> ReleaseExclusive(uint cookie)
        {
            var r = new Response();

            var (l, c) = await store.GetLockAsync();
            var (d, f, u, t) = await store.GetDataAsync();

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
            await store.RemoveLockAsync();
            r.Status = ResponseStatus.Ok;
            return r;
        }

        public async Task<Response> Remove(uint? cookie)
        {
            var r = new Response();

            var (l, c) = await store.GetLockAsync();
            var (d, f, u, t) = await store.GetDataAsync();

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

            await store.RemoveDataAsync();
            r.Status = ResponseStatus.Ok;
            return r;
        }

        public async Task<Response> ResetTimeout()
        {
            var r = new Response();

            var (l, c) = await store.GetLockAsync();
            var (d, f, t, u) = await store.GetDataAsync();

            r.LockCookie = l;
            r.LockTime = c;
            r.LockAge = DateTime.Now - r.LockTime;

            if (d == null)
            {
                r.Status = ResponseStatus.NotFound;
                return r;
            }

            // refresh data, which refreshes timeout
            await store.SetDataAsync(d, f, t);
            (d, f, t, u) = await store.GetDataAsync();

            r.Status = ResponseStatus.Ok;
            r.Timeout = (u + t) - DateTime.Now;

            // ensure we auto expire
            if (r.Timeout > TimeSpan.Zero)
                await store.SetTimeoutAsync(r.Timeout);

            return r;
        }

    }

}
