using System;

namespace AspNetStateService.Amazon.S3
{

    class StateObjectData
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="buffer"></param>
        public StateObjectData(StateObjectMetadata metadata, byte[] buffer)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        }

        public StateObjectMetadata Metadata { get; }

        public byte[] Buffer { get; }

    }

}
