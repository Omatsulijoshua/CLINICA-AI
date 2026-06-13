using System;
using System.Collections.Generic;

namespace ClinicaAI.Core.Models
{
    public class PossibleCause
    {
        public string Name { get; set; } = string.Empty;
        public double ConfidenceScore { get; set; } // 0.0 to 1.0 (or percentage)
        public string Description { get; set; } = string.Empty;
    }

    public class MedicalSource
    {
        public string Name { get; set; } = string.Empty; // PubMed, WHO, CDC, NIH, NHS, Mayo Clinic, etc.
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Snippet { get; set; } = string.Empty;
    }

    public class VideoRecommendation
    {
        public string Title { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
    }

    public class ClinicaResponse
    {
        public string Summary { get; set; } = string.Empty;
        public List<PossibleCause> PossibleCauses { get; set; } = new();
        public string RiskLevel { get; set; } = "Low"; // Low, Medium, High, Critical / Emergency
        public List<string> LifestyleRecommendations { get; set; } = new();
        public List<string> NutritionSuggestions { get; set; } = new();
        public List<string> SupplementInformation { get; set; } = new();
        public List<string> MedicationEducation { get; set; } = new();
        public List<MedicalSource> Sources { get; set; } = new();
        public List<VideoRecommendation> EducationalVideos { get; set; } = new();
        public string MedicalDisclaimer { get; set; } = "Clinica AI is an educational assistant and not a licensed physician. Consult a qualified healthcare professional for diagnosis and treatment.";
        public bool IsEmergency { get; set; } = false;
        public string EmergencyGuidance { get; set; } = string.Empty;
        public List<string> RelatedConditions { get; set; } = new();
        public List<string> RecommendedNextSteps { get; set; } = new();
        public double ConfidenceScore { get; set; } // Aggregate score for the result display
    }
}
