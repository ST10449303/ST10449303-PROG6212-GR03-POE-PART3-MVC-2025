using ContractMonthlyClaimSystem.Services;
using ContractMonthlyClaimSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Add [Authorize(Roles = "Lecturer,HR,Manager")] if needed
    public class ClaimsApiController : ControllerBase
    {
        private readonly ClaimService _claimService;

        public ClaimsApiController(ClaimService claimService)
        {
            _claimService = claimService;
        }

        /// <summary>
        /// Get all claims.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var claims = await _claimService.GetAllClaimsAsync();

            var vm = claims.Select(c => new ClaimVM
            {
                Id = c.Id,
                Title = c.Title ?? string.Empty,
                Notes = c.Description ?? string.Empty,
                HoursWorked = c.HoursWorked,
                HourlyRate = c.HourlyRate,
                Status = c.Status ?? "Pending",
                DateSubmitted = c.DateSubmitted,
                FilePath = c.FilePath ?? string.Empty,
                ModuleName = c.ModuleName ?? string.Empty,
                ModuleCode = c.ModuleCode ?? string.Empty,
                FullName = c.LecturerProfile?.FullName ?? string.Empty,
                EmployeeID = c.LecturerProfile?.EmployeeID ?? string.Empty,
                YearLevel = c.LecturerProfile?.YearLevel ?? string.Empty,
                QualificationName = c.LecturerProfile?.QualificationName ?? string.Empty,
                QualificationCode = c.LecturerProfile?.QualificationCode ?? string.Empty,
                Faculty = c.LecturerProfile?.Faculty ?? string.Empty
            }).ToList();

            return Ok(new { Total = vm.Count, Claims = vm });
        }

        /// <summary>
        /// Auto-verify pending claims with optional role.
        /// </summary>
        [HttpPost("AutoVerifyDetailed")]
        public async Task<IActionResult> AutoVerifyDetailed([FromQuery] string? role = null)
        {
            var result = await _claimService.AutoVerifyClaimsDetailedAsync(role);

            var rejectedWithReason = result.RejectedIds.Select(id => new
            {
                Id = id,
                Reason = "Validation failed (hours/rate/amount rules)"
            });

            return Ok(new
            {
                TotalPending = result.TotalPending,
                VerifiedCount = result.VerifiedIds.Count,
                VerifiedIds = result.VerifiedIds,
                RejectedCount = rejectedWithReason.Count(),
                Rejected = rejectedWithReason
            });
        }

        /// <summary>
        /// Auto-approve verified claims with optional role.
        /// </summary>
        [HttpPost("AutoApproveDetailed")]
        public async Task<IActionResult> AutoApproveDetailed([FromQuery] string? role = null)
        {
            var result = await _claimService.AutoApproveClaimsDetailedAsync(role);

            var rejectedWithReason = result.RejectedIds.Select(id => new
            {
                Id = id,
                Reason = "Validation failed for approval (business rules)"
            });

            return Ok(new
            {
                TotalVerified = result.TotalVerified,
                ApprovedCount = result.ApprovedIds.Count,
                ApprovedIds = result.ApprovedIds,
                RejectedCount = rejectedWithReason.Count(),
                Rejected = rejectedWithReason
            });
        }

        /// <summary>
        /// Update claim status by ID.
        /// </summary>
        [HttpPost("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] StatusUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(id) || dto == null || string.IsNullOrWhiteSpace(dto.Status))
                return BadRequest(new { Message = "Claim ID and Status are required." });

            var updated = await _claimService.UpdateClaimStatusAsync(id, dto.Status);
            if (!updated)
                return NotFound(new { Message = $"Claim not found: {id}" });

            return Ok(new { Message = $"Claim {id} status updated to '{dto.Status}'." });
        }

        /// <summary>
        /// DTO for updating claim status.
        /// </summary>
        public class StatusUpdateDto
        {
            public string Status { get; set; } = string.Empty;
        }
    }
}
