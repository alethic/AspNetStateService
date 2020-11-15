using System.Threading;
using System.Threading.Tasks;

namespace AspNetStateService.Interfaces
{

    /// <summary>
    /// Provides a method to look up the state object for the given state ID.
    /// </summary>
    public interface IStateObjectProvider
    {

        /// <summary>
        /// Starts the provider.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stops the provider.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StopAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the state object for the specified ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IStateObject> GetStateObjectAsync(string id, CancellationToken cancellationToken);

    }

}
