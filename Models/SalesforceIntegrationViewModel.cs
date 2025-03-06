using System.ComponentModel.DataAnnotations;

namespace CursProject.Models
{
    public class SalesforceIntegrationViewModel
    {
        // ==== ДЛЯ АККАУНТА (уже было) ====
        public string FullName { get; set; }
        public string Email { get; set; }

        // ==== ДЛЯ КОНТАКТА (новые поля) ====
        [Required(ErrorMessage = "First Name is required")]
        public string ContactFullName { get; set; }

        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }

        public string ContactTitle { get; set; }
    }
}
