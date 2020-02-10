using Azure.Storage.Blobs;

namespace AspNetStateService.Azure.Storage.Blob
{

    /// <summary>
    /// Provides <see cref="BlobContainerClient"/> instances for the backend.
    /// </summary>
    public interface IBlobContainerClientProvider
    {

        /// <summary>
        /// Gets the cloud blob client.
        /// </summary>
        /// <returns></returns>
        BlobContainerClient GetBlobClient();

    }

}