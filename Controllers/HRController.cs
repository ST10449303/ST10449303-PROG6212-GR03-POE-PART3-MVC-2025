using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;
using ContractMonthlyClaimSystem.ViewModels.HR;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Authorize(Roles = "HR")]
    [Route("HR")]
    public class HRController : Controller
    {
        private readonly HRService _hrService;

        public HRController(HRService hrService)
        {
            _hrService = hrService;
        }

        // ==========================
        // HR Dashboard
        // ==========================
        [Route("")]
        [Route("Dashboard")]
        public async Task<IActionResult> Index()
        {
            var claims = await _hrService.GetAllClaimsAsync();

            var vm = new HRDashboardVM
            {
                TotalPending = claims.Count(c => (c.Status ?? "").Trim().ToUpper() == "PENDING"),
                TotalVerified = claims.Count(c => (c.Status ?? "").Trim().ToUpper() == "VERIFIED"),
                TotalApproved = claims.Count(c => (c.Status ?? "").Trim().ToUpper() == "APPROVED"),
                TotalRejected = claims.Count(c => (c.Status ?? "").Trim().ToUpper() == "REJECTED"),
                TotalAmount = claims.Sum(c => c.HoursWorked * c.HourlyRate)
            };

            return View("Dashboard", vm);
        }

        // ==========================
        // Approved Claims
        // ==========================
        [Route("ApprovedClaims")]
        public async Task<IActionResult> ApprovedClaims()
        {
            var claims = await _hrService.GetAllClaimsAsync("Approved");

            var vm = claims.Select(c => new ApprovedClaimVM_HR
            {
                Id = c.Id,
                LecturerId = c.LecturerId,
                LecturerName = c.LecturerProfile?.FullName ?? "Unknown",
                EmployeeID = c.LecturerProfile?.EmployeeID ?? string.Empty,
                YearLevel = c.LecturerProfile?.YearLevel ?? string.Empty,
                QualificationName = c.LecturerProfile?.QualificationName ?? string.Empty,
                QualificationCode = c.LecturerProfile?.QualificationCode ?? string.Empty,
                Faculty = c.LecturerProfile?.Faculty ?? string.Empty,
                ModuleName = c.ModuleName,
                ModuleCode = c.ModuleCode,
                HoursWorked = (double)c.HoursWorked,
                HourlyRate = c.HourlyRate,
                TotalAmount = c.HoursWorked * c.HourlyRate,
                ApprovalDate = c.UpdatedAt ?? c.DateSubmitted,
                FilePath = c.FilePath,
                Status = c.Status ?? "Approved"
            }).ToList();

            return View(vm);
        }

        // ==========================
        // Lecturer Management
        // ==========================
        [Route("Lecturers")]
        public async Task<IActionResult> Lecturers()
        {
            var lecturers = await _hrService.GetAllLecturersAsync();
            var claims = await _hrService.GetAllClaimsAsync();

            var vm = new LecturersVM
            {
                Lecturers = lecturers
            };

            vm.ComputeStats(claims); // compute totals for each lecturer
            return View(vm);
        }

        [Route("Lecturer/Edit/{id}")]
        public async Task<IActionResult> EditLecturer(string id)
        {
            var lecturer = await _hrService.GetLecturerAsync(id);
            if (lecturer == null) return NotFound();
            return View(lecturer);
        }

        [HttpPost("Lecturer/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLecturer(LecturerProfile model)
        {
            if (!ModelState.IsValid) return View(model);

            await _hrService.SaveLecturerAsync(model);
            TempData["SuccessMessage"] = "Lecturer updated successfully.";
            return RedirectToAction("Lecturers");
        }

        // ==========================
        // Approve / Reject Single Claim
        // ==========================
        [Route("Approve/{id}")]
        public async Task<IActionResult> Approve(string id)
        {
            var success = await _hrService.ApproveClaimAsync(id, User.Identity?.Name ?? "HR");
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Claim approved successfully."
                : "Unable to approve claim.";
            return RedirectToAction("Index");
        }

        [Route("Reject/{id}")]
        public async Task<IActionResult> Reject(string id)
        {
            var success = await _hrService.RejectClaimAsync(id, User.Identity?.Name ?? "HR");
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Claim rejected successfully."
                : "Unable to reject claim.";
            return RedirectToAction("Index");
        }

        // ==========================
        // Auto-Approve Verified Claims
        // ==========================
        [Route("AutoApprove")]
        public async Task<IActionResult> AutoApprove()
        {
            var result = await _hrService.AutoApproveClaimsAsync();
            TempData["SuccessMessage"] = $"{result.ApprovedIds.Count} claims auto-approved.";
            if (result.RejectedIds.Any())
                TempData["ErrorMessage"] = $"{result.RejectedIds.Count} claims failed validation.";
            return RedirectToAction("Index");
        }

        // ==========================
        // Generate PDF Report for Approved Claims
        // ==========================
        [Route("GenerateReportPdf")]
        public async Task<IActionResult> GenerateReportPdf()
        {
            var claims = await _hrService.GetAllClaimsAsync("Approved");

            if (!claims.Any())
            {
                TempData["ErrorMessage"] = "No approved claims to generate PDF.";
                return RedirectToAction("ApprovedClaims");
            }

            using var ms = new MemoryStream();
            var doc = new Document(PageSize.A4, 20f, 20f, 20f, 20f);
            var writer = PdfWriter.GetInstance(doc, ms);
            doc.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16f);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12f);
            var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 10f);

            // Title
            var title = new Paragraph("Approved Claims Report", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15f
            };
            doc.Add(title);

            // Table
            var table = new PdfPTable(8)
            {
                WidthPercentage = 100
            };
            table.SetWidths(new float[] { 2f, 2f, 2f, 1.5f, 1.5f, 2f, 2f, 2f });

            // Table header with explicit light gray
            string[] headers = { "Lecturer Name", "Module", "Module Code", "Hours", "Rate", "Total", "Approval Date", "Status" };
            var lightGray = new BaseColor(211, 211, 211);

            foreach (var h in headers)
            {
                var cell = new PdfPCell(new Phrase(h, headerFont))
                {
                    BackgroundColor = lightGray,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5f
                };
                table.AddCell(cell);
            }

            // Table body
            foreach (var c in claims)
            {
                table.AddCell(new PdfPCell(new Phrase(c.LecturerProfile?.FullName ?? "Unknown", bodyFont)));
                table.AddCell(new PdfPCell(new Phrase(c.ModuleName, bodyFont)));
                table.AddCell(new PdfPCell(new Phrase(c.ModuleCode, bodyFont)));
                table.AddCell(new PdfPCell(new Phrase(c.HoursWorked.ToString(), bodyFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase(c.HourlyRate.ToString("C"), bodyFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase((c.HoursWorked * c.HourlyRate).ToString("C"), bodyFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase((c.UpdatedAt ?? c.DateSubmitted).ToShortDateString(), bodyFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(c.Status ?? "Approved", bodyFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
            }

            doc.Add(table);
            doc.Close();

            var fileName = $"ApprovedClaims_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(ms.ToArray(), "application/pdf", fileName);
        }
    }
}
