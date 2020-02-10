using Autofac;

using Azure.Storage.Blobs;

using Cogito.Autofac;

namespace AspNetStateService.Azure.Storage.Blob
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterModule<AspNetStateService.Core.AssemblyModule>();
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.Register(i => i.Resolve<IBlobContainerClientProvider>().GetBlobClient()).Named<BlobContainerClient>(StateObjectBlobDataStore.TypeNameKey).SingleInstance();
        }

    }

}
