using MyMeetUp.Web.Models.Shared;
using System.ComponentModel.DataAnnotations;

namespace MyMeetUp.Web.Models
{
    public class Group_GrupCategory : BaseModel
    {
        [Required]
        public int GroupId { get; set; }
        public Group Group { get; set; }

        [Required]
        public int GroupCategoryId { get; set; }
        public GroupCategory GroupCategory { get; set; }
    }
}
