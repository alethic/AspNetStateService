namespace AspNetStateService.Interfaces
{

    /// <summary>
    /// Describes a response from a <see cref="IStateObject"/> that contains response data.
    /// </summary>
    public class DataResponse : Response
    {

        /// <summary>
        /// Contents of the data.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// The action flags represent optional data that a state server implementation returns to a client. A value of "0" means no special action by the client is necessary for the session state data. A value of "1" means the client MUST perform extra initialization work for the session.
        /// </summary>
        public uint? ActionFlags { get; set; }


    }

}
