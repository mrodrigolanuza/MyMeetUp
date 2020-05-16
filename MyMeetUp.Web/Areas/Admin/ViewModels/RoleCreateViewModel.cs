using System.ComponentModel.DataAnnotations;

namespace MyMeetUp.Web.Areas.Admin.ViewModels
{
    public class RoleCreateViewModel
    {
        [Required]
        [StringLength(100)]
        [MinLength(3)]
        public string Name { get; set; }
    }
}
