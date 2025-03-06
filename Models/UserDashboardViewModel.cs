namespace CursProject.Models
{
    public class UserDashboardViewModel
    {
        // User created templates
        public List<SurveyTemplate> MyTemplates { get; set; }
        // Completed forms (responses) for user-created templates
        public List<SurveyResponse> MyResponses { get; set; }

        public IEnumerable<ApplicationUser> AllUsers { get; set; }

        public string SelectedUserId { get; set; }
    }
}
