using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClinicaAI.Core.Interfaces;
using ClinicaAI.Core.Models;
using ClinicaAI.Infrastructure.Data;

namespace ClinicaAI.Infrastructure.Agents
{
    public class MemoryAgent : IMedicalAgent
    {
        private readonly ClinicaDbContext _context;

        public string Name => "Memory Agent";

        public MemoryAgent(ClinicaDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync(AgentExecutionContext context)
        {
            // If UserId is set in context metadata, load user's profile and memory
            if (context.Metadata.TryGetValue("UserId", out var userIdObj) && userIdObj is Guid userId)
            {
                var user = await _context.Users
                    .Include(u => u.Profile)
                    .ThenInclude(p => p!.HistoryEntries)
                    .Include(u => u.Profile)
                    .ThenInclude(p => p!.MemoryStores)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user?.Profile != null)
                {
                    context.Profile = user.Profile;
                    context.Country = user.Profile.Country;
                    context.StateRegion = user.Profile.StateRegion;

                    // Inject allergies, medications and known conditions into metadata
                    context.Metadata["Allergies"] = user.Profile.Allergies;
                    context.Metadata["CurrentMedications"] = user.Profile.CurrentMedications;
                    context.Metadata["KnownConditions"] = user.Profile.KnownConditions;

                    // Extract memory summaries
                    var memories = user.Profile.MemoryStores.ToDictionary(m => m.Key, m => m.Value);
                    context.Metadata["PastSymptoms"] = memories.GetValueOrDefault("past_symptoms", "");
                    context.Metadata["PastDiagnoses"] = memories.GetValueOrDefault("past_diagnoses", "");
                }
            }
        }
    }
}
