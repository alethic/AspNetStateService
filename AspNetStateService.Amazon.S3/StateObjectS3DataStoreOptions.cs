using Cogito.Extensions.Options.ConfigurationExtensions.Autofac;

namespace AspNetStateService.Amazon.S3
{

    [RegisterOptions("AspNetStateService.Amazon.S3")]
    public class StateObjectS3DataStoreOptions
    {

        /// <summary>
        /// Gets or sets the access key.
        /// </summary>
        public string AwsAccessKeyId { get; set; }

        /// <summary>
        /// Gets or sets the access key secret.
        /// </summary>
        public string AwsSecretAccessKey { get; set; }

        /// <summary>
        /// Gets the name of the region endpoint.
        /// </summary>
        public string RegionEndpointName { get; set; }

        /// <summary>
        /// Gets or sets the name of the bucket.
        /// </summary>
        public string BucketName { get; set; }

    }

}
