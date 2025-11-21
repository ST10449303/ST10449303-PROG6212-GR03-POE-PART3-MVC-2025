
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.ViewModels.HR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Services.Reports
{
    public class ReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================
        // Generate Monthly Report
        // ==========================
        public async Task<ReportVM> GenerateMonthlyReportAsync(int month, int year)
        {
            // Fetch approved claims for the specified month/year
            var approvedClaims = await _context.Claims
                .Where(c => c.Status != null &&
                            c.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase) &&
                            c.DateSubmitted.Month == month &&
                            c.DateSubmitted.Year == year)
                .Include(c => c.LecturerProfile)
                .ToListAsync();

            // Map to HR-friendly structure
            var claimVMs = approvedClaims.Select(c => new ApprovedClaimVM_HR
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

            return new ReportVM
            {
                Claims = claimVMs,
                Total = claimVMs.Sum(c => c.TotalAmount),
                GeneratedOn = DateTime.Now
            };
        }
    }
}
