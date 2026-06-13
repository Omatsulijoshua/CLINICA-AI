using System;
using System.Collections.Generic;

namespace ClinicaAI.Core.Entities
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ConversationId { get; set; }
        public string Sender { get; set; } = string.Empty; // "User" or "AI"
        public string Content { get; set; } = string.Empty;
        public string? StructuredResponseJson { get; set; } // Detailed JSON output (disclaimer, causes, risk, etc.)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Conversation Conversation { get; set; } = null!;
        public ICollection<SourceReference> SourceReferences { get; set; } = new List<SourceReference>();
        public ICollection<VideoReference> VideoReferences { get; set; } = new List<VideoReference>();
    }
}
