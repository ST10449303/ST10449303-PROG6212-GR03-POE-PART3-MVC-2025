using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    public class LecturerProfile
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string EmployeeID { get; set; } = string.Empty;

        [StringLength(100)]
        public string QualificationName { get; set; } = string.Empty;

        [StringLength(20)]
        public string QualificationCode { get; set; } = string.Empty;

        [StringLength(100)]
        public string Faculty { get; set; } = string.Empty;

        [StringLength(50)]
        public string YearLevel { get; set; } = string.Empty;

        // ✅ Added Email field
        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}
