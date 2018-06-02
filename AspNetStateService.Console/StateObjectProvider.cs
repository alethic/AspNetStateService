using System;
using System.Threading.Tasks;

using AspNetStateService.Core;
using AspNetStateService.Interfaces;

using FileAndServe.Autofac;

namespace AspNetStateService.Console
{

    [RegisterAs(typeof(IStateObjectProvider))]
    [RegisterSingleInstance]
    public class StateObjectProvider : IStateObjectProvider
    {

        readonly IStateObjectDataStore store;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="store"></param>
        public StateObjectProvider(IStateObjectDataStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public Task<IStateObject> GetStateObjectAsync(string id)
        {
            return Task.FromResult<IStateObject>(new StateObject(id, store));
        }

    }

}
