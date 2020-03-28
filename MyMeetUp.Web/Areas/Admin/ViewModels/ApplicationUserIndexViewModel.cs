using System;
using System.ComponentModel.DataAnnotations;

namespace MyMeetUp.Web.Areas.Admin.ViewModels
{
    public class ApplicationUserIndexViewModel
    {
        public string Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public string Surname { get; set; }
        [Required]
        public string Email { get; set; }
        public string LinkedIn { get; set; }
        [StringLength(9, MinimumLength = 9)]
        [RegularExpression(@"[0-9]{8}[A-Z]", ErrorMessage = "The DNI format is not valid.")]
        public string DNI { get; set; }
        [Phone]
        public string Phone { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime? LeavingDate { get; set; }
        
    }
}
