using System;
using System.Collections.Generic;
using System.Linq;

namespace ContractMonthlyClaimSystem.ViewModels
{
    public class LecturerDashboardVM
    {
        // Lecturer identity
        public string LecturerName { get; set; } = string.Empty;
        public string EmployeeID { get; set; } = string.Empty;

        // Lecturer profile
        public string QualificationName { get; set; } = string.Empty;
        public string QualificationCode { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
        public string YearLevel { get; set; } = string.Empty;

        // Claims list
        public List<ClaimVM> Claims { get; set; } = new List<ClaimVM>();

        // Totals and summary
        public int TotalClaims => Claims?.Count ?? 0;
        public int PendingClaims => Claims?.Count(c => c.Status == "Pending") ?? 0;
        public int ApprovedClaims => Claims?.Count(c => c.Status == "Approved") ?? 0;
        public int RejectedClaims => Claims?.Count(c => c.Status == "Rejected") ?? 0;
        public int VerifiedClaims => Claims?.Count(c => c.Status == "Verified") ?? 0;

        // Total amount for lecturer (approved + verified + pending)
        public decimal Amount => Claims?.Sum(c => c.TotalAmount) ?? 0m;
    }

    public class ClaimVM
    {
        // Core claim fields
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;

        // Hours + rate
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }

        // Computed total
        public decimal TotalAmount => HoursWorked * HourlyRate;

        // Status & submission
        public string Status { get; set; } = "Pending";
        public DateTime DateSubmitted { get; set; }

        // Supporting file
        public string FilePath { get; set; } = string.Empty;

        // Notes
        public string Notes { get; set; } = string.Empty;

        // Module info
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;

        // Lecturer profile fields used by Manager Dashboard
        public string FullName { get; set; } = string.Empty;
        public string EmployeeID { get; set; } = string.Empty;
        public string YearLevel { get; set; } = string.Empty;
        public string QualificationName { get; set; } = string.Empty;
        public string QualificationCode { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
    }
}
