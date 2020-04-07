using Amazon.S3;

namespace AspNetStateService.Amazon.S3
{

    /// <summary>
    /// Provides <see cref="IAmazonS3"/> instances for the backend.
    /// </summary>
    public interface IS3ClientProvider
    {

        /// <summary>
        /// Gets the cloud blob client.
        /// </summary>
        /// <returns></returns>
        IAmazonS3 CreateClient();

    }

}