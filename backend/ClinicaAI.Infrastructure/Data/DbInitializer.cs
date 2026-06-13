using System;
using System.Linq;
using ClinicaAI.Core.Entities;
using ClinicaAI.Core.Enums;

namespace ClinicaAI.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ClinicaDbContext context)
        {
            // Ensure the database exists
            context.Database.EnsureCreated();

            // Check if users exist. If not, seed them.
            if (!context.Users.Any())
            {
                var adminUser = new User
                {
                    Email = "admin@clinica.ai",
                    FullName = "Clinica Admin",
                    Role = UserRole.Admin,
                    IsEmailVerified = true,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminSecurePassword123!")
                };

                var premiumUser = new User
                {
                    Email = "premium@clinica.ai",
                    FullName = "Premium Patient",
                    Role = UserRole.Premium,
                    IsEmailVerified = true,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Premium123!")
                };

                var freeUser = new User
                {
                    Email = "free@clinica.ai",
                    FullName = "Free Account Patient",
                    Role = UserRole.Free,
                    IsEmailVerified = true,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("FreePassword123!")
                };

                context.Users.AddRange(adminUser, premiumUser, freeUser);
                context.SaveChanges();

                // Create profiles
                var premiumProfile = new UserProfile
                {
                    UserId = premiumUser.Id,
                    Age = 34,
                    Gender = "Female",
                    Country = "Nigeria",
                    StateRegion = "Lagos",
                    Height = 168,
                    Weight = 62.5,
                    BloodGroup = "O+",
                    KnownConditions = "Asthma",
                    Allergies = "Peanuts, Penicillin",
                    CurrentMedications = "Albuterol Inhaler",
                    MedicalHistory = "Diagnosed with childhood asthma. Otherwise healthy lifestyle."
                };

                var freeProfile = new UserProfile
                {
                    UserId = freeUser.Id,
                    Age = 45,
                    Gender = "Male",
                    Country = "USA",
                    StateRegion = "California",
                    Height = 180,
                    Weight = 84.0,
                    BloodGroup = "A-",
                    KnownConditions = "Hypertension",
                    Allergies = "None",
                    CurrentMedications = "Lisinopril 10mg",
                    MedicalHistory = "Mild essential hypertension managed with lisinopril since 2023."
                };

                var adminProfile = new UserProfile
                {
                    UserId = adminUser.Id,
                    Age = 40,
                    Gender = "Male",
                    Country = "UK",
                    StateRegion = "London",
                    Height = 175,
                    Weight = 78.0,
                    BloodGroup = "B+",
                    KnownConditions = "None",
                    Allergies = "None",
                    CurrentMedications = "None",
                    MedicalHistory = "System administrator account."
                };

                context.Profiles.AddRange(premiumProfile, freeProfile, adminProfile);

                // Add subscriptions
                context.Subscriptions.Add(new Subscription
                {
                    UserId = premiumUser.Id,
                    PlanType = UserRole.Premium,
                    StartDate = DateTime.UtcNow.AddDays(-5),
                    EndDate = DateTime.UtcNow.AddDays(25),
                    IsActive = true
                });

                context.Subscriptions.Add(new Subscription
                {
                    UserId = freeUser.Id,
                    PlanType = UserRole.Free,
                    StartDate = DateTime.UtcNow.AddDays(-10),
                    EndDate = DateTime.UtcNow.AddDays(365),
                    IsActive = true
                });

                context.Subscriptions.Add(new Subscription
                {
                    UserId = adminUser.Id,
                    PlanType = UserRole.Admin,
                    StartDate = DateTime.UtcNow.AddDays(-10),
                    EndDate = DateTime.UtcNow.AddDays(3650),
                    IsActive = true
                });

                // Seed some medical history for the users
                context.MedicalHistoryEntries.Add(new MedicalHistoryEntry
                {
                    ProfileId = premiumProfile.Id,
                    Condition = "Asthma",
                    Status = "Active",
                    DiagnosedDate = DateTime.UtcNow.AddYears(-15),
                    Notes = "Intermittent mild asthma, exacerbated by cold weather."
                });

                context.MedicalHistoryEntries.Add(new MedicalHistoryEntry
                {
                    ProfileId = freeProfile.Id,
                    Condition = "Hypertension",
                    Status = "Active",
                    DiagnosedDate = DateTime.UtcNow.AddYears(-3),
                    Notes = "Blood pressure controlled with 10mg daily Lisinopril."
                });

                // Add audit logs
                context.AuditLogs.Add(new AuditLog
                {
                    Action = "System Initialized",
                    IpAddress = "127.0.0.1",
                    Details = "Seeded database tables and initial user accounts.",
                    Timestamp = DateTime.UtcNow
                });

                context.SaveChanges();
            }
        }
    }
}
