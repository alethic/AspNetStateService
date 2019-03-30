using System;

namespace AspNetStateService.Fabric.Client
{

    public class FabricPartitionResolverException : Exception
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="message"></param>
        public FabricPartitionResolverException(string message) :
            base(message)
        {

        }

    }

}
