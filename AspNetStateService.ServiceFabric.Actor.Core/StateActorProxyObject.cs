using System;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.ServiceFabric.Actor.Interfaces;
using AspNetStateService.Interfaces;

namespace AspNetStateService.ServiceFabric.Actor.Core
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

        public Task<DataResponse> Get(CancellationToken cancellationToken)
        {
            return actor.Get(cancellationToken);
        }

        public Task<DataResponse> GetExclusive(CancellationToken cancellationToken)
        {
            return actor.GetExclusive(cancellationToken);
        }

        public Task<Response> Set(uint? cookie, byte[] data, uint? extraFlags, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            return actor.Set(cookie, data, extraFlags, timeout, cancellationToken);
        }

        public Task<Response> ReleaseExclusive(uint cookie, CancellationToken cancellationToken)
        {
            return actor.ReleaseExclusive(cookie, cancellationToken);
        }

        public Task<Response> Remove(uint? cookie, CancellationToken cancellationToken)
        {
            return actor.Remove(cookie, cancellationToken);
        }

        public Task<Response> ResetTimeout(CancellationToken cancellationToken)
        {
            return actor.ResetTimeout(cancellationToken);
        }

    }

}
