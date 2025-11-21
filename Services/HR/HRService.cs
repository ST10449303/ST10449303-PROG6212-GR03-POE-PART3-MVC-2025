using ContractMonthlyClaimSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Services
{
    public class HRService
    {
        private readonly ApplicationDbContext _context;

        public HRService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================
        // Get all claims (optionally filtered by status and faculty)
        // ==========================
        public async Task<List<Claim>> GetAllClaimsAsync(string? status = null, string? faculty = null)
        {
            var query = _context.Claims
                .Include(c => c.LecturerProfile)
                .Include(c => c.Lecturer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                var upperStatus = status.Trim().ToUpper();
                query = query.Where(c => c.Status != null && c.Status.Trim().ToUpper() == upperStatus);
            }

            if (!string.IsNullOrWhiteSpace(faculty))
            {
                var upperFaculty = faculty.Trim().ToUpper();
                query = query.Where(c => c.LecturerProfile != null && c.LecturerProfile.Faculty.ToUpper() == upperFaculty);
            }

            return await query.ToListAsync();
        }

        // ==========================
        // Approve single claim
        // ==========================
        public async Task<bool> ApproveClaimAsync(string claimId, string processedBy)
        {
            var claim = await _context.Claims.FirstOrDefaultAsync(c => c.Id == claimId);
            if (claim == null) return false;

            claim.Status = "Approved";
            claim.UpdatedAt = DateTime.Now;

            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();
            return true;
        }

        // ==========================
        // Reject single claim
        // ==========================
        public async Task<bool> RejectClaimAsync(string claimId, string processedBy, string? reason = null)
        {
            var claim = await _context.Claims.FirstOrDefaultAsync(c => c.Id == claimId);
            if (claim == null) return false;

            claim.Status = "Rejected";
            claim.UpdatedAt = DateTime.Now;

            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();
            return true;
        }

        // ==========================
        // Batch approve claims
        // ==========================
        public async Task<BatchProcessResult> BatchApproveClaimsAsync(List<string> claimIds, string processedBy)
        {
            var approvedIds = new List<string>();
            var rejectedIds = new List<string>();

            foreach (var id in claimIds)
            {
                var success = await ApproveClaimAsync(id, processedBy);
                if (success) approvedIds.Add(id);
                else rejectedIds.Add(id);
            }

            return new BatchProcessResult
            {
                ApprovedIds = approvedIds,
                RejectedIds = rejectedIds
            };
        }

        // ==========================
        // Auto-approve verified claims
        // ==========================
        public async Task<BatchProcessResult> AutoApproveClaimsAsync(string role = "HR")
        {
            var verifiedClaims = await _context.Claims
                .Where(c => c.Status != null && c.Status.Trim().ToUpper() == "VERIFIED")
                .ToListAsync();

            var result = new BatchProcessResult();

            foreach (var claim in verifiedClaims)
            {
                claim.Status = "Approved";
                claim.UpdatedAt = DateTime.Now;
                result.ApprovedIds.Add(claim.Id);
            }

            if (verifiedClaims.Any())
                _context.Claims.UpdateRange(verifiedClaims);

            await _context.SaveChangesAsync();
            return result;
        }

        // ==========================
        // Lecturer Management
        // ==========================
        public async Task<List<LecturerProfile>> GetAllLecturersAsync()
        {
            return await _context.LecturerProfiles.ToListAsync();
        }

        public async Task<LecturerProfile?> GetLecturerAsync(string id)
        {
            return await _context.LecturerProfiles.FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<LecturerProfile> SaveLecturerAsync(LecturerProfile profile)
        {
            var existing = await _context.LecturerProfiles.FirstOrDefaultAsync(l => l.Id == profile.Id);
            if (existing == null)
            {
                _context.LecturerProfiles.Add(profile);
            }
            else
            {
                existing.FullName = profile.FullName;
                existing.EmployeeID = profile.EmployeeID;
                existing.YearLevel = profile.YearLevel;
                existing.QualificationName = profile.QualificationName;
                existing.QualificationCode = profile.QualificationCode;
                existing.Faculty = profile.Faculty;
                _context.LecturerProfiles.Update(existing);
            }

            await _context.SaveChangesAsync();
            return profile;
        }

        // ==========================
        // Batch Process Result
        // ==========================
        public class BatchProcessResult
        {
            public List<string> ApprovedIds { get; set; } = new();
            public List<string> RejectedIds { get; set; } = new();
        }
    }
}
