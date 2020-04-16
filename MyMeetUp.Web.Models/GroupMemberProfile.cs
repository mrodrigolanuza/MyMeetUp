using MyMeetUp.Web.Models.Shared;
using System.ComponentModel.DataAnnotations;

namespace MyMeetUp.Web.Models
{
    public class GroupMemberProfile : BaseModel
    {
        [Required]
        [Display(Name = "Nombre de Perfil")]
        public string Name { get; set; }    //Coordinador grupo, miembro grupo
    }
}
