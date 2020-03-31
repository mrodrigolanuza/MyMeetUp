using MyMeetUp.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyMeetUp.Web.Areas.Admin.ViewModels
{
    public class GroupCreateViewModel
    {
        public Group Group { get; set; }
        [Display(Name ="Categorías")]
        public List<int> GroupCategories { get; set; }
    }
}
