using System;

namespace ClinicaAI.Core.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? UserId { get; set; } // Can be null if system event or unauthenticated access
        public string Action { get; set; } = string.Empty; // e.g. "Read Medical Record", "Update Profile"
        public string IpAddress { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User? User { get; set; }
    }
}
