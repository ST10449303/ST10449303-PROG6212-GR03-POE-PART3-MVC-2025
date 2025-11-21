using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;
using ContractMonthlyClaimSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Authorize(Roles = "Coordinator")]
    public class CoordinatorController : Controller
    {
        private readonly ClaimService _claimService;

        public CoordinatorController(ClaimService claimService)
        {
            _claimService = claimService;
        }

        // ======================================================
        // DASHBOARD
        // ======================================================
        public async Task<IActionResult> Dashboard()
        {
            var allClaims = await _claimService.GetAllClaimsAsync();

            var vm = new CoordinatorDashboardVM
            {
                TotalPending = CountByStatus(allClaims, "Pending"),
                TotalVerified = CountByStatus(allClaims, "Verified"),
                TotalApproved = CountByStatus(allClaims, "Approved"),
                TotalRejected = CountByStatus(allClaims, "Rejected"),

                PendingClaims = MapToClaimVM(FilterByStatus(allClaims, "Pending")),
                VerifiedClaims = MapToClaimVM(FilterByStatus(allClaims, "Verified")),
                ApprovedClaims = MapToClaimVM(FilterByStatus(allClaims, "Approved")),
                RejectedClaims = MapToClaimVM(FilterByStatus(allClaims, "Rejected"))
            };

            return View(vm);
        }

        // ======================================================
        // ALL SUBMITTED CLAIMS VIEW
        // ======================================================
        public async Task<IActionResult> AllClaims()
        {
            var allClaims = await _claimService.GetAllClaimsAsync();

            var vm = new CoordinatorDashboardVM
            {
                TotalPending = CountByStatus(allClaims, "Pending"),
                TotalVerified = CountByStatus(allClaims, "Verified"),
                TotalApproved = CountByStatus(allClaims, "Approved"),
                TotalRejected = CountByStatus(allClaims, "Rejected"),

                PendingClaims = MapToClaimVM(FilterByStatus(allClaims, "Pending")),
                VerifiedClaims = MapToClaimVM(FilterByStatus(allClaims, "Verified")),
                ApprovedClaims = MapToClaimVM(FilterByStatus(allClaims, "Approved")),
                RejectedClaims = MapToClaimVM(FilterByStatus(allClaims, "Rejected"))
            };

            return View(vm);
        }

        // ======================================================
        // VERIFY SINGLE CLAIM
        // ======================================================
        public async Task<IActionResult> Verify(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessage"] = "Invalid claim ID.";
                return RedirectToAction("Dashboard");
            }

            var success = await _claimService.UpdateClaimStatusAsync(id, "Verified");
            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Claim has been verified." : "Unable to verify claim.";

            return RedirectToAction("Dashboard");
        }

        // ======================================================
        // REJECT SINGLE CLAIM
        // ======================================================
        public async Task<IActionResult> Reject(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessage"] = "Invalid claim ID.";
                return RedirectToAction("Dashboard");
            }

            var success = await _claimService.UpdateClaimStatusAsync(id, "Rejected");
            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Claim has been rejected." : "Unable to reject claim.";

            return RedirectToAction("Dashboard");
        }

        // ======================================================
        // AUTO VERIFY ALL PENDING CLAIMS
        // ======================================================
        public async Task<IActionResult> AutoVerify()
        {
            var result = await _claimService.AutoVerifyClaimsDetailedAsync("Coordinator");

            if (!result.VerifiedIds.Any() && !result.RejectedIds.Any())
            {
                TempData["ErrorMessage"] = "No pending claims available for auto-verification.";
            }
            else
            {
                if (result.VerifiedIds.Any())
                    TempData["SuccessMessage"] = $"{result.VerifiedIds.Count} claim(s) automatically verified.";

                if (result.RejectedIds.Any())
                    TempData["ErrorMessage"] = $"{result.RejectedIds.Count} claim(s) failed validation.";
            }

            return RedirectToAction("Dashboard");
        }

        // ======================================================
        // HELPER METHODS
        // ======================================================
        private int CountByStatus(System.Collections.Generic.List<Claim> claims, string status)
        {
            return claims.Count(c => string.Equals(c.Status?.Trim(), status, StringComparison.OrdinalIgnoreCase));
        }

        private System.Collections.Generic.List<Claim> FilterByStatus(System.Collections.Generic.List<Claim> claims, string status)
        {
            return claims.Where(c => string.Equals(c.Status?.Trim(), status, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // ======================================================
        // MAP CLAIM TO VIEWMODEL WITH DECIMAL CAST
        // ======================================================
        private System.Collections.Generic.List<ClaimVM> MapToClaimVM(System.Collections.Generic.List<Claim> claims)
        {
            return claims.Select(c => new ClaimVM
            {
                Id = c?.Id ?? string.Empty,
                Title = c?.Title ?? string.Empty,
                HoursWorked = c?.HoursWorked != null ? (decimal)c.HoursWorked : 0m,
                HourlyRate = c?.HourlyRate != null ? (decimal)c.HourlyRate : 0m,
                Status = c?.Status ?? "Pending",
                DateSubmitted = c?.DateSubmitted ?? System.DateTime.MinValue,
                FilePath = c?.FilePath ?? string.Empty,
                Notes = c?.Description ?? string.Empty,
                ModuleName = c?.ModuleName ?? string.Empty,
                ModuleCode = c?.ModuleCode ?? string.Empty,
                FullName = c?.LecturerProfile?.FullName ?? string.Empty,
                EmployeeID = c?.LecturerProfile?.EmployeeID ?? string.Empty,
                YearLevel = c?.LecturerProfile?.YearLevel ?? string.Empty,
                QualificationName = c?.LecturerProfile?.QualificationName ?? string.Empty,
                QualificationCode = c?.LecturerProfile?.QualificationCode ?? string.Empty,
                Faculty = c?.LecturerProfile?.Faculty ?? string.Empty
            }).ToList();
        }
    }
}
