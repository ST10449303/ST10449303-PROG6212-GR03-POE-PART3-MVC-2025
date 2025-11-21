using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.ViewModels
{
    public class ClaimViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Hours Worked is required.")]
        [Range(0.1, 24, ErrorMessage = "Hours worked must be between 0.1 and 24.")]
        [Display(Name = "Hours Worked")]
        public double HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly Rate is required.")]
        [Range(0.01, 1000, ErrorMessage = "Hourly rate must be positive.")]
        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Amount (auto-calculated)")]
        public decimal Amount { get; set; }  // calculated in controller

        [Display(Name = "Upload Supporting Document")]
        public IFormFile File { get; set; }

        public string ExistingFilePath { get; set; } // For edit scenario
    }
}
