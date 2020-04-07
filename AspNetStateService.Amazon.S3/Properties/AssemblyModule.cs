using Amazon.S3;

using Autofac;

using Cogito.Autofac;

namespace AspNetStateService.Amazon.S3
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterModule<AspNetStateService.Core.AssemblyModule>();
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.Register(i => i.Resolve<IS3ClientProvider>().CreateClient()).Named<IAmazonS3>(StateObjectS3DataStore.TypeNameKey).SingleInstance();
        }

    }

}
