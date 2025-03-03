using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CursProject.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CursProject.Controllers
{
    // Доступ к страницам администратора только для пользователей с ролью Administrator
    [Authorize(Roles = "Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager,
                               SignInManager<ApplicationUser> signInManager,
                               RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: /Admin/Index
        [HttpGet]
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        // POST: api/admin/block
        [HttpPost("block")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Block([FromBody] string[] userIds)
        {
            bool selfBlocked = false;

            foreach (var id in userIds)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    continue;

                user.IsBlocked = true;
                await _userManager.UpdateAsync(user);

                if (User.Identity.Name == user.UserName)
                    selfBlocked = true;
            }

            if (selfBlocked)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            return Ok();
        }

        // POST: /Admin/Unblock/{id}
        [HttpPost("unblock/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unblock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.IsBlocked = false;
            await _userManager.UpdateAsync(user);

            return Ok();
        }

        // DELETE: /Admin/Delete
        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] string[] userIds)
        {
            bool selfDeleted = false;

            foreach (var id in userIds)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    continue;

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                if (User.Identity.Name == user.UserName)
                    selfDeleted = true;
            }

            if (selfDeleted)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            return Ok();
        }

        // POST: /api/admin/addadmin
        [HttpPost("addadmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdmin([FromBody] string[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
                return NotFound("Пользователи не выбраны.");

            foreach (var userId in userIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    continue;

                if (!await _roleManager.RoleExistsAsync("Administrator"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Administrator"));
                }
                await _userManager.AddToRoleAsync(user, "Administrator");
                await _signInManager.RefreshSignInAsync(user);
            }
            return Ok();
        }

        // POST: /api/admin/removeadmin
        [HttpPost("removeadmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAdmin([FromBody] string[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
                return NotFound("Пользователи не выбраны.");

            foreach (var userId in userIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    continue;

                await _userManager.RemoveFromRoleAsync(user, "Administrator");
                await _signInManager.RefreshSignInAsync(user);
            }
            return Ok();
        }

        // POST: /Admin/RelinquishAdmin
        [HttpPost("relinquishadmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RelinquishAdmin()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return NotFound();

            await _userManager.RemoveFromRoleAsync(currentUser, "Administrator");
            await _signInManager.RefreshSignInAsync(currentUser);
            return RedirectToAction("Index", "SurveyTemplate");
        }
    }
}
