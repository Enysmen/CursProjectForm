using System.ComponentModel.DataAnnotations;

namespace CursProject.Models
{
    public class TemplateComment
    {
        public int Id { get; set; }

        [Required]
        public int? SurveyTemplateId { get; set; }
        public SurveyTemplate SurveyTemplate { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public string CommentText { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
