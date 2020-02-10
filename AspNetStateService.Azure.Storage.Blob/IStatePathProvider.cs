namespace AspNetStateService.Azure.Storage.Blob
{

    /// <summary>
    /// Generates blob container path values from state ID values.
    /// </summary>
    public interface IStatePathProvider
    {

        /// <summary>
        /// Generates the path for the given state ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string GetPath(string id);

    }

}