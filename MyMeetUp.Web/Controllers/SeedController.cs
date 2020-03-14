using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyMeetUp.Web.Data;
using MyMeetUp.Web.Models;
using MyMeetUp.Web.Utils;

namespace MyMeetUp.Web.Controllers
{
    [AllowAnonymous]
    public class SeedController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _applicationDbContext;

        public SeedController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext applicationDbContext) {
            _userManager = userManager;
            _roleManager = roleManager;
            _applicationDbContext = applicationDbContext;
        }

        public IActionResult Roles() {
            IdentityInitializer.SeedRoles(_roleManager);
            return Ok("Seed Roles completed successfully");
        }

        public IActionResult Users() {
            IdentityInitializer.SeedUsers(_userManager, _applicationDbContext);
            return Ok("Seed Users completed successfully");
        }
    }
}