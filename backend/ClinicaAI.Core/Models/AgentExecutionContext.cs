using System;
using System.Collections.Generic;
using ClinicaAI.Core.Entities;

namespace ClinicaAI.Core.Models
{
    public class AgentExecutionContext
    {
        public string UserInput { get; set; } = string.Empty;
        public UserProfile? Profile { get; set; }
        public List<Message> ChatHistory { get; set; } = new();
        public List<UploadedFile> UploadedFiles { get; set; } = new();
        public ClinicaResponse Response { get; set; } = new();
        public string ExtractedReportText { get; set; } = string.Empty;
        public string ImageAnalysisDescription { get; set; } = string.Empty;
        
        // Metadata / settings
        public string Country { get; set; } = "USA";
        public string StateRegion { get; set; } = string.Empty;
        public string Climate { get; set; } = "Temperate";
        public string Season { get; set; } = "Summer";
        
        // Temp/internal storage to pass information between agents
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
