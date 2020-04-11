using MyMeetUp.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMeetUp.Web.ViewModels
{
    public class GroupIndexViewModel
    {
        public List<Group> AllGroups { get; set; }
        public string UserId { get; set; }
        public List<int> ActualUserAsGroupCoordinator{ get; set; }
        public List<int> ActualUserAsGroupMember { get; set; }
        public Dictionary<int, List<string>> GroupCategories { get; set; }
    }
}
