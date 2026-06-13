using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicaAI.Core.Interfaces;
using ClinicaAI.Core.Models;

namespace ClinicaAI.Infrastructure.Agents
{
    public class DrugAgent : IMedicalAgent
    {
        public string Name => "Drug Agent";

        public Task ExecuteAsync(AgentExecutionContext context)
        {
            var text = context.UserInput.ToLowerInvariant();
            var medEducation = new List<string>();
            var supplements = new List<string>();

            // Retrieve current medications from context profile/memory
            var currentMeds = context.Metadata.GetValueOrDefault("CurrentMedications", "") as string ?? "";
            currentMeds = currentMeds.ToLowerInvariant();

            // 1. Analyze Lisinopril
            if (text.Contains("lisinopril"))
            {
                medEducation.Add("Lisinopril is an ACE (Angiotensin-Converting Enzyme) Inhibitor commonly prescribed for hypertension and heart failure. It works by relaxing blood vessels to lower blood pressure.");
                medEducation.Add("Common side effects include a persistent dry cough, dizziness, and elevated potassium levels. It should not be taken during pregnancy.");

                if (text.Contains("ibuprofen") || text.Contains("advil") || currentMeds.Contains("ibuprofen") || currentMeds.Contains("advil"))
                {
                    context.Response.RiskLevel = "Medium";
                    context.Response.Summary = "WARNING: Potential drug interaction. Co-administering Lisinopril and NSAIDs (like Ibuprofen) can decrease the blood-pressure-lowering effect and increase the risk of severe kidney impairment.";
                }
            }

            // 2. Analyze Metformin
            if (text.Contains("metformin") || text.Contains("glucophage"))
            {
                medEducation.Add("Metformin is an oral biguanide anti-diabetic drug. It reduces glucose production in the liver and improves insulin sensitivity in peripheral tissues.");
                medEducation.Add("Side effects are primarily gastrointestinal (nausea, diarrhea, abdominal pain). A rare but serious complication is lactic acidosis, especially in patients with impaired renal function.");
            }

            // 3. Analyze Ibuprofen / NSAIDs
            if (text.Contains("ibuprofen") || text.Contains("advil") || text.Contains("naproxen") || text.Contains("nsaid"))
            {
                medEducation.Add("Ibuprofen is a Non-Steroidal Anti-Inflammatory Drug (NSAID) used to manage mild pain, fever, and inflammation.");
                medEducation.Add("NSAIDs block cyclooxygenase (COX) enzymes, reducing prostaglandin synthesis. Chronic use can lead to gastric ulcers, gastrointestinal bleeding, and fluid retention.");

                if (currentMeds.Contains("lisinopril"))
                {
                    context.Response.RiskLevel = "Medium";
                    context.Response.Summary = "WARNING: Potential interaction. You are currently taking Lisinopril. Regular use of NSAIDs like Ibuprofen can strain the kidneys and reduce the antihypertensive efficacy of Lisinopril.";
                }
            }

            // 4. Analyze Aspirin
            if (text.Contains("aspirin"))
            {
                medEducation.Add("Aspirin (acetylsalicylic acid) is a salicylate that inhibits platelet aggregation by irreversibly acetylating COX-1. It is used in low doses (81mg) for cardioprotection, and in higher doses for pain/fever.");
                medEducation.Add("It carries risk of gastrointestinal bleeding. In children and teenagers recovering from viral infections, aspirin is contraindicated due to the risk of Reye's Syndrome.");
            }

            // 5. Supplements Analysis (e.g. Vitamin D, Iron, Magnesium, Omega-3)
            if (text.Contains("vitamin d") || text.Contains("vit d"))
            {
                supplements.Add("Vitamin D3 (Cholecalciferol) is crucial for calcium absorption and bone mineralization. Standard maintenance is 600-2000 IU daily unless deficiency is diagnosed.");
            }
            if (text.Contains("iron") || text.Contains("ferrous sulfate") || text.Contains("anemia"))
            {
                supplements.Add("Iron supplements (e.g., Ferrous Sulfate) help treat iron-deficiency anemia. Best absorbed on an empty stomach with Vitamin C (orange juice), but can cause constipation or dark stools.");
            }
            if (text.Contains("magnesium"))
            {
                supplements.Add("Magnesium supplements (Glycinate, Citrate) are often used to support muscle function, sleep, or stool softening. Avoid excessive doses as they can cause loose stools.");
            }

            // Populating Response fields
            foreach (var med in medEducation)
            {
                context.Response.MedicationEducation.Add(med);
            }
            foreach (var sup in supplements)
            {
                context.Response.SupplementInformation.Add(sup);
            }

            return Task.CompletedTask;
        }
    }
}
