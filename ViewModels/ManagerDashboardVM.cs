using System.Collections.Generic;
using System.Linq;

namespace ContractMonthlyClaimSystem.ViewModels
{
    public class ManagerDashboardVM
    {
        // Totals passed from Controller
        public int TotalVerified { get; set; }
        public int TotalApproved { get; set; }
        public int TotalRejected { get; set; }

        // Claim lists grouped by status
        public List<ClaimVM> VerifiedClaims { get; set; } = new();
        public List<ClaimVM> ApprovedClaims { get; set; } = new();
        public List<ClaimVM> RejectedClaims { get; set; } = new();

        // Combined list for "All Claims" page
        public IEnumerable<ClaimVM> AllClaims =>
            (VerifiedClaims ?? Enumerable.Empty<ClaimVM>())
            .Concat(ApprovedClaims ?? Enumerable.Empty<ClaimVM>())
            .Concat(RejectedClaims ?? Enumerable.Empty<ClaimVM>());

        // ========= TOTAL AMOUNTS (HoursWorked * HourlyRate) =========
        public decimal TotalVerifiedAmount =>
            VerifiedClaims?.Sum(c => c.TotalAmount) ?? 0m;

        public decimal TotalApprovedAmount =>
            ApprovedClaims?.Sum(c => c.TotalAmount) ?? 0m;

        public decimal TotalRejectedAmount =>
            RejectedClaims?.Sum(c => c.TotalAmount) ?? 0m;

        // Only approved claims qualify for "Payments"
        public decimal TotalPayments => TotalApprovedAmount;

        // ========= COUNT COMPUTED FROM LISTS =========
        public int ComputedTotalVerified => VerifiedClaims?.Count ?? 0;
        public int ComputedTotalApproved => ApprovedClaims?.Count ?? 0;
        public int ComputedTotalRejected => RejectedClaims?.Count ?? 0;
    }
}
