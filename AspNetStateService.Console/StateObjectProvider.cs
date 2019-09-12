using System;
using System.Threading.Tasks;

using AspNetStateService.Core;
using AspNetStateService.Interfaces;

using Cogito.Autofac;

using Serilog;

namespace AspNetStateService.Console
{

    [RegisterAs(typeof(IStateObjectProvider))]
    [RegisterSingleInstance]
    public class StateObjectProvider : IStateObjectProvider
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

        public Task<IStateObject> GetStateObjectAsync(string id)
        {
            return Task.FromResult<IStateObject>(new StateObject(id, store, logger));
        }

    }

}
