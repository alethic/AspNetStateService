using Alethic.KeyShift.AspNetCore;

using AspNetStateService.AspNetCore;

using Cogito.Autofac;

using Microsoft.AspNetCore.Builder;

namespace AspNetStateService.KeyShift
{

    /// <summary>
    /// Configures the request pipeline with KeyShift services.
    /// </summary>
    [RegisterAs(typeof(IApplicationBuilderConfigurator))]
    class ApplicationBuilderConfigurator : IApplicationBuilderConfigurator
    {

        public void Configure(IApplicationBuilder builder)
        {
            builder.UseEndpoints(configure =>
            {
                configure.MapKeyShiftHost("/kshost");
                configure.MapKeyShiftKeys("/kskeys");
            });
        }

    }

}
