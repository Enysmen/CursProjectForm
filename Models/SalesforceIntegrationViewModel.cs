namespace CursProject.Models
{
    public class SalesforceIntegrationViewModel
    {
        // Данные пользователя из ApplicationUser
        public string FullName { get; set; }
        public string Email { get; set; }

        // Дополнительные данные (если есть)
        public int FormsCount { get; set; }
        public List<string> FormNames { get; set; } = new List<string>();
    }
}
