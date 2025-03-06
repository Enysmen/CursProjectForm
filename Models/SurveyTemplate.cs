using System.ComponentModel.DataAnnotations;

namespace CursProject.Models
{
    public class SurveyTemplate
    {

        public SurveyTemplate()
        {
            Responses = new List<SurveyResponse>();
            Comments = new List<TemplateComment>();
            Likes = new List<TemplateLike>();
            AllowedUsers = new List<ApplicationUser>();
        }

        public int Id { get; set; }

        [Required]
        public string Title { get; set; }         // The value will be updated from JSON

        public string Description { get; set; }     // Same thing - from JSON

        // JSON schema created in SurveyJS Creator
        [Required]
        public string JsonSchema { get; set; }

        public bool IsPublic { get; set; }



        public DateTime CreatedDate { get; set; }

        // User ID (template creator)
        public string UserId { get; set; }

        // Navigation property to user
        public ApplicationUser User { get; set; }

        // If required - a list of answers
        public ICollection<SurveyResponse> Responses { get; set; }

        // Add collections for comments and likes
        public ICollection<TemplateComment> Comments { get; set; }
        public ICollection<TemplateLike> Likes { get; set; }

        public ICollection<ApplicationUser> AllowedUsers { get; set; }
    }
}
