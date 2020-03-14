using MyMeetUp.Web.Models.Shared;
using System;
using System.ComponentModel.DataAnnotations;

namespace MyMeetUp.Web.Models
{
    public class Event : BaseModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Título")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Detalles")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Dirección")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Ciudad")]
        public string City { get; set; }

        [Required]
        [Display(Name = "País")]
        public string Country { get; set; }

        [Required]
        [Display(Name = "Fecha y Hora")]
        public DateTime FechaHora { get; set; }

        //Foreign Keys
        [Required]
        public int GroupId { get; set; }
        public Group Group { get; set; }

        [Required]
        public int EventCategoryId { get; set; }
        public EventCategory EventCategory { get; set; }
    }
}
