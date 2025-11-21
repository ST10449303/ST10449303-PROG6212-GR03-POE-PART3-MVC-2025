using ContractMonthlyClaimSystem.ViewModels;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace ContractMonthlyClaimSystem.Validators
{
    public class ClaimValidator : AbstractValidator<SubmitClaimVM>
    {
        private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".xlsx" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public ClaimValidator()
        {
            // 🔹 Lecturer Profile Validation
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full Name is required.")
                .MaximumLength(100).WithMessage("Full Name cannot exceed 100 characters.");

            RuleFor(x => x.EmployeeID)
                .NotEmpty().WithMessage("Employee ID is required.")
                .MaximumLength(50);

            RuleFor(x => x.YearLevel)
                .NotEmpty().WithMessage("Year Level is required.")
                .MaximumLength(20);

            RuleFor(x => x.QualificationName)
                .NotEmpty().WithMessage("Qualification Name is required.")
                .MaximumLength(100);

            RuleFor(x => x.QualificationCode)
                .NotEmpty().WithMessage("Qualification Code is required.")
                .MaximumLength(20);

            RuleFor(x => x.Faculty)
                .NotEmpty().WithMessage("Faculty is required.")
                .MaximumLength(50);

            // 🔹 Claim Fields Validation
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");

            RuleFor(x => x.HoursWorked)
                .InclusiveBetween(0.1, 24).WithMessage("Hours worked must be between 0.1 and 24.");

            RuleFor(x => x.HourlyRate)
                .GreaterThan(0).WithMessage("Hourly rate must be greater than 0.");

            // 🔹 Module Info Validation
            RuleFor(x => x.ModuleName)
                .NotEmpty().WithMessage("Module Name is required.")
                .MaximumLength(100);

            RuleFor(x => x.ModuleCode)
                .NotEmpty().WithMessage("Module Code is required.")
                .MaximumLength(20);

            // 🔹 File Validation
            RuleFor(x => x.File)
                .Must(BeAValidFile).WithMessage("Invalid file. Only PDF, DOCX, XLSX under 5MB allowed.");
        }

        // Helper method for file validation
        private bool BeAValidFile(IFormFile? file)
        {
            if (file == null) return true;

            var ext = Path.GetExtension(file.FileName)?.ToLower();
            if (!AllowedExtensions.Contains(ext)) return false;

            if (file.Length > MaxFileSize) return false;

            return true;
        }
    }
}
