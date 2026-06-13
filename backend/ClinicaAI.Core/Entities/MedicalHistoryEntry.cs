using System;

namespace ClinicaAI.Core.Entities
{
    public class MedicalHistoryEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProfileId { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Active, Resolved, Managed, etc.
        public DateTime? DiagnosedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public UserProfile Profile { get; set; } = null!;
    }
}
