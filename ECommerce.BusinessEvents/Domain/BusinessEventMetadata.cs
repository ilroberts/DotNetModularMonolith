using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.BusinessEvents.Domain
{
    public class BusinessEventMetadata
    {
        [Key, Column(Order = 0)]
        public Guid EventId { get; set; }

        [Key, Column(Order = 1)]
        [MaxLength(100)]
        public string MetadataKey { get; set; }

        [Required]
        [MaxLength(200)]
        public string EntityType { get; set; }

        [Required]
        [MaxLength(200)]
        public string EntityId { get; set; }

        [Required]
        [MaxLength(500)]
        public string MetadataValue { get; set; }

        [Required]
        [MaxLength(50)]
        public string DataType { get; set; } // 'string', 'number', 'boolean', 'date'

        public BusinessEvent BusinessEvent { get; set; }
    }
}

