using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaAI.Core.Entities;
using ClinicaAI.Infrastructure.Data;

namespace ClinicaAI.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly ClinicaDbContext _context;

        public ProfileController(ClinicaDbContext context)
        {
            _context = context;
        }

        private Guid GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdStr, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User context is not authenticated.");
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            var profile = await _context.Profiles
                .Include(p => p.HistoryEntries)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return NotFound(new { Message = "Clinical profile not found." });
            }

            return Ok(profile);
        }

        public class UpdateProfileRequest
        {
            public int Age { get; set; }
            public string Gender { get; set; } = string.Empty;
            public string Country { get; set; } = string.Empty;
            public string StateRegion { get; set; } = string.Empty;
            public double Height { get; set; }
            public double Weight { get; set; }
            public string BloodGroup { get; set; } = string.Empty;
            public string KnownConditions { get; set; } = string.Empty;
            public string Allergies { get; set; } = string.Empty;
            public string CurrentMedications { get; set; } = string.Empty;
            public string MedicalHistory { get; set; } = string.Empty;
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = GetUserId();
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile { UserId = userId };
                _context.Profiles.Add(profile);
            }

            profile.Age = request.Age;
            profile.Gender = request.Gender;
            profile.Country = request.Country;
            profile.StateRegion = request.StateRegion;
            profile.Height = request.Height;
            profile.Weight = request.Weight;
            profile.BloodGroup = request.BloodGroup;
            profile.KnownConditions = request.KnownConditions;
            profile.Allergies = request.Allergies;
            profile.CurrentMedications = request.CurrentMedications;
            profile.MedicalHistory = request.MedicalHistory;
            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Store updated details as memory store properties
            var memories = await _context.MemoryStores.ToListAsync();
            var allergiesMem = memories.FirstOrDefault(m => m.ProfileId == profile.Id && m.Key == "allergies");
            if (allergiesMem == null)
            {
                _context.MemoryStores.Add(new MemoryStore { ProfileId = profile.Id, Key = "allergies", Value = request.Allergies });
            }
            else
            {
                allergiesMem.Value = request.Allergies;
            }

            var medsMem = memories.FirstOrDefault(m => m.ProfileId == profile.Id && m.Key == "current_medications");
            if (medsMem == null)
            {
                _context.MemoryStores.Add(new MemoryStore { ProfileId = profile.Id, Key = "current_medications", Value = request.CurrentMedications });
            }
            else
            {
                medsMem.Value = request.CurrentMedications;
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Clinical profile updated successfully.", Profile = profile });
        }
    }
}
