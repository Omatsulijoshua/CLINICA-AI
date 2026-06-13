using System;

namespace ClinicaAI.Core.Entities
{
    public class UploadedReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid FileId { get; set; }
        public Guid ProfileId { get; set; }
        public string ReportType { get; set; } = string.Empty; // CBC, FBC, Urinalysis, Lipid Panel, Kidney Function, Liver Function, Blood Sugar, Hormonal Panel, etc.
        public string Summary { get; set; } = string.Empty;
        public string AbnormalFindings { get; set; } = string.Empty;
        public string FullInterpretationJson { get; set; } = string.Empty; // Detailed interpretation details
        public DateTime InterpretedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public UploadedFile File { get; set; } = null!;
        public UserProfile Profile { get; set; } = null!;
    }
}
