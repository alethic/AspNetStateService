using System.Threading;
using System.Threading.Tasks;

namespace AspNetStateService.Interfaces
{

    /// <summary>
    /// Provides a method to look up the state object for the given state ID.
    /// </summary>
    public interface IStateObjectProvider
    {

        Task<IStateObject> GetStateObjectAsync(string id, CancellationToken cancellationToken);

    }

}
