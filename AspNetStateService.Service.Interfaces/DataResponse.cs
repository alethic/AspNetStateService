namespace AspNetStateService.Service.Interfaces
{

    public class DataResponse : Response
    {

        /// <summary>
        /// Contents of the data.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Flag to indicate whether client should act.
        /// </summary>
        public uint ActionFlag { get; set; }


    }

}