using Autofac;

using Cogito.Autofac;

using Microsoft.Azure.Cosmos.Table;

namespace AspNetStateService.Azure.Storage.Table
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterModule<AspNetStateService.Core.AssemblyModule>();
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.Register(i => i.Resolve<ICloudStorageAccountProvider>().GetStorageAccount()).Named<CloudStorageAccount>(StateObjectTableDataStore.TypeNameKey).SingleInstance();
            builder.Register(i => i.Resolve<ICloudTableClientProvider>().GetTableClient()).Named<CloudTableClient>(StateObjectTableDataStore.TypeNameKey).SingleInstance();
        }

    }

}
