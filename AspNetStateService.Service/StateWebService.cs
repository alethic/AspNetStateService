using System;
using System.Threading.Tasks;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using FileAndServe.Autofac;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetStateService.Service
{

    [RegisterAs(typeof(StateWebService))]
    public class StateWebService
    {

        readonly ILifetimeScope parent;
        ILifetimeScope scope;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parent"></param>
        public StateWebService(ILifetimeScope parent)
        {
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        /// <summary>
        /// Registers framework dependencies.
        /// </summary>
        /// <param name="services"></param>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return new AutofacServiceProvider(scope = parent.BeginLifetimeScope(builder => builder.Populate(services)));
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="applicationLifetime"></param>
        public void Configure(IApplicationBuilder app, IApplicationLifetime applicationLifetime)
        {
            applicationLifetime.ApplicationStopped.Register(() => scope.Dispose());
        }

        public Task Get()
        {
            throw new NotImplementedException();
        }

        public Task GetExclusive()
        {
            throw new NotImplementedException();
        }

        public Task Set()
        {
            throw new NotImplementedException();
        }

        public Task ReleaseExclusive()
        {
            throw new NotImplementedException();
        }

        public Task Remove()
        {
            throw new NotImplementedException();
        }

        public Task ResetTimeout()
        {
            throw new NotImplementedException();
        }

    }

}
