
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractMonthlyClaimSystem.Models
{
    public class Claim
    {
        
        
        public string Id { get; set; } = Guid.NewGuid().ToString(); // auto-generate

        // Claim Title
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        // Description / Notes of the claim
        [StringLength(500)]
        public string? Description { get; set; }

        // Hours worked by the lecturer for the claim
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0.1, 24, ErrorMessage = "Hours worked must be between 0.1 and 24.")]
        public decimal HoursWorked { get; set; }

        // Hourly rate for calculating claim amount
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 1000, ErrorMessage = "Hourly rate must be positive.")]
        public decimal HourlyRate { get; set; }

        // Computed Amount = HoursWorked * HourlyRate
        [NotMapped]
        public decimal Amount { get; private set; }  // EF is satisfied


        // Date the claim was submitted
        [Required]
        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        // Optional last update timestamp
        [NotMapped] // EF will ignore this property
        public DateTime CreatedAt => DateSubmitted;
        public DateTime? UpdatedAt { get; set; }

        // Status of the claim
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        // Foreign key to the lecturer profile (optional)
        public string? LecturerProfileId { get; set; }
        [ForeignKey("LecturerProfileId")]
        public LecturerProfile? LecturerProfile { get; set; }

        // FK to ApplicationUser (required, string ID)
        [Required]
        public string LecturerId { get; set; } = string.Empty;

        [ForeignKey("LecturerId")]
        public ApplicationUser? Lecturer { get; set; }

        // Uploaded file path (optional)
        [StringLength(200)]
        public string? FilePath { get; set; }

        // Module Information
        [Required]
        [StringLength(100)]
        public string ModuleName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ModuleCode { get; set; } = string.Empty;
    }
}
