namespace ContractMonthlyClaimSystem.ViewModels.HR
{
   
        public class HRDashboardVM
        {
            public int TotalPending { get; set; }
            public int TotalVerified { get; set; }
            public int TotalApproved { get; set; }
            public int TotalRejected { get; set; }
            public decimal TotalAmount { get; set; }
        }

        public class ApprovedClaimVM
        {
            public string Id { get; set; } = string.Empty;
            public string LecturerName { get; set; } = string.Empty;
            public string ModuleName { get; set; } = string.Empty;
            public string ModuleCode { get; set; } = string.Empty;
            public decimal HoursWorked { get; set; }
            public decimal HourlyRate { get; set; }
            public decimal Amount { get; set; }
            public System.DateTime DateSubmitted { get; set; }
        }
    }
