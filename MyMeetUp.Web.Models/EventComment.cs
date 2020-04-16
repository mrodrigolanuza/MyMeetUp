using System;
using MyMeetUp.Web.Models.Shared;
using System.ComponentModel.DataAnnotations;

namespace MyMeetUp.Web.Models
{
    public class EventComment : BaseModel
    {
        [Required]
        [StringLength(250)]
        [Display(Name = "Comentario")]
        public string Text { get; set; }
        [Display(Name = "Fecha Publicación")]
        public DateTime PublicationDate { get; set; }

        //ForeignKeys
        [Required]
        [Display(Name = "Usuario")]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        [Display(Name = "Evento")]
        public int EventId { get; set; }
        public Event Event { get; set; }

        public int? ParentEventCommentId { get; set; }
        public EventComment ParentEventComment { get; set; }
    }
}
