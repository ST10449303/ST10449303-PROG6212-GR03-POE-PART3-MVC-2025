using ContractMonthlyClaimSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ContractMonthlyClaimSystem.ViewModels.HR
{
    public class LecturerDetailsVM
    {
        // Lecturer profile info
        public LecturerProfile Lecturer { get; set; } = new LecturerProfile();

        // List of claims submitted by the lecturer
        public List<Claim> Claims { get; set; } = new List<Claim>();

        // Computed properties for convenience
        public int TotalClaims => Claims?.Count ?? 0;
        public int PendingClaims => Claims?.Count(c => c.Status?.Equals("Pending", StringComparison.OrdinalIgnoreCase) ?? false) ?? 0;
        public int VerifiedClaims => Claims?.Count(c => c.Status?.Equals("Verified", StringComparison.OrdinalIgnoreCase) ?? false) ?? 0;
        public int ApprovedClaims => Claims?.Count(c => c.Status?.Equals("Approved", StringComparison.OrdinalIgnoreCase) ?? false) ?? 0;
        public int RejectedClaims => Claims?.Count(c => c.Status?.Equals("Rejected", StringComparison.OrdinalIgnoreCase) ?? false) ?? 0;

        public decimal TotalAmount => Claims?.Sum(c => c.HoursWorked * c.HourlyRate) ?? 0m;
    }
}
