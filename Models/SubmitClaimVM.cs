using Microsoft.AspNetCore.Http;

namespace ContractMonthlyClaimSystem.ViewModels
{
    public class SubmitClaimVM
    {
        // 🔹 Lecturer Profile fields
        public string FullName { get; set; } = string.Empty;
        public string EmployeeID { get; set; } = string.Empty;
        public string YearLevel { get; set; } = string.Empty;
        public string QualificationName { get; set; } = string.Empty;
        public string QualificationCode { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;

        // 🔹 Claim fields
        public string Title { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public double HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }

        // Computed Amount = HoursWorked * HourlyRate
        public decimal Amount => (decimal)HoursWorked * HourlyRate;

        public IFormFile? File { get; set; }

        // 🔹 Module Information
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
    }
}
