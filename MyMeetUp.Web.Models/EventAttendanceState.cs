using MyMeetUp.Web.Models.Shared;
using System.ComponentModel.DataAnnotations;


namespace MyMeetUp.Web.Models
{
    public class EventAttendanceState : BaseModel
    {
        [Required]
        [Display(Name = "Estado Asistencia a Evento")]
        public string State { get; set; }    //Asistiré, No Asistiré, Asistió, No Asistió, Plaza Liberada
    }
}
