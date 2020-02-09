using Autofac;

using Cogito.Autofac;

using Harmony;

namespace AspNetStateService.AspNetCore.Kestrel
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterModule<AspNetStateService.AspNetCore.AssemblyModule>();
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
