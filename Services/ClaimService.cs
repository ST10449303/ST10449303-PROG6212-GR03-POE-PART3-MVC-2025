using ContractMonthlyClaimSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Services
{
    public class ClaimService
    {
        private readonly ApplicationDbContext _context;

        public ClaimService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===========================
        // CREATE CLAIM
        // ===========================
        public async Task<Claim> CreateClaimAsync(Claim claim)
        {
            if (string.IsNullOrWhiteSpace(claim.LecturerProfileId))
                throw new Exception("LecturerProfileId must be set before creating a claim.");

            claim.Status = "Pending"; // New claims always start as Pending
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();
            return claim;
        }

        // ===========================
        // GET CLAIMS
        // ===========================
        public async Task<List<Claim>> GetAllClaimsAsync()
        {
            return await _context.Claims
                .Include(c => c.LecturerProfile)
                .Include(c => c.Lecturer)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetClaimsByLecturerAsync(string lecturerId)
        {
            if (string.IsNullOrWhiteSpace(lecturerId)) return new List<Claim>();

            return await _context.Claims
                .Include(c => c.LecturerProfile)
                .Include(c => c.Lecturer)
                .Where(c => c.LecturerId == lecturerId)
                .ToListAsync();
        }

        public async Task<Claim?> GetClaimByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            return await _context.Claims
                .Include(c => c.LecturerProfile)
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // ===========================
        // UPDATE & DELETE CLAIM
        // ===========================
        public async Task<bool> UpdateClaimStatusAsync(string claimId, string newStatus)
        {
            if (string.IsNullOrWhiteSpace(claimId) || string.IsNullOrWhiteSpace(newStatus))
                return false;

            var claim = await _context.Claims.FirstOrDefaultAsync(c => c.Id == claimId);
            if (claim == null) return false;

            claim.Status = newStatus.Trim();
            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteClaimAsync(string claimId)
        {
            if (string.IsNullOrWhiteSpace(claimId)) return false;

            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null) return false;

            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();
            return true;
        }

        // ===========================
        // LECTURER PROFILE
        // ===========================
        public async Task<LecturerProfile?> GetLecturerProfileAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;
            return await _context.LecturerProfiles.FirstOrDefaultAsync(p => p.Id == userId);
        }

        public async Task<LecturerProfile> SaveLecturerProfileAsync(LecturerProfile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.Id))
                throw new Exception("LecturerProfile.Id must be set before saving.");

            var existing = await _context.LecturerProfiles.FirstOrDefaultAsync(p => p.Id == profile.Id);

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

        // ===========================
        // VALIDATE CLAIM (MULTI-LEVEL)
        // ===========================
        public bool ValidateClaim(Claim claim, string role = "Coordinator")
        {
            if (claim == null) return false;

            // Lecturer max 24, Coordinator max 40, Manager max 50
            decimal maxHours = role.ToLower() switch
            {
                "lecturer" => 24m,
                "coordinator" => 40m,
                "manager" => 50m,
                _ => 40m
            };

            if (claim.HoursWorked <= 0 || claim.HoursWorked > maxHours) return false;
            if (claim.HourlyRate <= 0 || claim.HourlyRate > 1000) return false;

            // Optional: enforce total pay limit if needed
            // if ((claim.HoursWorked * claim.HourlyRate) > 2000) return false;

            return true;
        }

        // ===========================
        // AUTO-VERIFY PENDING CLAIMS
        // ===========================
        public async Task<AutoVerifyResult> AutoVerifyClaimsDetailedAsync(string role = "Coordinator")
        {
            var pendingClaims = await _context.Claims
                .Where(c => c.Status != null && c.Status.Trim().ToLower() == "pending")
                .ToListAsync();

            var verifiedIds = new List<string>();
            var rejectedIds = new List<string>();

            foreach (var claim in pendingClaims)
            {
                if (ValidateClaim(claim, role))
                {
                    claim.Status = "Verified";
                    verifiedIds.Add(claim.Id);
                }
                else
                {
                    claim.Status = "Rejected";
                    rejectedIds.Add(claim.Id);
                }
            }

            if (pendingClaims.Any())
                _context.Claims.UpdateRange(pendingClaims);

            await _context.SaveChangesAsync();

            return new AutoVerifyResult
            {
                TotalPending = pendingClaims.Count,
                VerifiedIds = verifiedIds,
                RejectedIds = rejectedIds
            };
        }

        // ===========================
        // AUTO-APPROVE VERIFIED CLAIMS
        // ===========================
        public async Task<AutoApproveResult> AutoApproveClaimsDetailedAsync(string role = "Coordinator")
        {
            var verifiedClaims = await _context.Claims
                .Where(c => c.Status != null && c.Status.Trim().ToLower() == "verified")
                .ToListAsync();

            var approvedIds = new List<string>();
            var rejectedIds = new List<string>();

            foreach (var claim in verifiedClaims)
            {
                if (ValidateClaim(claim, role))
                {
                    claim.Status = "Approved";
                    approvedIds.Add(claim.Id);
                }
                else
                {
                    claim.Status = "Rejected";
                    rejectedIds.Add(claim.Id);
                }
            }

            if (verifiedClaims.Any())
                _context.Claims.UpdateRange(verifiedClaims);

            await _context.SaveChangesAsync();

            return new AutoApproveResult
            {
                TotalVerified = verifiedClaims.Count,
                ApprovedIds = approvedIds,
                RejectedIds = rejectedIds
            };
        }

        // ===========================
        // HELPER RESULT CLASSES
        // ===========================
        public class AutoVerifyResult
        {
            public int TotalPending { get; set; }
            public List<string> VerifiedIds { get; set; } = new();
            public List<string> RejectedIds { get; set; } = new();
        }

        public class AutoApproveResult
        {
            public int TotalVerified { get; set; }
            public List<string> ApprovedIds { get; set; } = new();
            public List<string> RejectedIds { get; set; } = new();
        }
    }
}
