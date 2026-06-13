using System;
using System.Collections.Generic;

namespace ClinicaAI.Core.Entities
{
    public class UserProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string StateRegion { get; set; } = string.Empty;
        public double Height { get; set; } // in cm
        public double Weight { get; set; } // in kg
        public string BloodGroup { get; set; } = string.Empty;
        public string KnownConditions { get; set; } = string.Empty; // comma-separated or text
        public string Allergies { get; set; } = string.Empty; // comma-separated or text
        public string CurrentMedications { get; set; } = string.Empty; // comma-separated or text
        public string MedicalHistory { get; set; } = string.Empty; // text summary
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User User { get; set; } = null!;
        public ICollection<MedicalHistoryEntry> HistoryEntries { get; set; } = new List<MedicalHistoryEntry>();
        public ICollection<UploadedReport> UploadedReports { get; set; } = new List<UploadedReport>();
        public ICollection<MemoryStore> MemoryStores { get; set; } = new List<MemoryStore>();
    }
}
