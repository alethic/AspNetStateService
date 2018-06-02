using System.Threading.Tasks;

namespace AspNetStateService.EntityFrameworkCore
{

    public interface IStateObjectDbContextProvider
    {

        /// <summary>
        /// Gets the <see cref="StateObjectDbContext"/> used to serve requests.
        /// </summary>
        /// <returns></returns>
        Task<StateObjectDbContext> CreateDbContextAsync();

    }

}
