using System;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Core;
using AspNetStateService.Interfaces;

using Cogito.Autofac;

using Serilog;

namespace AspNetStateService.AspNetCore
{

    /// <summary>
    /// Provides objects for managing individual session state against a shared store.
    /// </summary>
    [RegisterAs(typeof(IStateObjectProvider))]
    class StateObjectProvider : IStateObjectProvider
    {

        readonly IStateObjectDataStore store;
        readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="logger"></param>
        public StateObjectProvider(IStateObjectDataStore store, ILogger logger)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Starts the provider.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.Verbose("StartAsync()");
            await store.StartAsync(cancellationToken);
        }

        /// <summary>
        /// Stops the provider.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Verbose("StopAsync()");
            await store.StopAsync(cancellationToken);
        }

        /// <summary>
        /// Gets the <see cref="IStateObject"/> for the particular ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<IStateObject> GetStateObjectAsync(string id, CancellationToken cancellationToken)
        {
            return Task.FromResult<IStateObject>(new StateObject(id, store, logger));
        }

    }

}
