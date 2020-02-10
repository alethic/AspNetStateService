using Cogito.Extensions.Options.ConfigurationExtensions.Autofac;

namespace AspNetStateService.AspNetCore
{

    [RegisterOptions("AspNetStateService.AspNetCore")]
    public class StateWebServiceOptions
    {

        /// <summary>
        /// Gets the name of the store to use.
        /// </summary>
        public string Store { get; set; } = "EntityFrameworkCore";

    }

}
