using MyMeetUp.Web.Models.Shared;
using System.ComponentModel.DataAnnotations;

namespace MyMeetUp.Web.Models
{
    public class EventCategory : BaseModel
    {
        [Required]
        [Display(Name = "Categoría")]
        public string Name { get; set; }
    }
}
