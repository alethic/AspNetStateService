using System;

namespace AspNetStateService.Interfaces
{

    /// <summary>
    /// Describes a response from a <see cref="IStateObject"/>.
    /// </summary>
    public class Response
    {

        /// <summary>
        /// Status of the response.
        /// </summary>
        public ResponseStatus Status { get; set; }

        /// <summary>
        /// Length of time from now when the data will expire.
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Identifier of the lock.
        /// </summary>
        public uint? LockCookie { get; set; }

        /// <summary>
        /// Date at which the lock was last renewed.
        /// </summary>
        public DateTime? LockTime { get; set; }

        /// <summary>
        /// Current age of the lock.
        /// </summary>
        public TimeSpan? LockAge { get; set; }

    }

}