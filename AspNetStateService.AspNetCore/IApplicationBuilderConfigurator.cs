using Microsoft.AspNetCore.Builder;

namespace AspNetStateService.AspNetCore
{

    /// <summary>
    /// Builds the ASP.NET Core state pipeline.
    /// </summary>
    public interface IApplicationBuilderConfigurator
    {

        /// <summary>
        /// Adds any dynamic pipeline services.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        void Configure(IApplicationBuilder builder);

    }

}
