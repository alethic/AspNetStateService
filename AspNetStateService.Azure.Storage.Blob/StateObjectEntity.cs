using System;
using System.Text.Json.Serialization;

using AspNetStateService.Azure.Storage.Blob.Converters;

namespace AspNetStateService.Azure.Storage.Blob
{

    /// <summary>
    /// JSON serializable entity of a state object.
    /// </summary>
    public class StateObjectEntity
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public StateObjectEntity()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id"></param>
        public StateObjectEntity(string id)
        {
            Id = id;
        }

        /// <summary>
        /// ID of the state object.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Persisted data.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// ExtraFlags set on the state object.
        /// </summary>
        public uint? ExtraFlags { get; set; }

        /// <summary>
        /// Timeout of the state object.
        /// </summary>
        [JsonConverter(typeof(TimeSpanConverter))]
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
