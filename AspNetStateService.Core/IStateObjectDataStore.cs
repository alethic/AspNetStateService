using System;
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
        /// <param name="data"></param>
        /// <param name="extraFlags"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task SetDataAsync(byte[] data, uint? extraFlags, TimeSpan? timeout);

        /// <summary>
        /// Sets the extra flags field.
        /// </summary>
        /// <param name="extraFlags"></param>
        /// <returns></returns>
        Task SetFlagAsync(uint? extraFlags);

        /// <summary>
        /// Gets the current data fields.
        /// </summary>
        /// <returns></returns>
        Task<(byte[] data, uint? extraFlags, TimeSpan? timeout, DateTime? touch)> GetDataAsync();

        /// <summary>
        /// Removes the current data.
        /// </summary>
        /// <returns></returns>
        Task RemoveDataAsync();

        /// <summary>
        /// Gets the current lock fields.
        /// </summary>
        /// <returns></returns>
        Task<(uint? cookie, DateTime? time)> GetLockAsync();

        /// <summary>
        /// Sets the current lock fields.
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        Task SetLockAsync(uint cookie, DateTime time);

        /// <summary>
        /// Removes the current lock.
        /// </summary>
        /// <returns></returns>
        Task RemoveLockAsync();

        /// <summary>
        /// Signaled to set the expire at time.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task SetTimeoutAsync(TimeSpan? timeout);

    }

}