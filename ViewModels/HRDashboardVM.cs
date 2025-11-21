using ContractMonthlyClaimSystem.Models;
using System.Collections.Generic;
using System.Linq;

namespace ContractMonthlyClaimSystem.ViewModels
{
    public class HRDashboardVM
    {
        // Total number of approved claims
        public int TotalApproved { get; set; }

        // Sum of all approved claim amounts
        public decimal TotalPayments { get; set; }

        // List of approved claims
        public List<Claim> ApprovedClaims { get; set; } = new List<Claim>();

        // Total number of lecturers in the system
        public int TotalLecturers { get; set; }

        // Optional: Compute TotalApproved dynamically based on the list
        public int TotalApprovedClaims => ApprovedClaims?.Count ?? 0;

        // Optional: Compute TotalPayments dynamically
        public decimal TotalApprovedPayments => ApprovedClaims?.Sum(c => c.Amount) ?? 0;
    }
}
