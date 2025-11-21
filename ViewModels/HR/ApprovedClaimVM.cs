using System;

namespace ContractMonthlyClaimSystem.ViewModels.HR
{
    /// <summary>
    /// Represents an approved claim for HR reporting purposes.
    /// </summary>
    public class ApprovedClaimVM_HR
    {
        /// <summary>
        /// Unique identifier of the claim.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Lecturer's user ID.
        /// </summary>
        public string LecturerId { get; set; } = string.Empty;

        /// <summary>
        /// Lecturer's full name.
        /// </summary>
        public string LecturerName { get; set; } = string.Empty;

        /// <summary>
        /// Lecturer's employee ID.
        /// </summary>
        public string EmployeeID { get; set; } = string.Empty;

        /// <summary>
        /// Lecturer's year level.
        /// </summary>
        public string YearLevel { get; set; } = string.Empty;

        /// <summary>
        /// Lecturer's qualification name.
        /// </summary>
        public string QualificationName { get; set; } = string.Empty;

        /// <summary>
        /// Lecturer's qualification code.
        /// </summary>
        public string QualificationCode { get; set; } = string.Empty;

        /// <summary>
        /// Faculty of the lecturer.
        /// </summary>
        public string Faculty { get; set; } = string.Empty;

        /// <summary>
        /// Name of the module associated with the claim.
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;

        /// <summary>
        /// Module code.
        /// </summary>
        public string ModuleCode { get; set; } = string.Empty;

        /// <summary>
        /// Hours worked for this claim.
        /// </summary>
        public double HoursWorked { get; set; }

        /// <summary>
        /// Hourly rate for the claim.
        /// </summary>
        public decimal HourlyRate { get; set; }

        /// <summary>
        /// Total amount for the claim (HoursWorked * HourlyRate).
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Date when the claim was approved.
        /// </summary>
        public DateTime ApprovalDate { get; set; }

        /// <summary>
        /// Optional file path for supporting documents.
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Current status of the claim (e.g., Approved).
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
}
