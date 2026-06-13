using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicaAI.Core.Interfaces;
using ClinicaAI.Core.Models;

namespace ClinicaAI.Infrastructure.Agents
{
    public class SymptomAgent : IMedicalAgent
    {
        public string Name => "Symptom Agent";

        public Task ExecuteAsync(AgentExecutionContext context)
        {
            if (context.Response.IsEmergency)
            {
                return Task.CompletedTask; // Bypass if emergency agent already escalated to emergency
            }

            var text = context.UserInput.ToLowerInvariant();
            
            // Check if user is reporting symptoms
            bool isSymptomQuery = text.Contains("fever") || text.Contains("headache") || text.Contains("cough") || 
                                  text.Contains("pain") || text.Contains("rash") || text.Contains("sore throat") ||
                                  text.Contains("fatigue") || text.Contains("vomiting") || text.Contains("nausea") ||
                                  text.Contains("tick") || text.Contains("bite") || text.Contains("lyme");

            if (!isSymptomQuery)
            {
                return Task.CompletedTask;
            }

            // Ask follow-up questions if they haven't provided details
            bool hasDuration = text.Contains("day") || text.Contains("week") || text.Contains("month") || text.Contains("since") || text.Contains("hour");
            bool hasSeverity = text.Contains("severe") || text.Contains("mild") || text.Contains("moderate") || text.Contains("high") || text.Contains("low");
            
            if (!hasDuration || !hasSeverity)
            {
                context.Response.Summary = "It looks like you are describing symptoms. To provide the best medical education, could you share a bit more detail?";
                context.Response.RecommendedNextSteps.Add("Please specify the duration of these symptoms (e.g., 2 days, 1 week).");
                context.Response.RecommendedNextSteps.Add("Please specify the severity (e.g., mild, moderate, severe) and if they are getting better or worse.");
                context.Response.RecommendedNextSteps.Add("Are you experiencing any other associated symptoms (e.g., chills, nausea, joint pain)?");
            }

            // Determine Country and Region
            string country = context.Country;
            string region = context.StateRegion;

            // Compute differential diagnosis based on geographic location
            var causes = new List<PossibleCause>();

            // Lyme Disease check (independent of fever)
            if (country.Equals("USA", StringComparison.OrdinalIgnoreCase) && 
                (text.Contains("tick") || text.Contains("lyme") || (text.Contains("rash") && (text.Contains("woods") || text.Contains("forest") || region.Contains("New York") || region.Contains("Connecticut") || region.Contains("Northeast")))))
            {
                causes.Add(new PossibleCause
                {
                    Name = "Lyme Disease",
                    ConfidenceScore = 0.80,
                    Description = "A tick-borne bacterial infection prevalent in the northeastern US, often presenting with an erythema migrans ('bulls-eye') rash, fever, and joint aches."
                });
            }

            if (text.Contains("fever") || text.Contains("chills") || text.Contains("sweating"))
            {
                if (country.Equals("Nigeria", StringComparison.OrdinalIgnoreCase))
                {
                    causes.Add(new PossibleCause
                    {
                        Name = "Malaria",
                        ConfidenceScore = 0.75,
                        Description = "A mosquito-borne infectious disease prevalent in Nigeria, characterized by cycles of high fever, chills, and sweating."
                    });
                    causes.Add(new PossibleCause
                    {
                        Name = "Typhoid Fever",
                        ConfidenceScore = 0.55,
                        Description = "A bacterial infection spread through contaminated food or water, causing prolonged high fever, headache, fatigue, and abdominal symptoms."
                    });
                    causes.Add(new PossibleCause
                    {
                        Name = "Dengue Fever",
                        ConfidenceScore = 0.40,
                        Description = "A viral infection transmitted by Aedes mosquitoes, causing sudden high fever, severe headache, and joint/muscle pain."
                    });
                }
                else if (country.Equals("UK", StringComparison.OrdinalIgnoreCase) || country.Equals("United Kingdom", StringComparison.OrdinalIgnoreCase))
                {
                    causes.Add(new PossibleCause
                    {
                        Name = "Influenza (Flu)",
                        ConfidenceScore = 0.70,
                        Description = "A common viral respiratory infection in the UK, especially during winter seasons, characterized by sudden fever, muscle aches, and dry cough."
                    });
                    causes.Add(new PossibleCause
                    {
                        Name = "COVID-19",
                        ConfidenceScore = 0.65,
                        Description = "A highly contagious viral infection that presents with fever, cough, fatigue, and loss of taste or smell."
                    });
                    causes.Add(new PossibleCause
                    {
                        Name = "Respiratory Syncytial Virus (RSV)",
                        ConfidenceScore = 0.45,
                        Description = "A common respiratory virus causing mild, cold-like symptoms, but can lead to bronchiolitis in children and older adults."
                    });
                }
                else // Default USA/Global (excluding Lyme check which is handled above)
                {
                    causes.Add(new PossibleCause
                    {
                        Name = "Influenza (Flu)",
                        ConfidenceScore = 0.65,
                        Description = "An acute viral respiratory disease, occurring in seasonal epidemics in the USA."
                    });
                    causes.Add(new PossibleCause
                    {
                        Name = "COVID-19",
                        ConfidenceScore = 0.60,
                        Description = "A respiratory illness caused by the SARS-CoV-2 virus, present globally with variable symptom presentations."
                    });
                }
            }

            if (text.Contains("cough") || text.Contains("sore throat"))
            {
                causes.Add(new PossibleCause
                {
                    Name = "Acute Bronchitis",
                    ConfidenceScore = 0.70,
                    Description = "Inflammation of the bronchial tubes, usually viral, causing a persistent cough and mild chest soreness."
                });
                causes.Add(new PossibleCause
                {
                    Name = "Streptococcal Pharyngitis (Strep Throat)",
                    ConfidenceScore = 0.50,
                    Description = "A bacterial infection of the throat, causing sudden sore throat, pain swallowing, and fever without a cough."
                });
            }

            if (text.Contains("rash") || text.Contains("itchy skin"))
            {
                causes.Add(new PossibleCause
                {
                    Name = "Contact Dermatitis",
                    ConfidenceScore = 0.60,
                    Description = "An allergic reaction or irritation resulting from direct skin contact with a trigger substance (like poison ivy or soaps)."
                });
                causes.Add(new PossibleCause
                {
                    Name = "Eczema (Atopic Dermatitis)",
                    ConfidenceScore = 0.50,
                    Description = "A chronic inflammatory skin condition causing dry, red, itchy patches, commonly flares up due to environmental triggers."
                });
            }

            // Assign causes
            foreach (var cause in causes)
            {
                context.Response.PossibleCauses.Add(cause);
            }

            // Update Risk Level based on symptoms
            if (text.Contains("stiff neck") && text.Contains("fever"))
            {
                context.Response.RiskLevel = "High";
                context.Response.Summary = "WARNING: Fever accompanied by a stiff neck can be consistent with Meningitis, which is a medical emergency.";
                context.Response.RecommendedNextSteps.Insert(0, "Seek urgent medical attention at an emergency clinic to rule out Meningitis.");
            }
            else if (context.Response.PossibleCauses.Count > 0)
            {
                context.Response.RiskLevel = "Medium";
                context.Response.Summary = $"Based on your reports, the symptoms could suggest {context.Response.PossibleCauses[0].Name} or other viral/bacterial etiologies, adjusted for your region ({country}).";
            }
            else
            {
                context.Response.RiskLevel = "Low";
                context.Response.Summary = "Your symptoms appear mild, but monitoring is recommended.";
            }

            // Related conditions
            foreach (var c in causes)
            {
                context.Response.RelatedConditions.Add(c.Name);
            }
            context.Response.RelatedConditions.Add("Viral Upper Respiratory Infection");

            // Recommended next steps
            context.Response.RecommendedNextSteps.Add("Monitor your temperature twice daily.");
            context.Response.RecommendedNextSteps.Add("Stay well-hydrated and rest.");
            context.Response.RecommendedNextSteps.Add("Consult a healthcare provider if symptoms worsen or persist past 3-5 days.");

            return Task.CompletedTask;
        }
    }
}
