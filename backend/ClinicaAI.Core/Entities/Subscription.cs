using System;
using ClinicaAI.Core.Enums;

namespace ClinicaAI.Core.Entities
{
    public class Subscription
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public UserRole PlanType { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public User User { get; set; } = null!;
    }
}
