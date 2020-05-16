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

            //Obtener la infomación de los grupos con más categorias asociadas
            List<int> moreCategorizedGroupIds = await _context.Group_GroupCategories
                        .GroupBy(ggc => ggc.GroupId)
                        .Select(ggc => new { GroupId = ggc.Key, CategoriesAssociated = ggc.Count() })
                        .OrderByDescending(g => g.CategoriesAssociated)
                        .Select(ggc => ggc.GroupId)
                        .Take(4)
                        .ToListAsync();

            List<Group> HighlightedGroups = await _context.Groups
                            .Where(g => moreCategorizedGroupIds.Contains(g.Id))
                            .ToListAsync();

            //Obtener las categorias de grupos más utilizadas
            List<int> moreUsedGroupCategoriesIds = await _context.Group_GroupCategories
                        .GroupBy(ggc => ggc.GroupCategoryId)
                        .Select(ggc => new { GroupCategoryId = ggc.Key, NumberOfGroupsWithCategoriesAssociated = ggc.Count() })
                        .OrderByDescending(g => g.NumberOfGroupsWithCategoriesAssociated)
                        .Select(ggc => ggc.GroupCategoryId)
                        .Take(4)
                        .ToListAsync();

            List<GroupCategory> highlightedGroupCategories = await _context.GroupCategories
                                .Where(gc => moreUsedGroupCategoriesIds.Contains(gc.Id))
                                .ToListAsync();


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
