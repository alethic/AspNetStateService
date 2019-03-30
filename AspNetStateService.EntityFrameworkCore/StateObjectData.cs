using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetStateService.EntityFrameworkCore
{

    /// <summary>
    /// Data object of a state object.
    /// </summary>
    public class StateObjectData
    {

        /// <summary>
        /// ID of the state object.
        /// </summary>
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual string Id { get; set; }

        /// <summary>
        /// Persisted data.
        /// </summary>
        [Column("Data")]
        public virtual byte[] Data { get; set; }

        /// <summary>
        /// ExtraFlags set on the state object.
        /// </summary>
        [Column("ExtraFlags")]
        public virtual uint? ExtraFlags { get; set; }

        /// <summary>
        /// Timeout of the state object.
        /// </summary>
        [Column("Timeout")]
        public virtual TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Last time the state object was modified.
        /// </summary>
        [Column("Altered")]
        public virtual DateTime? Altered { get; set; }

        /// <summary>
        /// Current lock cookie value.
        /// </summary>
        [Column("LockCookie")]
        public virtual uint? LockCookie { get; set; }

        /// <summary>
        /// Current lock acquired time.
        /// </summary>
        [Column("LockTime")]
        public virtual DateTime? LockTime { get; set; }

    }

}
