using System;

using Amazon;
using Amazon.S3;

using Cogito.Autofac;

using Microsoft.Extensions.Options;

namespace AspNetStateService.Amazon.S3
{

    /// <summary>
    /// Default provider of <see cref="IAmazonS3"/> instances.
    /// </summary>
    [RegisterAs(typeof(IS3ClientProvider))]
    public class DefaultS3ClientProvider : IS3ClientProvider
    {

        readonly IOptions<StateObjectS3DataStoreOptions> options;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="account"></param>
        public DefaultS3ClientProvider(IOptions<StateObjectS3DataStoreOptions> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IAmazonS3 CreateClient()
        {
            if (options.Value == null ||
                options.Value.AwsAccessKeyId == null ||
                options.Value.AwsSecretAccessKey == null)
                throw new InvalidOperationException("Missing configuration for Amazon S3.");

            if (options.Value.RegionEndpointName == null)
                throw new InvalidOperationException("Missing region endpoint for Amazon S3.");

            var endpoint = RegionEndpoint.GetBySystemName(options.Value.RegionEndpointName);
            if (endpoint == null)
                throw new InvalidOperationException("Invalid region endpoint for Amazon S3.");

            return new AmazonS3Client(options.Value.AwsAccessKeyId, options.Value.AwsSecretAccessKey, endpoint);
        }

    }

}
