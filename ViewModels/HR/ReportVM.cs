using System;
using System.Collections.Generic;
using System.Linq;

namespace ContractMonthlyClaimSystem.ViewModels.HR
{
    /// <summary>
    /// Report view model for HR reporting.
    /// Contains a list of ApprovedClaimVM_HR items and summary statistics.
    /// </summary>
    public class ReportVM
    {
        /// <summary>
        /// List of claims in HR-friendly format.
        /// </summary>
        public List<ApprovedClaimVM_HR> Claims { get; set; } = new();

        /// <summary>
        /// Total amount for all claims. 
        /// Assignable now, calculated from claim list if needed.
        /// </summary>
        public decimal Total { get; set; } = 0m;

        /// <summary>
        /// Total number of claims in the report.
        /// </summary>
        public int TotalClaims => Claims?.Count ?? 0;

        /// <summary>
        /// Count of pending claims.
        /// </summary>
        public int PendingClaims => Claims?.Count(c => c.Status?.Equals("Pending", StringComparison.OrdinalIgnoreCase) ?? false) ?? 0;

        /// <summary>
        /// Count of verified claims.
        /// </summary>
        public int VerifiedClaims => Claims?.Count(c => c.Status?.Equals("Verified", StringComparison.OrdinalIgnoreCase) ?? false) ?? 0;

        /// <summary>
        /// Count of approved claims.
        /// </summary>
        public int ApprovedClaims => Claims?.Count(c => c.Status?.Equals("Approved", StringComparison.OrdinalIgnoreCase) ?? false) ?? 0;

        /// <summary>
        /// Count of rejected claims.
        /// </summary>
        public int RejectedClaims => Claims?.Count(c => c.Status?.Equals("Rejected", StringComparison.OrdinalIgnoreCase) ?? false) ?? 0;

        /// <summary>
        /// Date and time when the report was generated.
        /// </summary>
        public DateTime GeneratedOn { get; set; } = DateTime.Now;

        /// <summary>
        /// Helper method to calculate Total from Claims list.
        /// Call this after setting the Claims property.
        /// </summary>
        public void ComputeTotal()
        {
            Total = Claims?.Sum(c => (decimal)c.HoursWorked * c.HourlyRate) ?? 0m;
        }
    }
}
