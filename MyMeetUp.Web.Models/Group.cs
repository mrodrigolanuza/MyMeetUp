using System;
using MyMeetUp.Web.Models.Shared;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MyMeetUp.Web.Models
{
    public class Group : BaseModel
    {
        [Required(ErrorMessage = "Campo obligatorio")]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [StringLength(3000)]
        [Display(Name = "Sobre Nosotros..")]
        public string AboutUs { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [StringLength(50)]
        [Display(Name = "Pais")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [StringLength(50)]
        [Display(Name = "Ciudad")]
        public string City { get; set; }

        [Display(Name = "Fecha Alta")]
        public DateTime CreationDate { get; set; }
        
        [Display(Name = "Fecha Baja")]
        public DateTime? FinalizationDate { get; set; }

        public ICollection<Group_GroupCategory> GroupCategories { get; set; }
    }
}
