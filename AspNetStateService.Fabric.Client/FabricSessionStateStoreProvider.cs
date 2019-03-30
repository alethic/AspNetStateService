using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

using Microsoft.AspNet.SessionState;

namespace AspNetStateService.Fabric.Client
{

    public class FabricSessionStateStoreProvider : SessionStateStoreProviderAsyncBase
    {

        public override void InitializeRequest(HttpContextBase context)
        {

        }

        public override SessionStateStoreData CreateNewStoreData(HttpContextBase context, int timeout)
        {
            throw new NotImplementedException();
        }

        public override Task CreateUninitializedItemAsync(HttpContextBase context, string id, int timeout, CancellationToken cancellationToken)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

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
