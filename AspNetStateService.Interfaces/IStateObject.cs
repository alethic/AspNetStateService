using System;
using System.Threading.Tasks;

namespace AspNetStateService.Interfaces
{

    /// <summary>
    /// Describes an object that manages a single set of state.
    /// </summary>
    public interface IStateObject
    {

        /// <summary>
        /// Implemenets the Get operation.
        /// </summary>
        /// <returns></returns>
        Task<DataResponse> Get();

        /// <summary>
        /// Implements the GetExclusive operation.
        /// </summary>
        /// <returns></returns>
        Task<DataResponse> GetExclusive();

        /// <summary>
        /// Implements the Set operation.
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="data"></param>
        /// <param name="extraFlags"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task<Response> Set(uint? cookie, byte[] data, uint? extraFlags, TimeSpan? timeout);

        /// <summary>
        /// Implements the ReleaseExclusive operation.
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        Task<Response> ReleaseExclusive(uint cookie);

        /// <summary>
        /// Implements the Remove operation.
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        Task<Response> Remove(uint? cookie);

        /// <summary>
        /// Implements the ResetTimeout operation.
        /// </summary>
        /// <returns></returns>
        Task<Response> ResetTimeout();

    }

}
