using System.ComponentModel.DataAnnotations;

namespace PublicComplaintForm_API.Models
{
    public class ContactFormRequest
    {
        [Required(ErrorMessage = "Case ID is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "CourtCaseNumber must be between 1 and 100 characters")]
        public string CourtCaseNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "ContactDescription is required")]
        [StringLength(7000, MinimumLength = 1, ErrorMessage = "ContactDescription must be between 1 and 7000 characters")]
        public string ContactDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "CourtHouse is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "CourtHouse must be between 1 and 100 characters")]
        public string CourtHouse { get; set; } = string.Empty;
    }
}
