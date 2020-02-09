using Microsoft.Azure.Cosmos.Table;

namespace AspNetStateService.Azure.Cosmos.Table
{

    /// <summary>
    /// Provides <see cref="CloudTableClient"/> instances for the backend.
    /// </summary>
    public interface ICloudTableClientProvider
    {

        /// <summary>
        /// Gets the cloud table client.
        /// </summary>
        /// <returns></returns>
        CloudTableClient GetTableClient();

    }

}