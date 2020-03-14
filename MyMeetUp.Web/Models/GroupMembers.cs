using MyMeetUp.Web.Models.Shared;
using System.ComponentModel.DataAnnotations;

namespace MyMeetUp.Web.Models
{
    public class GroupMembers : BaseModel
    {
        [Required]
        public int GroupId { get; set; }
        public Group Group { get; set; }
        
        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        
        [Required]
        public int GroupMemberProfileId { get; set; }
        public GroupMemberProfile GroupMemberProfile { get; set; }
    }
}
