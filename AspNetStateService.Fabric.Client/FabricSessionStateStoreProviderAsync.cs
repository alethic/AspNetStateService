using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

using AspNetStateService.Fabric.Interfaces;

using Cogito;

using Microsoft.AspNet.SessionState;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace AspNetStateService.Fabric.Client
{

    public class FabricSessionStateStoreProviderAsync : SessionStateStoreProviderAsyncBase
    {

        const string APPLICATION_URI_CONFIGURATION_NAME = "serviceUri";
        const string DEFAULT_STATE_APPLICATION_NAME = "fabric:/AspNetStateService";
        const string ACTOR_PROXY_KEY = "FabricSessionStateStoreProviderAsync:ActorProxy";

        string stateApplicationName;

        /// <summary>
        /// Initializes the session state store.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            stateApplicationName = config[APPLICATION_URI_CONFIGURATION_NAME]?.TrimOrNull() ?? DEFAULT_STATE_APPLICATION_NAME;

            base.Initialize(name, config);
        }

        /// <summary>
        /// Gets the <see cref="IStateActor"/> proxy instance for the given state ID.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        IStateActor GetActorProxy(HttpContext context, string id)
        {
            if (context.Items.Contains(ACTOR_PROXY_KEY) == false)
                context.Items[ACTOR_PROXY_KEY] = ActorProxy.Create<IStateActor>(new ActorId(id), applicationName: stateApplicationName);

            return (IStateActor)context.Items[ACTOR_PROXY_KEY];
        }

        public override void InitializeRequest(HttpContextBase context)
        {

        }

        public override SessionStateStoreData CreateNewStoreData(HttpContextBase context, int timeout)
        {
            return new SessionStateStoreData(new SessionStateItemCollection(), SessionStateUtility.GetSessionStaticObjects(context.ApplicationInstance.Context), timeout);
        }

        public override async Task CreateUninitializedItemAsync(HttpContextBase context, string id, int timeout, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<GetItemResult> GetItemAsync(HttpContextBase context, string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<GetItemResult> GetItemExclusiveAsync(HttpContextBase context, string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task ReleaseItemExclusiveAsync(HttpContextBase context, string id, object lockId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task RemoveItemAsync(HttpContextBase context, string id, object lockId, SessionStateStoreData item, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task ResetItemTimeoutAsync(HttpContextBase context, string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task SetAndReleaseItemExclusiveAsync(HttpContextBase context, string id, SessionStateStoreData item, object lockId, bool newItem, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            throw new NotImplementedException();
        }

        public override Task EndRequestAsync(HttpContextBase context)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

    }

}
