using Autofac;

using Cogito.Autofac;

using Harmony;

namespace AspNetStateService.AspNetCore.Kestrel
{

    public class AssemblyModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.RegisterCallback(i => Patch());
        }

        /// <summary>
        /// Invokes the Harmony patches.
        /// </summary>
        void Patch()
        {
            var h = HarmonyInstance.Create(typeof(AssemblyModule).Assembly.GetName().Name);
            h.PatchAll(typeof(AssemblyModule).Assembly);
        }

    }

}
