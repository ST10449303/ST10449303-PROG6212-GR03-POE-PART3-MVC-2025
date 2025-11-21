using System.Collections.Generic;
using System.Linq;
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.ViewModels.HR
{
    public class LecturersVM
    {
        // List of lecturers
        public List<LecturerProfile> Lecturers { get; set; } = new();

        // Optional: mapping of Lecturer ID to total claims
        public Dictionary<string, int> TotalClaimsPerLecturer { get; set; } = new();

        // Optional: mapping of Lecturer ID to total approved claims
        public Dictionary<string, int> ApprovedClaimsPerLecturer { get; set; } = new();

        // Optional: mapping of Lecturer ID to total pending claims
        public Dictionary<string, int> PendingClaimsPerLecturer { get; set; } = new();

        // Optional: mapping of Lecturer ID to total verified claims
        public Dictionary<string, int> VerifiedClaimsPerLecturer { get; set; } = new();

        // Optional: mapping of Lecturer ID to total rejected claims
        public Dictionary<string, int> RejectedClaimsPerLecturer { get; set; } = new();

        // Optional: mapping of Lecturer ID to total amount claimed
        public Dictionary<string, decimal> TotalAmountPerLecturer { get; set; } = new();

        // Convenience properties for overall totals
        public int TotalClaims => TotalClaimsPerLecturer.Values.Sum();
        public int TotalApproved => ApprovedClaimsPerLecturer.Values.Sum();
        public int TotalPending => PendingClaimsPerLecturer.Values.Sum();
        public int TotalVerified => VerifiedClaimsPerLecturer.Values.Sum();
        public int TotalRejected => RejectedClaimsPerLecturer.Values.Sum();
        public decimal TotalAmount => TotalAmountPerLecturer.Values.Sum();

        // Populate computed fields
        public void ComputeStats(IEnumerable<Claim> allClaims)
        {
            foreach (var lecturer in Lecturers)
            {
                var lecturerClaims = allClaims.Where(c => c.LecturerProfileId == lecturer.Id).ToList();

                TotalClaimsPerLecturer[lecturer.Id] = lecturerClaims.Count;
                ApprovedClaimsPerLecturer[lecturer.Id] = lecturerClaims.Count(c => c.Status?.ToLower() == "approved");
                PendingClaimsPerLecturer[lecturer.Id] = lecturerClaims.Count(c => c.Status?.ToLower() == "pending");
                VerifiedClaimsPerLecturer[lecturer.Id] = lecturerClaims.Count(c => c.Status?.ToLower() == "verified");
                RejectedClaimsPerLecturer[lecturer.Id] = lecturerClaims.Count(c => c.Status?.ToLower() == "rejected");
                TotalAmountPerLecturer[lecturer.Id] = lecturerClaims.Sum(c => c.HoursWorked * c.HourlyRate);
            }
        }
    }
}
