using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CursProject.Data;
using CursProject.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Identity;

namespace CursProject.Controllers
{
    public class SurveyTemplateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SurveyTemplateController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /SurveyTemplate/Index?filter=all|mine|others
        
        public async Task<IActionResult> Index(string filter = "all")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get all templates with creator included
            IQueryable<SurveyTemplate> allQuery = _context.SurveyTemplates.Include(t => t.User);
            if (filter == "mine")
                allQuery = allQuery.Where(t => t.UserId == userId);
            else if (filter == "others")
                allQuery = allQuery.Where(t => t.UserId != userId);
            var allTemplates = await allQuery.ToListAsync();


            // Popular templates – top 5 by number of completed forms
            var popularTemplates = await _context.SurveyTemplates
                    .Include(t => t.Responses)
                    .Include(t => t.Likes)
                    .Include(t => t.User)
                    .OrderByDescending(t => t.Responses.Count())
                    .Take(5)
                    .ToListAsync();

            var model = new SurveyTemplateIndexViewModel
            {
                PopularTemplates = popularTemplates,
                AllTemplates = allTemplates,
                Filter = filter
            };



            return View(model);

        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var template = await _context.SurveyTemplates
                .Include(t => t.User)
                .Include(t => t.Comments).ThenInclude(c => c.User)
                .Include(t => t.Likes)
                .Include(t => t.Responses).ThenInclude(r => r.User)
                .Include(t => t.AllowedUsers)
                .FirstOrDefaultAsync(t => t.Id == id);


            var currentUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.TemplateId = template.Id;
            ViewBag.LikesCount = template.Likes?.Count() ?? 0;
            ViewBag.UserLiked = (currentUserId != null) && (template.Likes != null && template.Likes.Any(l => l.UserId == currentUserId));
            ViewBag.Comments = template.Comments?.OrderBy(c => c.CreatedDate).ToList();
            ViewBag.JsonSchema = template.JsonSchema;
            ViewBag.AllUsers = await _context.Users.ToListAsync();


            ViewBag.AdminUserEmails = (from user in _context.Users
                                       join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                       join role in _context.Roles on userRole.RoleId equals role.Id
                                       where role.Name == "Administrator"
                                       select user.Email.ToLower())
                                       .ToList();


            if (template == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(template.JsonSchema))
            {
                return Content("Ошибка: шаблон не содержит JSON-схему.");
            }

           

            return View(template);
        }

        // GET: /SurveyTemplate/Create
        [Authorize]
        public async Task<IActionResult> Create(int? cloneTemplateId)
        {
            string initialSchema; 
            var allUsers = await _context.Users.ToListAsync();
            ViewBag.AllUsers = allUsers;

            // Get the list of administrators (using UserManager if you have one)
            var adminUsers = await _userManager.GetUsersInRoleAsync("Administrator");
            ViewBag.AdminUserEmails = adminUsers.Select(u => u.Email.ToLower()).ToList();

            if (cloneTemplateId.HasValue)
            {
                var templateToClone = await _context.SurveyTemplates.FindAsync(cloneTemplateId.Value);
                if (templateToClone != null)
                {
                    
                    initialSchema = templateToClone.JsonSchema;
                }
                else
                {
                    initialSchema = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        title = "New Poll",
                        description = "Enter a description of the survey",
                        elements = new object[] { }
                    });
                }
            }
            else
            {
                initialSchema = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    title = "New Poll",
                    description = "Enter a description of the survey",
                    elements = new object[] { }
                });
            }

            var model = new SurveyTemplate
            {
                CreatedDate = DateTime.UtcNow,
                IsPublic = true,
                JsonSchema = initialSchema
            };



            return View(model);
        }

        // POST: /SurveyTemplate/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SurveyTemplate surveyTemplate, string AllowedUserIds)
        {
            ModelState.Remove("Title");
            ModelState.Remove("Description");

            if (ModelState.IsValid)
            {
                return View(surveyTemplate);
            }
            ViewBag.AllUsers = await _context.Users.ToListAsync();
            
            surveyTemplate.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            surveyTemplate.CreatedDate = DateTime.UtcNow;

            if (!surveyTemplate.IsPublic && AllowedUserIds != null && AllowedUserIds.Any())
            {

                var allowedUserIds = AllowedUserIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                surveyTemplate.AllowedUsers = new List<ApplicationUser>();
                foreach (var userId in allowedUserIds)
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        surveyTemplate.AllowedUsers.Add(user);
                    }
                }
            }

            try
            {
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(surveyTemplate.JsonSchema);
                surveyTemplate.Title = jsonObj.title != null ? (string)jsonObj.title : surveyTemplate.Title;
                surveyTemplate.Description = jsonObj.description != null ? (string)jsonObj.description : surveyTemplate.Description;
            }
            catch
            {

            }

            _context.Add(surveyTemplate);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /SurveyTemplate/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var surveyTemplate = await _context.SurveyTemplates
                .Include(t => t.User)
                .Include(t => t.Comments).ThenInclude(c => c.User)
                .Include(t => t.Likes)
                .Include(t => t.Responses).ThenInclude(r => r.User)
                .Include(t => t.AllowedUsers)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (surveyTemplate == null)
                return NotFound();



            var currentUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var adminUsers = await _userManager.GetUsersInRoleAsync("Administrator");
            ViewBag.AdminUserEmails = adminUsers.Select(u => u.Email.ToLower()).ToList();
            ViewBag.TemplateId = surveyTemplate.Id;
            ViewBag.JsonSchema = surveyTemplate.JsonSchema;
            ViewBag.AllUsers = await _context.Users.ToListAsync();



            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!User.IsInRole("Administrator") && surveyTemplate.UserId != userId && (surveyTemplate.AllowedUsers == null || !surveyTemplate.AllowedUsers.Any(u => u.Id == userId)))
            {
                return Forbid();
            }

            return View(surveyTemplate);
        }

        // POST: /SurveyTemplate/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SurveyTemplate surveyTemplate , string AllowedUserIds)
        {
            if (id != surveyTemplate.Id)
                return NotFound();

            ModelState.Remove("Title");
            ModelState.Remove("Description");

            if (!ModelState.IsValid)
            {
                try
                {
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    var existingTemplate = await _context.SurveyTemplates
                        .Include(t => t.AllowedUsers)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == id);
                    if (existingTemplate == null)
                        return NotFound();

                    // If the current user is not an administrator or owner, disable editing
                    if (!User.IsInRole("Administrator") && existingTemplate.UserId != currentUserId)
                        return Forbid();

                    // Attempt to parse JSON schema and update Title and Description
                    try
                    {
                        dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(surveyTemplate.JsonSchema);
                        surveyTemplate.Title = jsonObj.title != null ? (string)jsonObj.title : "";
                        surveyTemplate.Description = jsonObj.description != null ? (string)jsonObj.description : "";
                    }
                    catch
                    {
                        surveyTemplate.Title = "New Poll";
                        surveyTemplate.Description = "Enter a description of the survey";
                    }

                    // Keep the owner and creation date fields unchanged
                    surveyTemplate.UserId = existingTemplate.UserId;
                    surveyTemplate.CreatedDate = existingTemplate.CreatedDate;

                    // Processing the list of allowed users:
                    if (!surveyTemplate.IsPublic)
                    {
                        // Initialize or clear the AllowedUsers collection
                        if (existingTemplate.AllowedUsers == null)
                        {
                            surveyTemplate.AllowedUsers = new List<ApplicationUser>();
                        }
                        else
                        {
                            surveyTemplate.AllowedUsers = new List<ApplicationUser>();
                        }
                        // If there is data in AllowedUserIds, split the string and add users
                        if (!string.IsNullOrEmpty(AllowedUserIds))
                        {
                            var allowedIds = AllowedUserIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var userId in allowedIds)
                            {
                                var user = await _context.Users.FindAsync(userId);
                                if (user != null)
                                {
                                    surveyTemplate.AllowedUsers.Add(user);
                                }
                            }
                        }
                    }
                    else
                    {
                        // If the template is public, clear the list of allowed users
                        surveyTemplate.AllowedUsers?.Clear();
                    }

                    
                    _context.Update(surveyTemplate);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "SurveyTemplate");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SurveyTemplateExists(surveyTemplate.Id))
                        return NotFound();
                    else
                        throw;
                }
            }
            return View(surveyTemplate);
        }

        [Authorize]
        private bool SurveyTemplateExists(int id)
        {
            return _context.SurveyTemplates.Any(e => e.Id == id);
        }

        // POST: /SurveyTemplate/SelectPopularTemplate
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult SelectPopularTemplate(int? selectedTemplateId)
        {
            if (selectedTemplateId == null)
                return RedirectToAction("Create");
            return RedirectToAction("Create", new { cloneTemplateId = selectedTemplateId });
        }

        // POST: /SurveyTemplate/ProcessTemplate
        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessTemplate(int? selectedTemplateId, string actionType, string filter)
        {

            
            TempData["Filter"] = filter;
            
            if (selectedTemplateId == null && string.IsNullOrEmpty(actionType))
            {
                return RedirectToAction("Index", new { filter = filter });
            }

            if (selectedTemplateId == null)
            {
                if (actionType == "details")
                {
                    // If we try to view a template and it is not selected, we display an error message and return to Index
                    TempData["ErrorMessage"] = "No template selected.";
                    return RedirectToAction("Index", new { filter = filter });
                }
                else
                {
                    // For other actions, redirect to the new template creation page
                    return RedirectToAction("Create");
                }
            }

            var template = await _context.SurveyTemplates
                        .Include(t => t.AllowedUsers)
                        .FirstOrDefaultAsync(t => t.Id == selectedTemplateId);

            if (template == null)
            {
                return NotFound();
            }

            if (actionType == "create")
            {
                if (selectedTemplateId != null)
                {
                    return RedirectToAction("Create", new { cloneTemplateId = selectedTemplateId });
                }
                else
                {
                    return RedirectToAction("Create");
                }
            }

            // If the action is "details" - redirect to the template view page (Details.cshtml)
            if (actionType == "details")
            {
                return RedirectToAction("Details", new { id = selectedTemplateId });
            }

            // For the rest of the actions, we check that the user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

          
            if (actionType == "fill")
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!template.IsPublic)
                {

                    if (!User.IsInRole("Administrator") &&
                        !((template.AllowedUsers != null && template.AllowedUsers.Any(u => u.Id == currentUserId)) ||
                          template.UserId == currentUserId))
                    {
                        TempData["ErrorMessage"] = "You do not have access to complete this form.";
                        return RedirectToAction("Index", new { filter = filter });
                    }
                }
                return RedirectToAction("Fill", "Survey", new { id = selectedTemplateId });
            }
           
            else if (actionType == "edit")
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!User.IsInRole("Administrator") && !(template.UserId == currentUserId || (template.AllowedUsers != null && template.AllowedUsers.Any(u => u.Id == currentUserId))))
                {
                    TempData["ErrorMessage"] = "You do not have permission to edit this form.";
                    return RedirectToAction("Index", new { filter = filter });
                }
                return RedirectToAction("Edit", new { id = selectedTemplateId });
            }
 
            return RedirectToAction("Index", new { filter = filter });
        }

        [Authorize]
        public async Task<IActionResult> Responses(int id)
        {
            var template = await _context.SurveyTemplates
                .Include(t => t.Responses)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (template == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (template.UserId != userId && !User.IsInRole("Administrator"))
            {
                TempData["ErrorMessage"] = "You do not have permission to view responses to this form.";
                return RedirectToAction("Index");
            }
            return View(template);
        }

        // POST: /SurveyTemplate/Delete/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Load the template along with the associated data (likes, comments, replies)
            var template = await _context.SurveyTemplates
                    .Include(t => t.User)
                    .Include(t => t.Comments).ThenInclude(c => c.User)
                    .Include(t => t.Likes)
                    .Include(t => t.Responses).ThenInclude(r => r.User)
                    .Include(t => t.AllowedUsers)
                    .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
                return NotFound();

            // Check if the current user is the owner of the template,
            // if not, then allow deletion only to the administrator
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Administrator") && template.UserId != userId && (template.AllowedUsers == null || !template.AllowedUsers.Any(u => string.Equals(u.Id, userId, StringComparison.OrdinalIgnoreCase))))
            {
                return Forbid();
            }

            // Remove related objects using null check
            if ((template.Likes ?? Enumerable.Empty<TemplateLike>()).Any())
            {
                _context.TemplateLikes.RemoveRange(template.Likes);
            }
            if ((template.Comments ?? Enumerable.Empty<TemplateComment>()).Any())
            {
                _context.TemplateComments.RemoveRange(template.Comments);
            }
            if ((template.Responses ?? Enumerable.Empty<SurveyResponse>()).Any())
            {
                _context.SurveyResponses.RemoveRange(template.Responses);
            }

            // Удаляем сам шаблон
            _context.SurveyTemplates.Remove(template);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> GetComments(int templateId)
        {
            var comments = await _context.TemplateComments
                .Include(c => c.User) 
                .Where(c => c.SurveyTemplateId == templateId)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();
            return PartialView("_CommentsPartial", comments);
        }

        [Authorize]
        public async Task<IActionResult> AggregatedResponses(int id, string userId = null)
        {
            // Загружаем шаблон с ответами
            var template = await _context.SurveyTemplates
                .Include(t => t.Responses)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (template == null)
                return NotFound();

            // Если передан userId, фильтруем ответы по пользователю
            var responses = template.Responses.AsQueryable();
            if (!string.IsNullOrEmpty(userId))
            {
                responses = responses.Where(r => r.UserId == userId);
            }
            var responseList = await responses.ToListAsync();

            
            
            var model = new AggregatedResponsesViewModel
            {
                Template = template,
                Responses = responseList
            };

            return View(model);
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserToTemplate(int templateId, string userId)
        {
            // Load the template along with the AllowedUsers collection
            var template = await _context.SurveyTemplates
                .Include(t => t.AllowedUsers)
                .FirstOrDefaultAsync(t => t.Id == templateId);
            if (template == null)
            {
                return NotFound();
            }

            
            if (template.AllowedUsers == null)
            {
                template.AllowedUsers = new List<ApplicationUser>();
            }

            // Check if the selected user has already been added
            if (!template.AllowedUsers.Any(u => u.Id == userId))
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }
                template.AllowedUsers.Add(user);
                await _context.SaveChangesAsync();
            }

            
            return RedirectToAction("Details", new { id = templateId });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTemplate(int id, string jsonSchema, bool isPublic, string AllowedUserIds)
        {
            var template = await _context.SurveyTemplates
                        .Include(t => t.AllowedUsers)
                        .FirstOrDefaultAsync(t => t.Id == id);
            if (template == null)
            {
                return NotFound();
            }

            
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Administrator") &&
                template.UserId != currentUserId &&
                (template.AllowedUsers == null || !template.AllowedUsers.Any(u => u.Id == currentUserId)))
            {
                return Forbid();
            }

            
            template.JsonSchema = jsonSchema;
            template.IsPublic = isPublic;

            
            if (!isPublic)
            {
                
                if (template.AllowedUsers == null)
                {
                    template.AllowedUsers = new List<ApplicationUser>();
                }
                else
                {
                    // If the list is already filled, clear it to reset the current values
                    template.AllowedUsers.Clear();
                }

                if (!string.IsNullOrEmpty(AllowedUserIds))
                {
                    var allowedIds = AllowedUserIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var userId in allowedIds)
                    {
                        var user = await _context.Users.FindAsync(userId);
                        if (user != null)
                        {
                            template.AllowedUsers.Add(user);
                        }
                    }
                }
            }
            else
            {
                // If the template is public, clear the list of allowed users
                template.AllowedUsers?.Clear();
            }

            _context.Update(template);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        [Authorize]
        public async Task<IActionResult> ViewResponse(int id)
        {
            // Load the response along with the template (to get the JSON schema)
            var response = await _context.SurveyResponses
                .Include(r => r.SurveyTemplate)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (response == null)
                return NotFound();

            return View("Responses", response);
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FindUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email cannot be empty.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound("User with this email not found.");
            }

            
            return Json(new { userId = user.Id, email = user.Email, userName = user.UserName });
        }
    }
}
