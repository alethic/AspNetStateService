using System.Threading.Tasks;

using StackExchange.Redis;

namespace AspNetStateService.Redis
{

    public interface IRedisConnectionProvider
    {

        Task<ConnectionMultiplexer> GetConnectionAsync();

    }

}