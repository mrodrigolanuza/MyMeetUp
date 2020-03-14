using MyMeetUp.Web.Models.Shared;
using System.ComponentModel.DataAnnotations;

namespace MyMeetUp.Web.Models
{
    public class EventAttendance : BaseModel
    {
        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        
        [Required]
        public int EventId { get; set; }
        public Event Event { get; set; }
        
        [Required]
        public int EventAttendanceId { get; set; }
        public EventAttendanceState EventAttendanceState { get; set; }
    }
}
