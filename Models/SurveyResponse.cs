using System.ComponentModel.DataAnnotations;

namespace CursProject.Models
{
    public class SurveyResponse
    {
        public int Id { get; set; }

        [Required]
        public int SurveyTemplateId { get; set; }
        public SurveyTemplate SurveyTemplate { get; set; }

        public string UserId { get; set; } // Can be null for anonymous responses
        public ApplicationUser User { get; set; }

        public DateTime SubmittedDate { get; set; }

        [Required]
        public string ResponseData { get; set; }
    }
}
