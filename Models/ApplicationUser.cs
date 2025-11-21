using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace ContractMonthlyClaimSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Full name of the user
        public string FullName { get; set; } = string.Empty;

        // User role (optional, since IdentityUser already has Roles)
        public string Role { get; set; } = string.Empty;

        // Navigation property for related claims
        public ICollection<Claim> Claims { get; set; } = new List<Claim>();

        // Navigation property for LecturerProfile
        // Use string type for Id if LecturerProfile.Id is string (matches your earlier updates)
        public string? LecturerProfileId { get; set; }
        public LecturerProfile? LecturerProfile { get; set; }
    }
}
