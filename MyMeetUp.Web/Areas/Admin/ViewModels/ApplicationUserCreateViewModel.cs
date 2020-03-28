using System;
using System.ComponentModel.DataAnnotations;

namespace MyMeetUp.Web.Areas.Admin.ViewModels
{
    public class ApplicationUserCreateViewModel
    {
        [Required(ErrorMessage = "Campo Nombre obligatorio")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Campo Apellidos obligatorio")]
        [Display(Name = "Apellidos")]
        public string Surname { get; set; }
        
        [Required(ErrorMessage = "Campo Email obligatorio")]
        public string Email { get; set; }

        [RegularExpression(@"^https?:\/\/www\.linkedin\.com\/in\/.*", ErrorMessage = "The LinkedIn URL is not valid (Pattern>> https://www.linkedin.com/in/acount)")]
        public string LinkedIn { get; set; }

        [Required(ErrorMessage = "Campo DNI obligatorio")]
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

        [Required(ErrorMessage = "Campo Password obligatorio")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar password")]
        [Compare("Password", ErrorMessage = "No coinciden los campos de Password")]
        public string ConfirmPassword { get; set; }
    }
}
