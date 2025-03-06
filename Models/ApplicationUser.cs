using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;

namespace CursProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;   

        public bool IsBlocked { get; set; } = false;

        public DateTime? LastLoginTime { get; set; }

        public string ThemePreference { get; set; } = "light";

        public string LanguagePreference { get; set; } = "en"; // default english

        public string? SalesforceAccountId { get; set; }


    }
}
