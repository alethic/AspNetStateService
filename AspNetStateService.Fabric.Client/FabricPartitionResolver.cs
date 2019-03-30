using System;
using System.Threading;
using System.Web;

using Microsoft.ServiceFabric.Services.Client;

namespace AspNetStateService.Fabric.Client
{

    public class FabricPartitionResolver : IPartitionResolver
    {

        ServicePartitionResolver resolver;
        string fabricServiceName;

        /// <summary>
        /// Initializes the partition resolver.
        /// </summary>
        public void Initialize()
        {
            //this.resolver = new ServicePartitionResolver();
            //this.fabricServiceName = fabricServiceName?.TrimOrNull() ?? "fabric:/AspNetStateService/StateWebService";
        }

        /// <summary>
        /// Resolves the appropriate service partition for the endpoint.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string ResolvePartition(object key)
        {
            var partition = resolver.ResolveAsync(
                    new Uri(fabricServiceName),
                    ServicePartitionKey.Singleton,
                    CancellationToken.None)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            if (partition == null)
                throw new FabricPartitionResolverException("Unable to resolve state service partition.");

            var endpoint = partition.GetEndpoint();
            if (endpoint == null)
                throw new FabricPartitionResolverException("Cannot discover a state service endpoint.");

            return endpoint.Address;
        }

    }

}
