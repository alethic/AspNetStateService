using Cogito.Autofac;
using Cogito.Serilog;

using Serilog;

namespace AspNetStateService.Core
{

    [RegisterAs(typeof(ILoggerConfigurator))]
    class SerilogConfigurator : ILoggerConfigurator
    {

        public LoggerConfiguration Apply(LoggerConfiguration configuration)
        {
            return configuration
#if DEBUG
                .MinimumLevel.Verbose()
#endif
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithMachineName()
                .Enrich.WithMemoryUsage()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName();
        }

    }

}
