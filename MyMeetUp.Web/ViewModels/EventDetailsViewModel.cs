using MyMeetUp.Web.Models;
using System.Collections.Generic;

namespace MyMeetUp.Web.ViewModels
{
    public class EventDetailsViewModel
    {
        public Event EventInfo { get; set; }

        public bool SignedInUserWillAttend { get; set; }

        public List<ApplicationUser> EventAttendees { get; set; }

        public List<EventComment> EventComments { get; set; }

    }
}
