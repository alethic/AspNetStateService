using MongoDB.Driver;

namespace AspNetStateService.MongoDB
{

    /// <summary>
    /// Provides <see cref="MongoClient"/> instances for the backend.
    /// </summary>
    public interface IMongoClientProvider
    {

        /// <summary>
        /// Gets the MongoDB client.
        /// </summary>
        /// <returns></returns>
        IMongoClient CreateClient();

    }

}
