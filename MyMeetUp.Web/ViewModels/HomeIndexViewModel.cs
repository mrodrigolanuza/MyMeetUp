using MyMeetUp.Web.Models;
using System.Collections.Generic;

namespace MyMeetUp.Web.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Group> Groups { get; set; }
        public List<GroupCategory> GroupCategories { get; set; }
    }
}
