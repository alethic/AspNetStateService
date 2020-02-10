using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetStateService.EntityFrameworkCore
{

    /// <summary>
    /// Represents the state object.
    /// </summary>
    [Table("SessionData")]
    public class SessionDataObject
    {

        /// <summary>
        /// Unique identifier of this entity.
        /// </summary>
        [Key]
        [Column("Id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Persisted data.
        /// </summary>
        [Column("Value")]
        public virtual byte[] Value { get; set; }

    }

}
