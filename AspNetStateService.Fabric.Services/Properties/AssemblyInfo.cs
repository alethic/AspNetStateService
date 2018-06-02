using System.Runtime.CompilerServices;

using Castle.Core.Internal;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;

[assembly: InternalsVisibleTo(InternalsVisible.ToDynamicProxyGenAssembly2)]
[assembly: FabricTransportServiceRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]
