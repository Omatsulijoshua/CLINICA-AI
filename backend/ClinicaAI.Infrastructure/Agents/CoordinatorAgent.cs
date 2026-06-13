using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ClinicaAI.Core.Interfaces;
using ClinicaAI.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicaAI.Infrastructure.Agents
{
    public class CoordinatorAgent
    {
        private readonly IServiceProvider _serviceProvider;

        public CoordinatorAgent(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<ClinicaResponse> ProcessQueryAsync(string userInput, Guid? userId, string reportText = "", string imageDescription = "")
        {
            // 1. Setup execution context
            var context = new AgentExecutionContext
            {
                UserInput = userInput,
                ExtractedReportText = reportText,
                ImageAnalysisDescription = imageDescription
            };

            if (userId.HasValue)
            {
                context.Metadata["UserId"] = userId.Value;
            }

            // 2. Build Agent pipeline
            var pipeline = new List<IMedicalAgent>
            {
                _serviceProvider.GetRequiredService<MemoryAgent>(),
                _serviceProvider.GetRequiredService<EmergencyAgent>(),
                _serviceProvider.GetRequiredService<SymptomAgent>(),
                _serviceProvider.GetRequiredService<LabAgent>(),
                _serviceProvider.GetRequiredService<DrugAgent>(),
                _serviceProvider.GetRequiredService<NutritionAgent>(),
                _serviceProvider.GetRequiredService<ResearchAgent>(),
                _serviceProvider.GetRequiredService<VideoAgent>()
            };

            // 3. Execute agents sequentially to build context and compile information
            foreach (var agent in pipeline)
            {
                try
                {
                    await agent.ExecuteAsync(context);
                }
                catch (Exception ex)
                {
                    // HIPAA-inspired: Log internal system failures safely
                    Console.WriteLine($"[Agent Failure] {agent.Name} failed: {ex.Message}");
                }
            }

            // 4. Force mandatory disclaimer & final validation
            var response = context.Response;
            response.MedicalDisclaimer = "Clinica AI is an educational assistant and not a licensed physician. Consult a qualified healthcare professional for diagnosis and treatment.";

            // If summary is still blank, write a general medical explanation
            if (string.IsNullOrWhiteSpace(response.Summary))
            {
                response.Summary = "I am Clinica AI, your digital educational health assistant. How can I help you understand symptoms, medications, or lab reports today?";
            }

            // Calculate aggregate confidence score
            if (response.IsEmergency)
            {
                response.ConfidenceScore = 0.99;
            }
            else if (response.ConfidenceScore == 0)
            {
                response.ConfidenceScore = 0.70;
            }

            return response;
        }
    }
}
