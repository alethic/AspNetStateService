using System;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Interfaces;

using Microsoft.ServiceFabric.Actors;

namespace AspNetStateService.ServiceFabric.Actor.Interfaces
{

    /// <summary>
    /// Describes an actor that proxies to a <see cref="IStateObject"/>.
    /// </summary>
    public interface IStateActor : IActor
    {

        /// <summary>
        /// Implemenets the Get operation.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DataResponse> Get(CancellationToken cancellationToken);

        /// <summary>
        /// Implements the GetExclusive operation.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DataResponse> GetExclusive(CancellationToken cancellationToken);

        /// <summary>
        /// Implements the Set operation.
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="data"></param>
        /// <param name="extraFlags"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Response> Set(uint? cookie, byte[] data, uint? extraFlags, TimeSpan? timeout, CancellationToken cancellationToken);

        /// <summary>
        /// Implements the ReleaseExclusive operation.
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Response> ReleaseExclusive(uint cookie, CancellationToken cancellationToken);

        /// <summary>
        /// Implements the Remove operation.
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Response> Remove(uint? cookie, CancellationToken cancellationToken);

        /// <summary>
        /// Implements the ResetTimeout operation.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Response> ResetTimeout(CancellationToken cancellationToken);

        /// <summary>
        /// Returns <c>true</c> if the actor state is expired.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> IsExpired(CancellationToken cancellationToken);

    }

}
