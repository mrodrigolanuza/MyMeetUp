using MyMeetUp.Web.Models;
using System.Collections.Generic;

namespace MyMeetUp.Web.ViewModels
{
    public class GroupDetailsViewModel
    {
        public Group GroupInfo { get; set; }
        public string GroupProfileImagePath { get; set; }
        public List<Event> GroupEvents { get; set; }
        public int MembersTotalNumber { get; set; }
    }
}
