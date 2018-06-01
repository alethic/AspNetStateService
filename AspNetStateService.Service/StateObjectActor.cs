using System;
using System.Threading.Tasks;

using AspNetStateService.Service.Interfaces;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace AspNetStateService.Service
{

    [StatePersistence(StatePersistence.Volatile)]
    public class StateObjectActor : Actor, IStateObjectActor
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="actorService"></param>
        /// <param name="actorId"></param>
        public StateObjectActor(ActorService actorService, ActorId actorId) :
            base(actorService, actorId)
        {

        }

        /// <summary>
        /// Sets the current data value.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="flag"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        async Task SetDataAsync(byte[] data, uint? flag, TimeSpan? time)
        {
            await StateManager.SetStateAsync("Data", data);
            await StateManager.SetStateAsync("Flag", flag);
            await StateManager.SetStateAsync("Last", DateTime.Now);
            await StateManager.SetStateAsync("Time", time);
        }

        /// <summary>
        /// Sets the current flag value.
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        async Task SetFlagAsync(uint? flag)
        {
            await StateManager.SetStateAsync("Flag", flag);
        }

        /// <summary>
        /// Removes any stored data.
        /// </summary>
        /// <returns></returns>
        async Task RemoveDataAsync()
        {
            await StateManager.TryRemoveStateAsync("Data");
            await StateManager.TryRemoveStateAsync("Flag");
            await StateManager.TryRemoveStateAsync("Last");
            await StateManager.TryRemoveStateAsync("Time");
        }

        /// <summary>
        /// Gets the currently set data.
        /// </summary>
        /// <returns></returns>
        async Task<(byte[] data, uint? flag, DateTime? last, TimeSpan? time)> GetDataAsync()
        {
            var d = await StateManager.TryGetStateAsync<byte[]>("Data");
            var f = await StateManager.TryGetStateAsync<uint?>("Flag");
            var u = await StateManager.TryGetStateAsync<DateTime?>("Last");
            var t = await StateManager.TryGetStateAsync<TimeSpan?>("Time");

            return (
                d.HasValue ? d.Value : null,
                f.HasValue ? f.Value : null,
                u.HasValue ? u.Value : null,
                t.HasValue ? t.Value : null);
        }

        /// <summary>
        /// Sets the current lock data.
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        async Task SetLockAsync(uint cookie, DateTime update)
        {
            await StateManager.SetStateAsync("LockCookie", cookie);
            await StateManager.SetStateAsync("LockUpdate", update);
        }

        /// <summary>
        /// Unsets the current lock data.
        /// </summary>
        /// <returns></returns>
        async Task RemoveLockAsync()
        {
            await StateManager.TryRemoveStateAsync("LockCookie");
            await StateManager.TryRemoveStateAsync("LockCreate");
        }

        /// <summary>
        /// Gets the current lock data.
        /// </summary>
        /// <returns></returns>
        async Task<(uint? cookie, DateTime? create)> GetLockAsync()
        {
            var l = await StateManager.TryGetStateAsync<uint?>("LockCookie");
            var c = await StateManager.TryGetStateAsync<DateTime?>("LockCreate");

            return (
                l.HasValue ? l.Value : null,
                c.HasValue ? c.Value : null);
        }

        /// <summary>
        /// Processes a non-exclusive get request.
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public async Task<DataResponse> Get()
        {
            var r = new DataResponse();

            // load current information
            var (l, c) = await GetLockAsync();
            var (d, f, u, t) = await GetDataAsync();

            // return lock information if present
            if (l != null)
            {
                r.LockCookie = l;
                r.LockCreate = c;
                r.LockAge = DateTime.Now - r.LockCreate;
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
                await SetFlagAsync(0);
            }

            return r;
        }

        /// <summary>
        /// Processes an exclusive get request.
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public async Task<DataResponse> GetExclusive()
        {
            var r = await Get();

            // process requested lock if data successfully retrieved
            if (r.Status == ResponseStatus.Ok)
            {
                r.LockCookie = (uint)Guid.NewGuid().GetHashCode();
                r.LockCreate = DateTime.Now;
                r.LockAge = TimeSpan.Zero;
                await SetLockAsync((uint)r.LockCookie, (DateTime)r.LockCreate);
            }

            return r;
        }

        public async Task<Response> Set(uint? cookie, byte[] data, uint? flag, TimeSpan? time)
        {
            var r = new Response();

            var (l, c) = await GetLockAsync();
            var (d, f, u, t) = await GetDataAsync();

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
                r.LockCreate = c;
                r.LockAge = DateTime.Now - r.LockCreate;
            }

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
            await SetDataAsync(data, flag, time);
            (d, f, u, t) = await GetDataAsync();

            r.Status = ResponseStatus.Ok;
            r.Timeout = (u + t) - DateTime.Now;

            return r;
        }

        public async Task<Response> ReleaseExclusive(uint cookie)
        {
            var r = new Response();

            var (l, c) = await GetLockAsync();
            var (d, f, u, t) = await GetDataAsync();

            r.LockCookie = l;
            r.LockCreate = c;
            r.LockAge = DateTime.Now - r.LockCreate;

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
            await RemoveLockAsync();
            r.Status = ResponseStatus.Ok;
            return r;
        }

        public async Task<Response> Remove(uint cookie)
        {
            var r = new Response();

            var (l, c) = await GetLockAsync();
            var (d, f, u, t) = await GetDataAsync();

            r.LockCookie = l;
            r.LockCreate = c;
            r.LockAge = DateTime.Now - r.LockCreate;

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

            await RemoveDataAsync();
            r.Status = ResponseStatus.Ok;
            return r;
        }

        public async Task<Response> ResetTimeout()
        {
            var r = new Response();

            var (l, c) = await GetLockAsync();
            var (d, f, u, t) = await GetDataAsync();

            r.LockCookie = l;
            r.LockCreate = c;
            r.LockAge = DateTime.Now - r.LockCreate;

            if (d == null)
            {
                r.Status = ResponseStatus.NotFound;
                return r;
            }

            // refresh data, which refreshes timeout
            await SetDataAsync(d, f, t);
            r.Status = ResponseStatus.Ok;
            return r;
        }

    }

}
