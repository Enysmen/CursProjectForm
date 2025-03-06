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
using Microsoft.IdentityModel.Tokens;
using System.Net.Http;

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


        // GET: /Salesforce/Integration
        [HttpGet]
        public async Task<IActionResult> Integration()
        {
           
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            

            var model = new SalesforceIntegrationViewModel
            {
                
                FullName = "",  
                Email = "",     

                
                ContactFullName = user?.FullName ?? "",
                ContactEmail = user?.Email ?? "",
                ContactPhone = user?.PhoneNumber ?? "",
                ContactTitle = ""
            };

            return View(model);
        }

        // POST: /Salesforce/Integration 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Integration(SalesforceIntegrationViewModel model)
        {
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (ModelState.IsValid)
            {
                return View(model);
            }

           
            if (!string.IsNullOrEmpty(user?.SalesforceAccountId))
            {
                TempData["Error"] = "Account has already been created in Salesforce.";
                return RedirectToAction("Integration");
            }

            
            var (accessToken, instanceUrl) = await _salesforceService.AuthenticateAsync();

            
            var accountData = new Dictionary<string, object>
            {
                { "Name", model.FullName },
                { "Email__c", model.Email } 
            };

            
            var accountId = await _salesforceService.CreateAccountAsync(instanceUrl, accessToken, accountData);
            if (!string.IsNullOrEmpty(accountId))
            {
                TempData["Message"] = "Account successfully created in Salesforce!";

                if (user != null)
                {
                    user.SalesforceAccountId = accountId;
                    await _userManager.UpdateAsync(user);
                }
                return RedirectToAction("Index", "User"); 
            }
            else
            {
                TempData["Error"] = "Error creating Account in Salesforce.";
                return View(model);
            }
        }

       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccountAndContact(SalesforceIntegrationViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (!string.IsNullOrEmpty(user?.SalesforceAccountId))
            {
                TempData["Error"] = "Integration is already done. Records are already created.";
                return RedirectToAction("Integration");
            }

            if (!ModelState.IsValid)
            {
                return View("Integration", model);
            }

            
            var (accessToken, instanceUrl) = await _salesforceService.AuthenticateAsync();
            var accountData = new Dictionary<string, object>
            {
                { "Name", model.FullName },
                { "Email__c", model.Email } 
            };

            var accountId = await _salesforceService.CreateAccountAsync(instanceUrl, accessToken, accountData);
            if (string.IsNullOrEmpty(accountId))
            {
                TempData["Error"] = "Error creating Account in Salesforce.";
                return View("Integration", model);
            }

            
            user.SalesforceAccountId = accountId;
            await _userManager.UpdateAsync(user);

            
            var contactData = new Dictionary<string, object>
            {
                { "LastName", model.ContactFullName }, 
                { "Email", model.ContactEmail },
                { "Phone", model.ContactPhone },
                { "Title", model.ContactTitle },
                { "AccountId", accountId }
            };

            var contactId = await _salesforceService.CreateContactAsync(instanceUrl, accessToken, contactData);
            if (string.IsNullOrEmpty(contactId))
            {
                TempData["Error"] = "Error creating Contact in Salesforce.";
                return View("Integration", model);
            }
            else
            {
                TempData["Message"] = "Account and Contact have been successfully created in Salesforce!";
                return RedirectToAction("Integration");
            }
        }

        // GET: /Salesforce/EditIntegration
        [HttpGet]
        public async Task<IActionResult> EditIntegration()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || string.IsNullOrEmpty(user.SalesforceAccountId))
            {
                TempData["Error"] = "There is no generated data in Salesforce to edit.";
                return RedirectToAction("Integration");
            }

            
            var (accessToken, instanceUrl) = await _salesforceService.AuthenticateAsync();
            var accountData = await _salesforceService.GetAccountDataAsync(instanceUrl, accessToken, user.SalesforceAccountId);
            Dictionary<string, object>? contactData = await _salesforceService.GetContactByAccountIdAsync(instanceUrl, accessToken, user.SalesforceAccountId);

            var model = new SalesforceIntegrationViewModel
            {
                FullName = accountData.ContainsKey("Name") ? accountData["Name"].ToString() : "",
                Email = accountData.ContainsKey("Email__c") ? accountData["Email__c"].ToString() : "",

                ContactFullName = contactData.ContainsKey("LastName") ? contactData["LastName"].ToString() : "",
                ContactEmail = contactData.ContainsKey("Email") ? contactData["Email"].ToString() : "",
                ContactPhone = contactData.ContainsKey("Phone") ? contactData["Phone"].ToString() : "",
                ContactTitle = contactData.ContainsKey("Title") ? contactData["Title"].ToString() : ""
            };

            return View(model);
        }

        // POST: /Salesforce/EditIntegration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditIntegration(SalesforceIntegrationViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || string.IsNullOrEmpty(user.SalesforceAccountId))
            {
                TempData["Error"] = "There is no data created to edit.";
                return RedirectToAction("Integration");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            
            var (accessToken, instanceUrl) = await _salesforceService.AuthenticateAsync();

            
            var accountData = new Dictionary<string, object>
            {
                { "Name", model.FullName },
                { "Email__c", model.Email }
            };

            bool accountUpdated = await _salesforceService.UpdateAccountAsync(instanceUrl, accessToken, user.SalesforceAccountId, accountData);
            if (!accountUpdated)
            {
                TempData["Error"] = "Error updating Account in Salesforce.";
                return View(model);
            }

            
            var contactDt = await _salesforceService.GetContactByAccountIdAsync(instanceUrl, accessToken, user.SalesforceAccountId);
            string contactId = contactDt.ContainsKey("Id") ? contactDt["Id"].ToString() : null;
            var contactData = new Dictionary<string, object>
            {
                { "LastName", model.ContactFullName },
                { "Email", model.ContactEmail },
                { "Phone", model.ContactPhone },
                { "Title", model.ContactTitle }
                
            };

            bool contactOperationSuccess;
            if (!string.IsNullOrEmpty(contactId))
            {
                
                contactOperationSuccess = await _salesforceService.UpdateContactAsync(instanceUrl, accessToken, contactId, contactData);
            }
            else
            {
                
                contactId = await _salesforceService.CreateContactAsync(instanceUrl, accessToken, contactData);
                contactOperationSuccess = !string.IsNullOrEmpty(contactId);
            }

            if (!contactOperationSuccess)
            {
                TempData["Error"] = "Error updating/creating Contact in Salesforce.";
                return View(model);
            }

            TempData["Message"] = "Data successfully updated in Salesforce!";
            return RedirectToAction("EditIntegration");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteIntegration()
        {
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || string.IsNullOrEmpty(user.SalesforceAccountId))
            {
                TempData["Error"] = "There is no created Account to delete.";
                return RedirectToAction("Integration");
            }

            
            var (accessToken, instanceUrl) = await _salesforceService.AuthenticateAsync();

            
            var contactDt = await _salesforceService.GetContactByAccountIdAsync(instanceUrl, accessToken, user.SalesforceAccountId);
            string contactId = contactDt.ContainsKey("Id") ? contactDt["Id"].ToString() : null;
            bool contactDeleted = true;
            if (!string.IsNullOrEmpty(contactId))
            {
                contactDeleted = await _salesforceService.DeleteContactAsync(instanceUrl, accessToken, contactId);
            }

            
            bool accountDeleted = await _salesforceService.DeleteAccountAsync(instanceUrl, accessToken, user.SalesforceAccountId);

            if (accountDeleted && contactDeleted)
            {
                TempData["Message"] = "Account and Contact have been successfully removed from Salesforce!";
                
                user.SalesforceAccountId = null;
                await _userManager.UpdateAsync(user);
            }
            else
            {
                TempData["Error"] = "Error deleting records from Salesforce.";
            }

            return RedirectToAction("Integration");
        }

    }

}

