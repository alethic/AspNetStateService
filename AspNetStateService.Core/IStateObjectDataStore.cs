using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetStateService.Core
{

    /// <summary>
    /// Describes the interfaces for a state object to interact with it's data store.
    /// </summary>
    public interface IStateObjectDataStore
    {

        /// <summary>
        /// Sets the data fields.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="extraFlags"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetDataAsync(string id, byte[] data, uint? extraFlags, TimeSpan? timeout, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the extra flags field.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="extraFlags"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetFlagAsync(string id, uint? extraFlags, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the current data fields.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<(byte[] data, uint? extraFlags, TimeSpan? timeout, DateTime? altered)> GetDataAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Removes the current data.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RemoveDataAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the current lock fields.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<(uint? cookie, DateTime? time)> GetLockAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the current lock fields.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cookie"></param>
        /// <param name="time"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetLockAsync(string id, uint cookie, DateTime time, CancellationToken cancellationToken);

        /// <summary>
        /// Removes the current lock.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RemoveLockAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Signaled to set the expire at time.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetTimeoutAsync(string id, TimeSpan? timeout, CancellationToken cancellationToken);

    }

}