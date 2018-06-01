using System;

namespace AspNetStateService.Service.Interfaces
{

    public class Response
    {

        /// <summary>
        /// Status of the response.
        /// </summary>
        public ResponseStatus Status { get; set; }

        /// <summary>
        /// Date of data.
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Identifier of the lock.
        /// </summary>
        public uint? LockCookie { get; set; }

        /// <summary>
        /// Date at which the lock was last renewed.
        /// </summary>
        public DateTime? LockCreate { get; set; }

        /// <summary>
        /// Current age of the lock.
        /// </summary>
        public TimeSpan? LockAge { get; set; }

    }

}