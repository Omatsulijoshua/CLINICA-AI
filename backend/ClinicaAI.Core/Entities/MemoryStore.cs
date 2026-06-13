using System;

namespace ClinicaAI.Core.Entities
{
    public class MemoryStore
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProfileId { get; set; }
        public string Key { get; set; } = string.Empty; // e.g. "past_diagnoses", "past_symptoms", "dietary_preferences"
        public string Value { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public UserProfile Profile { get; set; } = null!;
    }
}
