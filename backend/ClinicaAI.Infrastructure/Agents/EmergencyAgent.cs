using System;
using System.Threading.Tasks;
using ClinicaAI.Core.Interfaces;
using ClinicaAI.Core.Models;

namespace ClinicaAI.Infrastructure.Agents
{
    public class EmergencyAgent : IMedicalAgent
    {
        public string Name => "Emergency Agent";

        public Task ExecuteAsync(AgentExecutionContext context)
        {
            var text = context.UserInput.ToLowerInvariant();
            
            // Check for chest pain / cardiac symptoms
            bool hasChestPain = text.Contains("chest pain") || text.Contains("heart attack") || text.Contains("pressure in chest") || text.Contains("angina");
            
            // Check for stroke symptoms
            bool hasStroke = text.Contains("stroke") || text.Contains("face drooping") || text.Contains("slurred speech") || text.Contains("numbness on one side") || text.Contains("arm weakness");
            
            // Check for breathing issues
            bool hasBreathingIssues = text.Contains("difficulty breathing") || text.Contains("shortness of breath") || text.Contains("cannot breathe") || text.Contains("gasping") || text.Contains("dyspnea");
            
            // Check for severe bleeding
            bool hasSevereBleeding = text.Contains("severe bleeding") || text.Contains("gushing blood") || text.Contains("hemorrhage") || text.Contains("bleeding heavily");

            if (hasChestPain || hasStroke || hasBreathingIssues || hasSevereBleeding)
            {
                context.Response.IsEmergency = true;
                context.Response.RiskLevel = "Critical / Emergency";
                context.Response.ConfidenceScore = 0.99;
                
                var emergencyGuidance = "CRITICAL WARNING: Life-threatening symptoms detected.\n";
                if (hasChestPain)
                {
                    emergencyGuidance += "- Call your local emergency number (e.g., 911 or 999) immediately.\n- Sit down and rest. Do not try to walk or drive yourself to the hospital.\n- If available and not allergic, chew an adult aspirin.";
                }
                else if (hasStroke)
                {
                    emergencyGuidance += "- Think FAST: Face drooping, Arm weakness, Speech difficulty, Time to call emergency services immediately.\n- Record the time when symptoms first appeared.";
                }
                else if (hasBreathingIssues)
                {
                    emergencyGuidance += "- Sit upright to ease breathing. Avoid lying down.\n- Use emergency inhalers or nebulizers if prescribed.\n- Seek immediate emergency care.";
                }
                else if (hasSevereBleeding)
                {
                    emergencyGuidance += "- Apply direct pressure to the wound with a clean cloth.\n- Elevate the bleeding limb above the heart if possible.\n- Seek immediate emergency care.";
                }

                context.Response.EmergencyGuidance = emergencyGuidance;
                context.Response.RecommendedNextSteps.Insert(0, "IMMEDIATE ACTION: Call emergency services (911/999/112) now!");
            }

            return Task.CompletedTask;
        }
    }
}
