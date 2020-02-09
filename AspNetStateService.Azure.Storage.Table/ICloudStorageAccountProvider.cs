using Microsoft.Azure.Cosmos.Table;

namespace AspNetStateService.Azure.Storage.Table
{

    /// <summary>
    /// Provides <see cref="CloudStorageAccount"/> instances for the backend.
    /// </summary>
    public interface ICloudStorageAccountProvider
    {

        /// <summary>
        /// Gets the cloud storage account.
        /// </summary>
        /// <returns></returns>
        CloudStorageAccount GetStorageAccount();

    }

}