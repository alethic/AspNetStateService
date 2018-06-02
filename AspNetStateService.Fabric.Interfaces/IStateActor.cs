using System;
using System.Threading.Tasks;

using AspNetStateService.Interfaces;

using Microsoft.ServiceFabric.Actors;

namespace AspNetStateService.Fabric.Interfaces
{

    /// <summary>
    /// Describes an actor that proxies to a <see cref="IStateObject"/>.
    /// </summary>
    public interface IStateActor : IActor
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
