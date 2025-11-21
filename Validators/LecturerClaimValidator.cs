using ContractMonthlyClaimSystem.ViewModels;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace ContractMonthlyClaimSystem.Validators
{
    public class LecturerClaimValidator : AbstractValidator<SubmitClaimVM>
    {
        private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".xlsx" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public LecturerClaimValidator()
        {
            // 🔹 Lecturer Profile Validation
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("FullName is required.")
                .MaximumLength(100).WithMessage("FullName cannot exceed 100 characters.");

            RuleFor(x => x.EmployeeID)
                .NotEmpty().WithMessage("EmployeeID is required.")
                .MaximumLength(50).WithMessage("EmployeeID cannot exceed 50 characters.");

            RuleFor(x => x.YearLevel)
                .NotEmpty().WithMessage("YearLevel is required.")
                .MaximumLength(20).WithMessage("YearLevel cannot exceed 20 characters.");

            RuleFor(x => x.QualificationName)
                .NotEmpty().WithMessage("QualificationName is required.")
                .MaximumLength(100).WithMessage("QualificationName cannot exceed 100 characters.");

            RuleFor(x => x.QualificationCode)
                .NotEmpty().WithMessage("QualificationCode is required.")
                .MaximumLength(20).WithMessage("QualificationCode cannot exceed 20 characters.");

            RuleFor(x => x.Faculty)
                .NotEmpty().WithMessage("Faculty is required.")
                .MaximumLength(50).WithMessage("Faculty cannot exceed 50 characters.");

            // 🔹 Claim Fields Validation
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");

            // Handle decimal HoursWorked safely
            RuleFor(x => x.HoursWorked)
                .GreaterThan(0).WithMessage("HoursWorked must be greater than 0.")
                .LessThanOrEqualTo(24).WithMessage("HoursWorked cannot exceed 24.");

            RuleFor(x => x.HourlyRate)
                .GreaterThan(0).WithMessage("HourlyRate must be greater than 0.");

            // 🔹 Module Info Validation
            RuleFor(x => x.ModuleName)
                .NotEmpty().WithMessage("ModuleName is required.")
                .MaximumLength(100).WithMessage("ModuleName cannot exceed 100 characters.");

            RuleFor(x => x.ModuleCode)
                .NotEmpty().WithMessage("ModuleCode is required.")
                .MaximumLength(20).WithMessage("ModuleCode cannot exceed 20 characters.");

            // 🔹 File Validation
            RuleFor(x => x.File).Must(BeAValidFile)
                .WithMessage($"File must be one of: {string.Join(", ", AllowedExtensions)} and not exceed 5MB.");
        }

        private bool BeAValidFile(IFormFile? file)
        {
            if (file == null) return true; // optional file is allowed
            var ext = Path.GetExtension(file.FileName)?.ToLower();
            if (!AllowedExtensions.Contains(ext)) return false;
            if (file.Length > MaxFileSize) return false;
            return true;
        }
    }
}
