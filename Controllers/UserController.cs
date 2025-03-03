using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CursProject.Data;
using CursProject.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using System.Text.RegularExpressions;
using System.Globalization;
using CursProject.Service;

namespace CursProject.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SalesforceService _salesforceService;
        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SalesforceService salesforceService)
        {
            _context = context;
            _userManager = userManager;
            _salesforceService = salesforceService;
        }

        // GET: /User/Index
        public async Task<IActionResult> Index(string userId = null)
        {
            bool isAdmin = User.IsInRole("Administrator");
            string selectedUserId;

            if (isAdmin)
            {
                if (string.IsNullOrEmpty(userId))
                {
                    var allUsers = await _context.Users.ToListAsync();
                    if (allUsers.Any())
                    {
                        selectedUserId = allUsers.OrderBy(u => Guid.NewGuid()).First().Id;
                    }
                    else
                    {
                        return View(new UserDashboardViewModel());
                    }
                }
                else
                {
                    selectedUserId = userId;
                }
            }
            else
            {
                selectedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            // Load templates created by the selected user
            var myTemplates = await _context.SurveyTemplates
                .Where(t => t.UserId == selectedUserId)
                .OrderBy(t => t.Title)
                .ToListAsync();

            // Load all responses for templates where the template creator is equal to the selected user
            var allResponses = await _context.SurveyResponses
                .Include(r => r.SurveyTemplate)
                .Include(r => r.User)
                .Where(r => r.SurveyTemplate.UserId == selectedUserId)
                .ToListAsync();

            // Group responses by SurveyTemplateId and select the last one from each group by date of completion
            var myResponses = allResponses
                .GroupBy(r => r.SurveyTemplateId)
                .Select(g => g.OrderByDescending(r => r.SubmittedDate).First())
                .OrderBy(r => r.SurveyTemplate.Title)
                .ToList();

            var allUsersList = isAdmin
                ? await _context.Users.ToListAsync()
                : null;

            var model = new UserDashboardViewModel
            {
                MyTemplates = myTemplates,
                MyResponses = myResponses,
                AllUsers = allUsersList,
                SelectedUserId = selectedUserId
            };

            ViewBag.IsAdmin = isAdmin;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateThemePreference(string theme)
        {
            // Valid values: "light" and "dark"
            if (string.IsNullOrEmpty(theme) || (theme.ToLower() != "light" && theme.ToLower() != "dark"))
            {
                return BadRequest("Invalid theme value.");
            }

            // If the user is not logged in, skip the update (or return Ok)
            if (!User.Identity.IsAuthenticated)
            {
                return Ok(new { success = true });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            user.ThemePreference = theme.ToLower();
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return StatusCode(500, "Could not update theme preference.");
            }
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLanguagePreference(string language)
        {
            
            if (string.IsNullOrEmpty(language) || (language.ToLower() != "en" && language.ToLower() != "ru"))
            {
                return BadRequest("Invalid language value.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            user.LanguagePreference = language.ToLower();
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return StatusCode(500, "Could not update language preference.");
            }

            return Json(new { success = true });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (string.IsNullOrEmpty(culture) || (culture.ToLower() != "en" && culture.ToLower() != "ru"))
            {
                culture = "en"; 
            }
            try
            {

                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddMinutes(30) }
                );

                var uri = new Uri(returnUrl);
                var localPath = uri.PathAndQuery; 


                return LocalRedirect(localPath);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSalesforceAccount(SalesforceIntegrationViewModel model, [FromServices] SalesforceService salesforceService)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Аутентификация в Salesforce: получаем access token и instance URL
            var (accessToken, instanceUrl) = await salesforceService.AuthenticateAsync();

            // Подготовка данных для создания Account. Используем FullName из модели как имя аккаунта.
            var accountData = new Dictionary<string, object>
             {
                { "Name", model.FullName }
                // При необходимости можно добавить дополнительные поля, например, Email или другие данные.
            };

            var success = await salesforceService.CreateAccountAsync(instanceUrl, accessToken, accountData);
            if (success)
            {
                TempData["Message"] = "Account успешно создан в Salesforce!";
                return RedirectToAction("Index", "User"); // перенаправляем на главную страницу профиля
            }
            else
            {
                TempData["Error"] = "Ошибка при создании Account в Salesforce.";
                return View(model);
            }
        }

        // GET: /Salesforce/Integration
        [HttpGet]
        public async Task<IActionResult> Integration()
        {
            // Получаем текущего пользователя
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            // Пример получения данных о формах (если нет — используйте тестовые данные)
            var forms = new List<string> { "Форма 1", "Форма 2" };

            var model = new SalesforceIntegrationViewModel
            {
                FullName = user?.FullName,     // Из вашей модели ApplicationUser
                Email = user?.Email,           // Email должен быть в IdentityUser
                FormsCount = forms.Count,
                FormNames = forms
            };

            return View(model);
        }

        // POST: /Salesforce/Integration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Integration(SalesforceIntegrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Аутентификация в Salesforce через сервис
            var (accessToken, instanceUrl) = await _salesforceService.AuthenticateAsync();

            // Подготовка данных для создания Account в Salesforce
            // Используем FullName как имя аккаунта; при необходимости можно комбинировать с Email
            var accountData = new Dictionary<string, object>
        {
            { "Name", model.FullName }  
            // Дополнительно можно передать email, если объект Account имеет такое поле, или другие данные
        };

            var success = await _salesforceService.CreateAccountAsync(instanceUrl, accessToken, accountData);
            if (success)
            {
                TempData["Message"] = "Запись успешно создана в Salesforce!";
                return RedirectToAction("Index", "User"); // Перенаправляем, например, на страницу профиля
            }
            else
            {
                TempData["Error"] = "Ошибка при создании записи в Salesforce.";
                return View(model);
            }
        }
    }

}

