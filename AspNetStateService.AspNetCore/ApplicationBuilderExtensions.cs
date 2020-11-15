using System.Collections.Generic;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetStateService.AspNetCore
{

    /// <summary>
    /// Builds the ASP.NET Core state pipeline.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {

        /// <summary>
        /// Adds any dynamic pipeline services.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseDynamicPipeline(this IApplicationBuilder builder)
        {
            var configurators = builder.ApplicationServices.GetRequiredService<IEnumerable<IApplicationBuilderConfigurator>>();

            foreach (var configurator in configurators)
                configurator.Configure(builder);

            return builder;
        }

    }

}
