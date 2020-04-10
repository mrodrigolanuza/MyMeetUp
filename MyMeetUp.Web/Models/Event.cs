using MyMeetUp.Web.Models.Shared;
using System;
using System.ComponentModel.DataAnnotations;

namespace MyMeetUp.Web.Models
{
    public class Event : BaseModel
    {
        [Required(ErrorMessage = "Campo obligatorio")]
        [StringLength(100, ErrorMessage ="El título es demasiado largo")]
        [Display(Name = "Título")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [Display(Name = "Detalles")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [Display(Name = "Dirección")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [Display(Name = "Ciudad")]
        public string City { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [Display(Name = "País")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Campo obligatorio")]
        [Display(Name = "Fecha y Hora")]
        public DateTime FechaHora { get; set; }

        //Foreign Keys
        [Required]
        [Display(Name = "Grupo")]
        public int GroupId { get; set; }
        public Group Group { get; set; }

        [Required]
        [Display(Name = "Categoría")]
        public int EventCategoryId { get; set; }
        public EventCategory EventCategory { get; set; }
    }
}
