namespace CursProject.Models
{
    public class SurveyTemplateIndexViewModel
    {
        public IEnumerable<SurveyTemplate> PopularTemplates { get; set; }
        public IEnumerable<SurveyTemplate> AllTemplates { get; set; }
        public string Filter { get; set; } // "all", "mine"  "others"
    }
}
