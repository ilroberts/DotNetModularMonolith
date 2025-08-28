namespace ECommerce.BusinessEvents.Domain
{
    /// <summary>
    /// Configuration for extracting metadata fields from JSON schema definitions.
    /// Specifies which fields should be stored in the BusinessEventMetadata table.
    /// </summary>
    public class MetadataExtractionConfig
    {
        /// <summary>
        /// List of field paths that should be extracted as metadata.
        /// Supports nested paths using dot notation (e.g., "Address.PostCode").
        /// </summary>
        public List<string> FieldsToExtract { get; set; } = new();

        /// <summary>
        /// Maps field paths to their expected data types for proper storage.
        /// Valid types: 'string', 'number', 'boolean', 'date'
        /// </summary>
        public Dictionary<string, string> FieldTypes { get; set; } = new();

        /// <summary>
        /// Indicates whether any metadata fields are configured for extraction.
        /// </summary>
        public bool HasMetadata => FieldsToExtract.Any();
    }
}
