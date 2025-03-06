using System.Diagnostics;
using System.Security.Claims;

using CursProject.Data;
using CursProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CursProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context )
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(string filter = "all")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            IQueryable<SurveyTemplate> allQuery = _context.SurveyTemplates.Include(t => t.User);

            if (filter == "mine")
                allQuery = allQuery.Where(t => t.UserId == userId);
            else if (filter == "others")
                allQuery = allQuery.Where(t => t.UserId != userId);

            var allTemplates = await allQuery.ToListAsync();

            // Если у вас нет популярных шаблонов в HomeController, можно задать пустой список или собрать их аналогично.
            var popularTemplates = await _context.SurveyTemplates
                .Include(t => t.Likes)
                .Include(t => t.User)
                .OrderByDescending(t => t.Likes.Count())
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
        


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
