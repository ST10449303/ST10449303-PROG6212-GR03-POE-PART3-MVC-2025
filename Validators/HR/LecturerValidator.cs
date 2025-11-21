using ContractMonthlyClaimSystem.Models;
using FluentValidation;

namespace ContractMonthlyClaimSystem.Validators.HR
{
    public class LecturerValidator : AbstractValidator<LecturerProfile>
    {
        public LecturerValidator()
        {
            // Full Name validation
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full Name is required.")
                .MaximumLength(100).WithMessage("Full Name cannot exceed 100 characters.");

            // Employee ID validation
            RuleFor(x => x.EmployeeID)
                .NotEmpty().WithMessage("Employee ID is required.")
                .MaximumLength(50).WithMessage("Employee ID cannot exceed 50 characters.");

            // Email validation
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email address format.")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");

            // Faculty validation
            RuleFor(x => x.Faculty)
                .NotEmpty().WithMessage("Faculty is required.")
                .MaximumLength(50).WithMessage("Faculty cannot exceed 50 characters.");

            // Optional: Qualification Name and Code validation
            RuleFor(x => x.QualificationName)
                .NotEmpty().WithMessage("Qualification Name is required.")
                .MaximumLength(100).WithMessage("Qualification Name cannot exceed 100 characters.");

            RuleFor(x => x.QualificationCode)
                .NotEmpty().WithMessage("Qualification Code is required.")
                .MaximumLength(20).WithMessage("Qualification Code cannot exceed 20 characters.");

            // Optional: Year Level validation
            RuleFor(x => x.YearLevel)
                .NotEmpty().WithMessage("Year Level is required.")
                .MaximumLength(20).WithMessage("Year Level cannot exceed 20 characters.");
        }
    }
}
