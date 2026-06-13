using System;
using System.Collections.Generic;

namespace ClinicaAI.Core.Entities
{
    public class Conversation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Title { get; set; } = "New Conversation";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User User { get; set; } = null!;
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<UploadedFile> UploadedFiles { get; set; } = new List<UploadedFile>();
    }
}
