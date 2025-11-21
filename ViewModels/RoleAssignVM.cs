using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ContractMonthlyClaimSystem.ViewModels
{
    public class RoleAssignVM
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        // Selected role
        public string SelectedRole { get; set; }

        // List of available roles
        public IEnumerable<SelectListItem> Roles { get; set; }
    }
}
