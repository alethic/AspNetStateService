using Cogito.Extensions.Options.ConfigurationExtensions.Autofac;

namespace AspNetStateService.Core.Options
{

    [RegisterOptions("Seq")]
    public class SeqOptions
    {

        public string ServerUrl { get; set; }

        public string ApiKey { get; set; }

    }

}
