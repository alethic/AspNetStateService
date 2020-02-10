using System;
using System.Threading;
using System.Threading.Tasks;

using AspNetStateService.Core;
using AspNetStateService.Interfaces;

using Cogito.Autofac;
using Cogito.Threading;

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
        readonly AsyncLock sync = new AsyncLock();

        bool init = true;

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

        async Task InitAsync(CancellationToken cancellationToken)
        {
            await store.InitAsync(cancellationToken);
            init = false;
        }

        async Task EnsureInitAsync(CancellationToken cancellationToken)
        {
            if (init)
                using (sync.LockAsync())
                    if (init)
                        await InitAsync(cancellationToken);
        }

        public async Task<IStateObject> GetStateObjectAsync(string id, CancellationToken cancellationToken)
        {
            await EnsureInitAsync(cancellationToken);
            return new StateObject(id, store, logger);
        }

    }

}
