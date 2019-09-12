using System;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Interfaces;
using Serilog;

namespace AspNetStateService.Core
{

    /// <summary>
    /// Implements the business operations of a <see cref="IStateObject"/>.
    /// </summary>
    public class StateObject : IStateObject
    {

        readonly string id;
        readonly IStateObjectDataStore store;
        readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="store"></param>
        /// <param name="logger"></param>
        public StateObject(string id, IStateObjectDataStore store, ILogger logger)
        {
            this.id = id ?? throw new ArgumentNullException(nameof(id));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DataResponse> Get(CancellationToken cancellationToken)
        {
            logger.Verbose("Get operation for {StateId}.", id);

            var r = new DataResponse();

            // load current information
            var (l, c) = await store.GetLockAsync(id, cancellationToken);
            var (d, f, t, m) = await store.GetDataAsync(id, cancellationToken);

            // expire session if required
            if ((m + t) - DateTime.UtcNow < TimeSpan.Zero)
            {
                await store.RemoveLockAsync(id, cancellationToken);
                await store.RemoveDataAsync(id, cancellationToken);

                // reload current state
                (l, c) = await store.GetLockAsync(id, cancellationToken);
                (d, f, t, m) = await store.GetDataAsync(id, cancellationToken);
            }

            // return lock information if present
            if (l != null)
            {
                r.LockCookie = l;
                r.LockTime = c;
                r.LockAge = DateTime.UtcNow - r.LockTime;
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
            r.Timeout = (m + t) - DateTime.UtcNow;

            // flag specified
            if (f == 1)
            {
                r.ActionFlags = 1;
                await store.SetFlagAsync(id, 0, cancellationToken);
            }

            return r;
        }

        public async Task<DataResponse> GetExclusive(CancellationToken cancellationToken)
        {
            logger.Verbose("GetExclusive operation for {StateId}.", id);

            var r = await Get(cancellationToken);

            // process requested lock if data successfully retrieved
            if (r.Status == ResponseStatus.Ok)
            {
                r.LockCookie = (uint)Guid.NewGuid().GetHashCode();
                r.LockTime = DateTime.UtcNow;
                r.LockAge = TimeSpan.Zero;
                await store.SetLockAsync(id, (uint)r.LockCookie, (DateTime)r.LockTime, cancellationToken);
            }

            return r;
        }

        public async Task<Response> Set(uint? cookie, byte[] data, uint? flag, TimeSpan? time, CancellationToken cancellationToken)
        {
            logger.Verbose("Set operation for {StateId} with {DataSize} bytes of data.", id, data?.Length);

            var r = new Response();

            var (l, c) = await store.GetLockAsync(id, cancellationToken);
            var (d, f, t, m) = await store.GetDataAsync(id, cancellationToken);

            // expire session if required
            if ((m + t) - DateTime.UtcNow < TimeSpan.Zero)
            {
                await store.RemoveLockAsync(id, cancellationToken);
                await store.RemoveDataAsync(id, cancellationToken);

                // reload current state
                (l, c) = await store.GetLockAsync(id, cancellationToken);
                (d, f, t, m) = await store.GetDataAsync(id, cancellationToken);
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
                r.LockAge = DateTime.UtcNow - r.LockTime;
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
            await store.SetDataAsync(id, data, flag, time, cancellationToken);
            (d, f, t, m) = await store.GetDataAsync(id, cancellationToken);

            r.Status = ResponseStatus.Ok;
            r.Timeout = (m + t) - DateTime.UtcNow;

            // ensure we auto expire
            if (r.Timeout > TimeSpan.Zero)
                await store.SetTimeoutAsync(id, r.Timeout, cancellationToken);

            return r;
        }

        public async Task<Response> ReleaseExclusive(uint cookie, CancellationToken cancellationToken)
        {
            logger.Verbose("ReleaseExclusive operation for {StateId}.", id);

            var r = new Response();

            var (l, c) = await store.GetLockAsync(id, cancellationToken);
            var (d, f, t, m) = await store.GetDataAsync(id, cancellationToken);

            r.LockCookie = l;
            r.LockTime = c;
            r.LockAge = DateTime.UtcNow - r.LockTime;

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

            // remove any locks
            await store.RemoveLockAsync(id, cancellationToken);
            r.Status = ResponseStatus.Ok;
            return r;
        }

        public async Task<Response> Remove(uint? cookie, CancellationToken cancellationToken)
        {
            logger.Verbose("Remove operation for {StateId}.", id);

            var r = new Response();

            var (l, c) = await store.GetLockAsync(id, cancellationToken);
            var (d, f, t, m) = await store.GetDataAsync(id, cancellationToken);

            r.LockCookie = l;
            r.LockTime = c;
            r.LockAge = DateTime.UtcNow - r.LockTime;

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

            await store.RemoveDataAsync(id, cancellationToken);
            r.Status = ResponseStatus.Ok;
            return r;
        }

        public async Task<Response> ResetTimeout(CancellationToken cancellationToken)
        {
            logger.Verbose("ResetTimeout operation for {StateId}.", id);

            var r = new Response();

            var (l, c) = await store.GetLockAsync(id, cancellationToken);
            var (d, f, t, m) = await store.GetDataAsync(id, cancellationToken);

            r.LockCookie = l;
            r.LockTime = c;
            r.LockAge = DateTime.UtcNow - r.LockTime;

            if (d == null)
            {
                r.Status = ResponseStatus.NotFound;
                return r;
            }

            // refresh data, which refreshes timeout
            await store.SetDataAsync(id, d, f, t, cancellationToken);
            (d, f, t, m) = await store.GetDataAsync(id, cancellationToken);

            r.Status = ResponseStatus.Ok;
            r.Timeout = (m + t) - DateTime.UtcNow;

            // ensure we auto expire
            if (r.Timeout > TimeSpan.Zero)
                await store.SetTimeoutAsync(id, r.Timeout, cancellationToken);

            return r;
        }

    }

}
