using System;

using AspNetStateService.Core.Options;

using Cogito.Autofac;
using Cogito.Serilog;

using Microsoft.Extensions.Options;

using Serilog;

namespace AspNetStateService.Core
{

    [RegisterAs(typeof(ILoggerConfigurator))]
    class SerilogSeqConfigurator : ILoggerConfigurator
    {

        readonly IOptions<SeqOptions> options;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="options"></param>
        public SerilogSeqConfigurator(IOptions<SeqOptions> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public LoggerConfiguration Apply(LoggerConfiguration configuration)
        {
            if (options.Value != null && !string.IsNullOrEmpty(options.Value.ServerUrl) && !string.IsNullOrEmpty(options.Value.ApiKey))
                return configuration.WriteTo.Seq(options.Value.ServerUrl, apiKey: options.Value.ApiKey);
            
            return configuration;
        }

    }

}
