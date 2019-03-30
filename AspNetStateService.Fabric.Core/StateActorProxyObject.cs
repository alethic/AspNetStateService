using System;
using System.Threading.Tasks;

using AspNetStateService.Fabric.Interfaces;
using AspNetStateService.Interfaces;

namespace AspNetStateService.Fabric.Core
{

    /// <summary>
    /// Exposes a <see cref="IStateActor"/> as a <see cref="IStateObject"/>.
    /// </summary>
    public class StateActorProxyObject : IStateObject
    {

        readonly IStateActor actor;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="actor"></param>
        public StateActorProxyObject(IStateActor actor)
        {
            this.actor = actor ?? throw new ArgumentNullException(nameof(actor));
        }

        public Task<DataResponse> Get()
        {
            return actor.Get();
        }

        public Task<DataResponse> GetExclusive()
        {
            return actor.GetExclusive();
        }

        public Task<Response> Set(uint? cookie, byte[] data, uint? extraFlags, TimeSpan? timeout)
        {
            return actor.Set(cookie, data, extraFlags, timeout);
        }

        public Task<Response> ReleaseExclusive(uint cookie)
        {
            return actor.ReleaseExclusive(cookie);
        }

        public Task<Response> Remove(uint? cookie)
        {
            return actor.Remove(cookie);
        }

        public Task<Response> ResetTimeout()
        {
            return actor.ResetTimeout();
        }

    }

}
