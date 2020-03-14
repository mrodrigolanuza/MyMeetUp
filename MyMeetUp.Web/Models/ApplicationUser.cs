using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace MyMeetUp.Web.Models
{
    public class ApplicationUser : IdentityUser
    {

        [Required]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Apellidos")]
        public string Surname { get; set; }

        [Required]
        [StringLength(9, MinimumLength = 9)]
        [RegularExpression(@"[0-9]{8}[A-Z]", ErrorMessage = "The DNI format is not valid.")]
        public string DNI { get; set; }

        [Phone]
        [Display(Name = "Teléfono")]
        public string Phone { get; set; }

        [Display(Name = "Ciudad")]
        public string City { get; set; }

        [Display(Name = "Pais")]
        public string Country { get; set; }

        [RegularExpression(@"^https?:\/\/www\.linkedin\.com\/in\/.*", ErrorMessage = "The LinkedIn URL is not valid (Pattern>> https://www.linkedin.com/in/acount)")]
        public string LinkedIn { get; set; }

        [Display(Name = "Fecha de Alta")]
        public DateTime EntryDate { get; set; }

        [Display(Name = "Fecha de Baja")]
        public DateTime? LeavingDate { get; set; }
    }
}
