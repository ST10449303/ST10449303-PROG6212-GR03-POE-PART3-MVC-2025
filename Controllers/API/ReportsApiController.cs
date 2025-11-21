using ContractMonthlyClaimSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsApiController : ControllerBase
    {
        private readonly ClaimService _claimService;

        public ReportsApiController(ClaimService claimService)
        {
            _claimService = claimService;
        }

        // GET: api/ReportsApi/Summary
        [HttpGet("Summary")]
        public async Task<IActionResult> Summary()
        {
            var claims = await _claimService.GetAllClaimsAsync();

            var summary = new
            {
                TotalClaims = claims.Count,
                PendingClaims = claims.Count(c => c.Status?.Equals("Pending", System.StringComparison.OrdinalIgnoreCase) ?? false),
                VerifiedClaims = claims.Count(c => c.Status?.Equals("Verified", System.StringComparison.OrdinalIgnoreCase) ?? false),
                ApprovedClaims = claims.Count(c => c.Status?.Equals("Approved", System.StringComparison.OrdinalIgnoreCase) ?? false),
                RejectedClaims = claims.Count(c => c.Status?.Equals("Rejected", System.StringComparison.OrdinalIgnoreCase) ?? false),
                TotalAmount = claims.Sum(c => c.HoursWorked * c.HourlyRate)
            };

            return Ok(summary);
        }

        // GET: api/ReportsApi/LecturerSummary
        [HttpGet("LecturerSummary")]
        public async Task<IActionResult> LecturerSummary()
        {
            var claims = await _claimService.GetAllClaimsAsync();

            var lecturerSummary = claims
                .GroupBy(c => c.LecturerId)
                .Select(g => new
                {
                    LecturerId = g.Key,
                    LecturerName = g.FirstOrDefault()?.LecturerProfile?.FullName ?? "Unknown",
                    TotalClaims = g.Count(),
                    TotalApprovedClaims = g.Count(c => c.Status?.Equals("Approved", System.StringComparison.OrdinalIgnoreCase) ?? false),
                    TotalAmount = g.Sum(c => c.HoursWorked * c.HourlyRate)
                })
                .ToList();

            return Ok(lecturerSummary);
        }
    }
}
