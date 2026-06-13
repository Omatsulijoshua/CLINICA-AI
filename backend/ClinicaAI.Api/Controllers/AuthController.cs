using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaAI.Core.Entities;
using ClinicaAI.Core.Enums;
using ClinicaAI.Infrastructure.Data;
using ClinicaAI.Infrastructure.Security;

namespace ClinicaAI.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ClinicaDbContext _context;
        private readonly JwtTokenService _tokenService;

        public AuthController(ClinicaDbContext context, JwtTokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public class RegisterRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            {
                return BadRequest(new { Message = "Email address already registered." });
            }

            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Free,
                IsEmailVerified = false,
                EmailVerificationToken = Guid.NewGuid().ToString("N")
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create blank clinical profile
            var profile = new UserProfile
            {
                UserId = user.Id,
                Age = 18,
                Gender = "Prefer not to say",
                Country = "USA",
                StateRegion = "",
                Height = 170,
                Weight = 70,
                BloodGroup = "O+",
                KnownConditions = "None",
                Allergies = "None",
                CurrentMedications = "None",
                MedicalHistory = "Initial profile setup."
            };

            _context.Profiles.Add(profile);
            
            // Seed a subscription record for Free tier
            var subscription = new Subscription
            {
                UserId = user.Id,
                PlanType = UserRole.Free,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1),
                IsActive = true
            };
            _context.Subscriptions.Add(subscription);

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Registration successful. Please verify your email.", VerificationToken = user.EmailVerificationToken });
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { Message = "Invalid email or password credentials." });
            }

            var token = _tokenService.GenerateToken(user);

            return Ok(new
            {
                Token = token,
                User = new { user.Id, user.Email, user.FullName, user.Role, user.IsEmailVerified }
            });
        }

        [HttpPost("google-signin")]
        public async Task<IActionResult> GoogleSignIn([FromBody] JsonElement request)
        {
            // Simple Google identity parser mock for sandbox use
            // Extract attributes from mock google details sent by frontend
            string email = "patient@clinica.ai";
            string fullName = "Google Patient";
            
            if (request.TryGetProperty("email", out var emailProp))
            {
                email = emailProp.GetString() ?? email;
            }
            if (request.TryGetProperty("name", out var nameProp))
            {
                fullName = nameProp.GetString() ?? fullName;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                // Auto register google patient
                user = new User
                {
                    Email = email,
                    FullName = fullName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Random password
                    Role = UserRole.Free,
                    IsEmailVerified = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var profile = new UserProfile
                {
                    UserId = user.Id,
                    Age = 30,
                    Gender = "Not specified",
                    Country = "USA",
                    StateRegion = "",
                    Height = 175,
                    Weight = 75,
                    BloodGroup = "O+",
                    KnownConditions = "None",
                    Allergies = "None",
                    CurrentMedications = "None",
                    MedicalHistory = "Signed in using Google Auth."
                };
                _context.Profiles.Add(profile);

                var subscription = new Subscription
                {
                    UserId = user.Id,
                    PlanType = UserRole.Free,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    IsActive = true
                };
                _context.Subscriptions.Add(subscription);

                await _context.SaveChangesAsync();
            }

            var token = _tokenService.GenerateToken(user);

            return Ok(new
            {
                Token = token,
                User = new { user.Id, user.Email, user.FullName, user.Role, user.IsEmailVerified }
            });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
            if (user == null)
            {
                return BadRequest(new { Message = "Invalid email verification token." });
            }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Email verified successfully." });
        }

        public class ResetPasswordRequestModel
        {
            public string Email { get; set; } = string.Empty;
        }

        [HttpPost("reset-password-request")]
        public async Task<IActionResult> ResetPasswordRequest([FromBody] ResetPasswordRequestModel request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (user == null)
            {
                return Ok(new { Message = "If the email exists, a password reset link has been dispatched." });
            }

            user.PasswordResetToken = Guid.NewGuid().ToString("N");
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(2);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Verification code generated.", ResetToken = user.PasswordResetToken });
        }

        public class ResetPasswordModel
        {
            public string Token { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token && u.ResetTokenExpiry > DateTime.UtcNow);
            if (user == null)
            {
                return BadRequest(new { Message = "Invalid or expired password reset token." });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.ResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Password reset successful. Please login with your new password." });
        }
    }
}
