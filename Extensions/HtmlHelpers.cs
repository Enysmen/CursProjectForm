using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using CursProject.Models;

namespace CursProject.Extensions
{
    public static class HtmlHelpers
    {
        public static async Task<string> GetUserRolesAsync(this IHtmlHelper html, ApplicationUser user)
        {
            var userManager = (UserManager<ApplicationUser>)html.ViewContext.HttpContext.RequestServices.GetService(typeof(UserManager<ApplicationUser>));
            var roles = await userManager.GetRolesAsync(user);
            return string.Join(", ", roles);
        }
    }
}
