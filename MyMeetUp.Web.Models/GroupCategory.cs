﻿using MyMeetUp.Web.Models.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyMeetUp.Web.Models
{
    public class GroupCategory : BaseModel
    {
        [Required]
        [Display(Name = "Categoría")]
        public string Name { get; set; }

        public ICollection<Group_GroupCategory> GroupCategories { get; set; }
    }
}
