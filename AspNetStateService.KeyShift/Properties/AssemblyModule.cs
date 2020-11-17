using Alethic.Kademlia;
using Alethic.Kademlia.InMemory;
using Alethic.Kademlia.Network.Udp;
using Alethic.Kademlia.Protobuf;
using Alethic.KeyShift;
using Alethic.KeyShift.AspNetCore;
using Alethic.KeyShift.InMemory;
using Alethic.KeyShift.Kademlia;

using Autofac;

using Cogito.Autofac;
using Cogito.Autofac.DependencyInjection;
using Cogito.Extensions.Options.Autofac;

using Microsoft.Extensions.DependencyInjection;

namespace AspNetStateService.KeyShift
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterModule<AspNetStateService.Core.AssemblyModule>();
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            RegisterKademlia(builder, 42424);
            RegisterKeyShift(builder);
        }

        static void RegisterKademlia(ContainerBuilder builder, ulong networkId)
        {
            builder.RegisterType<KProtobufMessageFormat<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KRefresher<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KConnector<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KInvoker<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KInvokerPolicy<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KRequestHandler<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KFixedTableRouter<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KLookup<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KValueAccessor<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KInMemoryStore<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KInMemoryPublisher<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KHost<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KUdpProtocol<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KUdpMulticastDiscovery<KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KHostedService>().AsSelf().SingleInstance();
            builder.Configure<KHostOptions<KNodeId256>>(o => o.NetworkId = networkId);
            builder.Configure<KFixedTableRouterOptions>(o => { });
        }

        static void RegisterKeyShift(ContainerBuilder builder)
        {
            builder.RegisterType<KsHostHttpClientFactory<string>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KsHostClientProvider<string>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KsHost<string>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KsInMemoryStore<string>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KsKademliaHashTable<string, KNodeId256>>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KsKademliaSha256StringHasher>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<KsKademliaSha1StringHasher>().AsImplementedInterfaces().SingleInstance();
            builder.Populate(s => s.AddKeyShiftMiddleware());
        }

    }

}
