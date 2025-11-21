using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;
using ContractMonthlyClaimSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Authorize]
    public class ClaimsController : Controller
    {
        private readonly ClaimService _claimService;

        public ClaimsController(ClaimService claimService)
        {
            _claimService = claimService;
        }

        /// <summary>
        /// List all claims. Applies role-based filtering automatically.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var allClaims = await _claimService.GetAllClaimsAsync() ?? new List<Claim>();
            allClaims = ApplyRoleBasedFilter(allClaims);

            var vm = allClaims.Select(MapToClaimVM).ToList();
            return View(vm);
        }

        /// <summary>
        /// Display detailed information for a specific claim.
        /// </summary>
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Claim ID is required.");

            var claim = await _claimService.GetClaimByIdAsync(id);
            if (claim == null)
                return NotFound("Claim not found.");

            // Lecturers cannot view other lecturers' claims
            if (User.IsInRole("Lecturer") && claim.LecturerId != User.Identity?.Name)
                return Forbid();

            var vm = MapToClaimVM(claim);
            return View(vm);
        }

        /// <summary>
        /// Filter claims by status (e.g., Pending, Verified, Approved, Rejected).
        /// </summary>
        public async Task<IActionResult> FilterByStatus(string status)
        {
            var allClaims = await _claimService.GetAllClaimsAsync() ?? new List<Claim>();
            allClaims = ApplyRoleBasedFilter(allClaims);

            if (!string.IsNullOrWhiteSpace(status))
            {
                allClaims = allClaims
                    .Where(c => string.Equals(c.Status?.Trim(), status, System.StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var vm = allClaims.Select(MapToClaimVM).ToList();
            ViewData["FilterStatus"] = status;
            return View("Index", vm);
        }

        /// <summary>
        /// Maps a Claim entity to a ClaimVM for the view.
        /// </summary>
        private static ClaimVM MapToClaimVM(Claim claim)
        {
            return new ClaimVM
            {
                Id = claim.Id,
                Title = claim.Title ?? string.Empty,
                Notes = claim.Description ?? string.Empty,
                HoursWorked = claim.HoursWorked,
                HourlyRate = claim.HourlyRate,
                Status = claim.Status ?? "Pending",
                DateSubmitted = claim.DateSubmitted,
                FilePath = claim.FilePath ?? string.Empty,
                ModuleName = claim.ModuleName ?? string.Empty,
                ModuleCode = claim.ModuleCode ?? string.Empty,
                FullName = claim.LecturerProfile?.FullName ?? "Unknown Lecturer",
                EmployeeID = claim.LecturerProfile?.EmployeeID ?? string.Empty,
                YearLevel = claim.LecturerProfile?.YearLevel ?? string.Empty,
                QualificationName = claim.LecturerProfile?.QualificationName ?? string.Empty,
                QualificationCode = claim.LecturerProfile?.QualificationCode ?? string.Empty,
                Faculty = claim.LecturerProfile?.Faculty ?? string.Empty
            };
        }

        /// <summary>
        /// Applies role-based filtering:
        /// - Lecturers see only their own claims.
        /// - Coordinators and Managers see all claims.
        /// </summary>
        private List<Claim> ApplyRoleBasedFilter(List<Claim> claims)
        {
            if (User.IsInRole("Lecturer"))
            {
                var userId = User.Identity?.Name ?? string.Empty;
                return claims.Where(c => c.LecturerId == userId).ToList();
            }

            // Other roles see all claims
            return claims;
        }
    }
}
