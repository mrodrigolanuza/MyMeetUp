using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyMeetUp.Web.Configuration;
using MyMeetUp.Web.Data;
using MyMeetUp.Web.Models;
using MyMeetUp.Web.ViewModels;

namespace MyMeetUp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, IOptions<AppSettings> appSettings, ApplicationDbContext context) {
            _logger = logger;
            _appSettings = appSettings;
            _context = context;
        }

        public async Task<IActionResult> Index() {

            //Obtener la infomación de los grupos más destacados
            List<Group> HighlightedGroups = await _context.Groups.Take(4).ToListAsync();
            List<GroupCategory> highlightedGroupCategories = await _context.GroupCategories.Take(4).ToListAsync();
            HomeIndexViewModel homeIndexViewModel = new HomeIndexViewModel { 
                Groups = HighlightedGroups, 
                GroupCategories = highlightedGroupCategories
            };

            return View(homeIndexViewModel);
        }

        public IActionResult Privacy() {
            ViewBag.EmailSupport = _appSettings.Value.EmailSupport;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
