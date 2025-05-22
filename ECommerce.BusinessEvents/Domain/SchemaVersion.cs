using System;

namespace ModularMonolith.Domain.BusinessEvents
{
    public class SchemaVersion
    {
        public string EntityType { get; set; }
        public int Version { get; set; }
        public string SchemaDefinition { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
