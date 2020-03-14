using System;
using MyMeetUp.Web.Models.Shared;
using System.ComponentModel.DataAnnotations;

namespace MyMeetUp.Web.Models
{
    public class Group : BaseModel
    {
        [Required]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Required]
        [StringLength(3000)]
        [Display(Name = "Sobre Nosotros..")]
        public string AboutUs { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Pais")]
        public string Country { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Ciudad")]
        public string City { get; set; }

        [Required]
        [Display(Name = "Fecha Alta")]
        public DateTime CreationDate { get; set; }
        
        [Display(Name = "Fecha Baja")]
        public DateTime? FinalizationDate { get; set; }
    }
}
