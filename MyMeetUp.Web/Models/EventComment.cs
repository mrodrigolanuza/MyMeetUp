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
        public DateTime PublicationDate { get; set; }

        //ForeignKeys
        [Required]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public int EventId { get; set; }
        public Event Event { get; set; }

        public int? ParentEventCommentId { get; set; }
        public EventComment ParentEventComment { get; set; }
    }
}
