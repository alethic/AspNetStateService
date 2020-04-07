namespace AspNetStateService.Amazon.S3
{

    /// <summary>
    /// Generates Amazon S3 key values from state ID values.
    /// </summary>
    public interface IStateKeyProvider
    {

        /// <summary>
        /// Generates the path for the given state ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string GetKey(string id);

    }

}