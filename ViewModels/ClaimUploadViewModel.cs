using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.ViewModels
{
    public class ClaimUploadViewModel
    {
        [Required]
        public int ClaimId { get; set; }

        [Required(ErrorMessage = "Please select a file to upload")]
        [DataType(DataType.Upload)]
        public IFormFile File { get; set; }
    }
}
