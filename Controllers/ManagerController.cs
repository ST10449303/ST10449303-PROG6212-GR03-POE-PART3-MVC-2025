using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;
using ContractMonthlyClaimSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly ClaimService _claimService;

        public ManagerController(ClaimService claimService)
        {
            _claimService = claimService;
        }

        // ==================================================
        // MANAGER DASHBOARD
        // ==================================================
        public async Task<IActionResult> Dashboard()
        {
            var allClaims = await _claimService.GetAllClaimsAsync();

            var vm = new ManagerDashboardVM
            {
                TotalVerified = CountByStatus(allClaims, "Verified"),
                TotalApproved = CountByStatus(allClaims, "Approved"),
                TotalRejected = CountByStatus(allClaims, "Rejected"),

                VerifiedClaims = MapToVM(FilterByStatus(allClaims, "Verified")),
                ApprovedClaims = MapToVM(FilterByStatus(allClaims, "Approved")),
                RejectedClaims = MapToVM(FilterByStatus(allClaims, "Rejected"))
            };

            return View(vm);
        }

        // ==================================================
        // ALL CLAIMS PAGE
        // ==================================================
        public async Task<IActionResult> AllClaims()
        {
            var allClaims = await _claimService.GetAllClaimsAsync();

            var vm = new ManagerDashboardVM
            {
                VerifiedClaims = MapToVM(FilterByStatus(allClaims, "Verified")),
                ApprovedClaims = MapToVM(FilterByStatus(allClaims, "Approved")),
                RejectedClaims = MapToVM(FilterByStatus(allClaims, "Rejected"))
            };

            return View(vm);
        }

        // ==================================================
        // APPROVE SINGLE CLAIM
        // ==================================================
        public async Task<IActionResult> Approve(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Invalid claim ID.";
                return RedirectToAction("Dashboard");
            }

            var success = await _claimService.UpdateClaimStatusAsync(id, "Approved");

            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Claim approved successfully." : "Failed to approve claim.";

            return RedirectToAction("Dashboard");
        }

        // ==================================================
        // REJECT SINGLE CLAIM
        // ==================================================
        public async Task<IActionResult> Reject(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Invalid claim ID.";
                return RedirectToAction("Dashboard");
            }

            var success = await _claimService.UpdateClaimStatusAsync(id, "Rejected");

            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Claim rejected successfully." : "Failed to reject claim.";

            return RedirectToAction("Dashboard");
        }

        // ==================================================
        // AUTO-APPROVE VERIFIED CLAIMS
        // ==================================================
        public async Task<IActionResult> AutoApprove()
        {
            var result = await _claimService.AutoApproveClaimsDetailedAsync("Manager");

            if (!result.ApprovedIds.Any() && !result.RejectedIds.Any())
            {
                TempData["ErrorMessage"] = "No verified claims passed validation.";
            }
            else
            {
                if (result.ApprovedIds.Any())
                    TempData["SuccessMessage"] = $"{result.ApprovedIds.Count} claim(s) automatically approved.";

                if (result.RejectedIds.Any())
                    TempData["ErrorMessage"] = $"{result.RejectedIds.Count} claim(s) failed validation.";
            }

            return RedirectToAction("Dashboard");
        }

        // ==================================================
        // AUTO-REJECT ALL PENDING CLAIMS
        // ==================================================
        public async Task<IActionResult> AutoRejectPending()
        {
            var allClaims = await _claimService.GetAllClaimsAsync();
            var pending = FilterByStatus(allClaims, "Pending");

            foreach (var claim in pending)
                await _claimService.UpdateClaimStatusAsync(claim.Id, "Rejected");

            TempData["SuccessMessage"] = pending.Any()
                ? $"{pending.Count} pending claim(s) automatically rejected."
                : "No pending claims found.";

            return RedirectToAction("Dashboard");
        }

        // ==================================================
        // HELPERS
        // ==================================================
        private int CountByStatus(List<Claim> claims, string status)
        {
            return claims.Count(c => c.Status?.Trim().Equals(status, System.StringComparison.OrdinalIgnoreCase) == true);
        }

        private List<Claim> FilterByStatus(List<Claim> claims, string status)
        {
            return claims
                .Where(c => c.Status?.Trim().Equals(status, System.StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
        }

        // ==================================================
        // MAP CLAIM → ClaimVM (FIXED VERSION)
        // ==================================================
        private List<ClaimVM> MapToVM(List<Claim> claims)
        {
            return claims.Select(c => new ClaimVM
            {
                Id = c.Id,
                Title = c.Title,
                HoursWorked = c.HoursWorked,
                HourlyRate = c.HourlyRate,
                Status = c.Status,
                Notes = c.Description,
                ModuleName = c.ModuleName,
                ModuleCode = c.ModuleCode,
                FilePath = c.FilePath,
                DateSubmitted = c.DateSubmitted,

                // AUTOFILL LECTURER FIELDS
                FullName = c.LecturerProfile?.FullName ?? "",
                EmployeeID = c.LecturerProfile?.EmployeeID ?? "",
                YearLevel = c.LecturerProfile?.YearLevel ?? "",
                QualificationName = c.LecturerProfile?.QualificationName ?? "",
                QualificationCode = c.LecturerProfile?.QualificationCode ?? "",
                Faculty = c.LecturerProfile?.Faculty ?? ""
            }).ToList();
        }
    }
}
