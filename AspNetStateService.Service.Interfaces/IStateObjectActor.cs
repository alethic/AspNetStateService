using System;
using System.Threading.Tasks;

using Microsoft.ServiceFabric.Actors;

namespace AspNetStateService.Service.Interfaces
{

    public interface IStateObjectActor : IActor
    {

        Task<DataResponse> Get();

        Task<DataResponse> GetExclusive();

        Task<Response> Set(uint? cookie, byte[] data, uint? flag, TimeSpan? time);

        Task<Response> ReleaseExclusive(uint cookie);

        Task<Response> Remove(uint cookie);

        Task<Response> ResetTimeout();

    }

}
