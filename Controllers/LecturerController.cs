using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;
using ContractMonthlyClaimSystem.ViewModels;
using ContractMonthlyClaimSystem.Validators;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AppClaim = ContractMonthlyClaimSystem.Models.Claim;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Authorize]
    public class LecturerController : Controller
    {
        private readonly ClaimService _claimService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IValidator<SubmitClaimVM> _lecturerValidator;
        private readonly IValidator<SubmitClaimVM> _coordinatorValidator;
        private readonly IValidator<SubmitClaimVM> _managerValidator;

        public LecturerController(
            ClaimService claimService,
            IWebHostEnvironment webHostEnvironment,
            IValidator<SubmitClaimVM> lecturerValidator,
            IValidator<SubmitClaimVM> coordinatorValidator,
            IValidator<SubmitClaimVM> managerValidator)
        {
            _claimService = claimService;
            _webHostEnvironment = webHostEnvironment;
            _lecturerValidator = lecturerValidator;
            _coordinatorValidator = coordinatorValidator;
            _managerValidator = managerValidator;
        }

        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private string GetUserRole()
        {
            if (User.IsInRole("Manager")) return "Manager";
            if (User.IsInRole("Coordinator")) return "Coordinator";
            return "Lecturer";
        }

        // ==================================================
        // LECTURER DASHBOARD
        // ==================================================
        public async Task<IActionResult> Dashboard()
        {
            ViewData["ActivePage"] = "Dashboard";
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return BadRequest("Invalid user ID.");

            var claims = await _claimService.GetClaimsByLecturerAsync(userId)
                         ?? new System.Collections.Generic.List<AppClaim>();

            var profile = await _claimService.GetLecturerProfileAsync(userId);

            // Convert DB claims → ClaimVM
            var claimVMs = claims.Select(c => new ClaimVM
            {
                Id = c?.Id ?? string.Empty,
                Title = c?.Title ?? string.Empty,
                HoursWorked = c?.HoursWorked ?? 0m,
                HourlyRate = c?.HourlyRate ?? 0m,
                Status = c?.Status ?? "Pending",
                DateSubmitted = c?.DateSubmitted ?? DateTime.MinValue,
                FilePath = c?.FilePath ?? string.Empty,
                Notes = c?.Description ?? string.Empty,
                ModuleName = c?.ModuleName ?? string.Empty,
                ModuleCode = c?.ModuleCode ?? string.Empty,

                // Lecturer Profile Data
                FullName = profile?.FullName ?? string.Empty,
                EmployeeID = profile?.EmployeeID ?? string.Empty,
                YearLevel = profile?.YearLevel ?? string.Empty,
                QualificationName = profile?.QualificationName ?? string.Empty,
                QualificationCode = profile?.QualificationCode ?? string.Empty,
                Faculty = profile?.Faculty ?? string.Empty
            }).ToList();

            // Build VM (computed totals handled inside VM)
            var vm = new LecturerDashboardVM
            {
                LecturerName = profile?.FullName ?? User.Identity?.Name ?? string.Empty,
                EmployeeID = profile?.EmployeeID ?? string.Empty,
                YearLevel = profile?.YearLevel ?? string.Empty,
                QualificationName = profile?.QualificationName ?? string.Empty,
                QualificationCode = profile?.QualificationCode ?? string.Empty,
                Faculty = profile?.Faculty ?? string.Empty,

                // Only assign claims — totals are auto-calculated
                Claims = claimVMs
            };

            return View(vm);
        }

        // ==================================================
        // GET: SUBMIT CLAIM
        // ==================================================
        public async Task<IActionResult> SubmitClaim()
        {
            ViewData["ActivePage"] = "SubmitClaim";
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return BadRequest("Invalid user ID.");

            var profile = await _claimService.GetLecturerProfileAsync(userId);

            return View(new SubmitClaimVM
            {
                FullName = profile?.FullName ?? string.Empty,
                EmployeeID = profile?.EmployeeID ?? string.Empty,
                YearLevel = profile?.YearLevel ?? string.Empty,
                QualificationName = profile?.QualificationName ?? string.Empty,
                QualificationCode = profile?.QualificationCode ?? string.Empty,
                Faculty = profile?.Faculty ?? string.Empty
            });
        }

        // ==================================================
        // POST: SUBMIT CLAIM
        // ==================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(SubmitClaimVM model)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return BadRequest("Invalid user ID.");

            // Role-based validator
            IValidator<SubmitClaimVM> validator = GetUserRole() switch
            {
                "Manager" => _managerValidator,
                "Coordinator" => _coordinatorValidator,
                _ => _lecturerValidator
            };

            ValidationResult result = await validator.ValidateAsync(model);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

                return View(model);
            }

            // Update or create profile
            var profile = await _claimService.GetLecturerProfileAsync(userId)
                          ?? new LecturerProfile { Id = userId };

            profile.FullName = model.FullName ?? string.Empty;
            profile.EmployeeID = model.EmployeeID ?? string.Empty;
            profile.YearLevel = model.YearLevel ?? string.Empty;
            profile.QualificationName = model.QualificationName ?? string.Empty;
            profile.QualificationCode = model.QualificationCode ?? string.Empty;
            profile.Faculty = model.Faculty ?? string.Empty;

            await _claimService.SaveLecturerProfileAsync(profile);

            // File upload
            string? filePath = null;
            if (model.File != null)
            {
                var uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadDir);

                var ext = Path.GetExtension(model.File.FileName)?.ToLower();
                var uniqueName = $"{Guid.NewGuid()}{ext}";
                var filePathLocal = Path.Combine(uploadDir, uniqueName);

                using var stream = new FileStream(filePathLocal, FileMode.Create);
                await model.File.CopyToAsync(stream);

                filePath = $"/uploads/{uniqueName}";
            }

            // Create new Claim
            var claim = new AppClaim
            {
                Id = Guid.NewGuid().ToString(),
                LecturerId = userId,
                LecturerProfileId = profile.Id,
                Title = model.Title ?? string.Empty,
                Description = model.Notes ?? string.Empty,
                HoursWorked = (decimal)model.HoursWorked,
                HourlyRate = model.HourlyRate,
                Status = "Pending",
                FilePath = filePath ?? string.Empty,
                DateSubmitted = DateTime.Now,
                ModuleName = model.ModuleName ?? string.Empty,
                ModuleCode = model.ModuleCode ?? string.Empty
            };

            await _claimService.CreateClaimAsync(claim);

            TempData["SuccessMessage"] = "Claim submitted successfully!";
            return RedirectToAction("Dashboard");
        }

        // ==================================================
        // MY CLAIMS
        // ==================================================
        public async Task<IActionResult> MyClaims()
        {
            ViewData["ActivePage"] = "MyClaims";
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return BadRequest("Invalid user ID.");

            var claims = await _claimService.GetClaimsByLecturerAsync(userId)
                         ?? new System.Collections.Generic.List<AppClaim>();

            var vm = claims.Select(c => new ClaimVM
            {
                Id = c?.Id ?? string.Empty,
                Title = c?.Title ?? string.Empty,
                HoursWorked = c?.HoursWorked ?? 0m,
                HourlyRate = c?.HourlyRate ?? 0m,
                Status = c?.Status ?? "Pending",
                DateSubmitted = c?.DateSubmitted ?? DateTime.MinValue,
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

            return View(vm);
        }
    }
}
