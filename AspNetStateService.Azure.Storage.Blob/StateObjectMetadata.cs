using System;

namespace AspNetStateService.Azure.Storage.Blob
{

    public struct StateObjectMetadata
    {

        /// <summary>
        /// ExtraFlags set on the state object.
        /// </summary>
        public uint? ExtraFlags { get; set; }

        /// <summary>
        /// Timeout of the state object.
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Last time the state object was modified.
        /// </summary>
        public DateTime? Altered { get; set; }

        /// <summary>
        /// Current lock cookie value.
        /// </summary>
        public uint? LockCookie { get; set; }

        /// <summary>
        /// Current lock acquired time.
        /// </summary>
        public DateTime? LockTime { get; set; }

    }

}
