using System.Runtime.CompilerServices;

using Castle.Core.Internal;

using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;

[assembly: InternalsVisibleTo(InternalsVisible.ToDynamicProxyGenAssembly2)]
[assembly: FabricTransportServiceRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2, RemotingClientVersion = RemotingClientVersion.V2)]
[assembly: FabricTransportActorRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2, RemotingClientVersion = RemotingClientVersion.V2)]
