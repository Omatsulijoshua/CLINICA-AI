using System;

namespace ClinicaAI.Core.Entities
{
    public class UploadedFile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ConversationId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty; // URL or cloud key
        public long FileSize { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public string ExtractedText { get; set; } = string.Empty; // Parsed OCR/text content
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Conversation Conversation { get; set; } = null!;
        public UploadedReport? UploadedReport { get; set; }
    }
}
