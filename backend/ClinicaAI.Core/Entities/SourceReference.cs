using System;

namespace ClinicaAI.Core.Entities
{
    public class SourceReference
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MessageId { get; set; }
        public string SourceName { get; set; } = string.Empty; // PubMed, WHO, CDC, NIH, etc.
        public string Title { get; set; } = string.Empty;
        public string URL { get; set; } = string.Empty;
        public string Snippet { get; set; } = string.Empty;

        // Navigation Properties
        public Message Message { get; set; } = null!;
    }
}
