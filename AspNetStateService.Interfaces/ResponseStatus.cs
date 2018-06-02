namespace AspNetStateService.Interfaces
{

    /// <summary>
    /// Describes the possible statuses of a state object interaction.
    /// </summary>
    public enum ResponseStatus
    {

        /// <summary>
        /// The operation was successful.
        /// </summary>
        Ok,

        /// <summary>
        /// The request was invalid in some way.
        /// </summary>
        BadRequest,

        /// <summary>
        /// The data was not found.
        /// </summary>
        NotFound,

        /// <summary>
        /// The data was found but is currently locked.
        /// </summary>
        Locked

    }

}
