using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyMeetUp.Web.Models;
using System.Collections.Generic;

namespace MyMeetUp.Web.ViewModels
{
    public class GroupCreateViewModel
    {
        public Group GroupInfo { get; set; }
        public List<int> GroupCategoriesSelected { get; set; }
        public SelectList GroupCategoriesList { get; set; }
        public IFormFile GroupImage { get; set; }
    }
}
