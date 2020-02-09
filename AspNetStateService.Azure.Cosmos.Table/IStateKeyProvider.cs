namespace AspNetStateService.Azure.Cosmos.Table
{

    /// <summary>
    /// Generates Partition Key values from state ID values.
    /// </summary>
    public interface IStateKeyProvider
    {

        /// <summary>
        /// Generates the partition ID for the given state ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string GetPartitionKey(string id);

        /// <summary>
        /// Generates the row ID for the given state ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string GetRowKey(string id);

    }

}