using System.Collections.Generic;
using System.Linq;

namespace ContractMonthlyClaimSystem.ViewModels
{
    public class CoordinatorDashboardVM
    {
        // Totals sent directly from controller
        public int TotalPending { get; set; }
        public int TotalVerified { get; set; }
        public int TotalApproved { get; set; }
        public int TotalRejected { get; set; }

        // Claims lists as ClaimVM for proper currency formatting
        public List<ClaimVM> PendingClaims { get; set; } = new();
        public List<ClaimVM> VerifiedClaims { get; set; } = new();
        public List<ClaimVM> ApprovedClaims { get; set; } = new();
        public List<ClaimVM> RejectedClaims { get; set; } = new();

        // Computed totals
        public int ComputedTotalPending => PendingClaims?.Count ?? 0;
        public int ComputedTotalVerified => VerifiedClaims?.Count ?? 0;
        public int ComputedTotalApproved => ApprovedClaims?.Count ?? 0;
        public int ComputedTotalRejected => RejectedClaims?.Count ?? 0;

        // Sum of all amounts (HoursWorked * HourlyRate)
        public decimal TotalPendingAmount => PendingClaims?.Sum(c => c.TotalAmount) ?? 0m;
        public decimal TotalVerifiedAmount => VerifiedClaims?.Sum(c => c.TotalAmount) ?? 0m;
        public decimal TotalApprovedAmount => ApprovedClaims?.Sum(c => c.TotalAmount) ?? 0m;
        public decimal TotalRejectedAmount => RejectedClaims?.Sum(c => c.TotalAmount) ?? 0m;

        // Combined list for easy iteration in card-based dashboard
        public IEnumerable<ClaimVM> AllClaims =>
            PendingClaims.Concat(VerifiedClaims)
                         .Concat(ApprovedClaims)
                         .Concat(RejectedClaims);
    }
}
