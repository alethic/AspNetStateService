using System;

using Microsoft.Azure.Cosmos.Table;

namespace AspNetStateService.Azure.Cosmos.Table
{

    /// <summary>
    /// Table entity of a state object.
    /// </summary>
    public class StateObjectEntity : TableEntity
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
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <param name="id"></param>
        public StateObjectEntity(string partitionKey, string rowKey, string id)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
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
        public int? ExtraFlags { get; set; }

        /// <summary>
        /// Timeout of the state object.
        /// </summary>
        public long? Timeout { get; set; }

        /// <summary>
        /// Last time the state object was modified.
        /// </summary>
        public DateTime? Altered { get; set; }

        /// <summary>
        /// Current lock cookie value.
        /// </summary>
        public int? LockCookie { get; set; }

        /// <summary>
        /// Current lock acquired time.
        /// </summary>
        public DateTime? LockTime { get; set; }

    }

}
