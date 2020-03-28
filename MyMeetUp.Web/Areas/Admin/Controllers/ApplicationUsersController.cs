using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyMeetUp.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyMeetUp.Web.Data;
using MyMeetUp.Web.Models;
using Microsoft.AspNetCore.Authorization;

namespace MyMeetUp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RolesData.Administrator)]
    public class ApplicationUsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        private IPasswordHasher<ApplicationUser> _passwordHasher;

        public ApplicationUsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IPasswordHasher<ApplicationUser> passwordHasher) {
            _userManager = userManager;
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public IActionResult Index() {
            return View(FechIndexViewUserList());
        }

        private List<ApplicationUserIndexViewModel> FechIndexViewUserList() {
            List<ApplicationUserIndexViewModel> Users = new List<ApplicationUserIndexViewModel>();
            foreach (ApplicationUser user in _userManager.Users) {
                Users.Add(new ApplicationUserIndexViewModel()
                {
                    Id = user.Id,
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    LinkedIn = user.LinkedIn,
                    DNI = user.DNI,
                    Phone = user.Phone,
                    City = user.City,
                    Country = user.Country,
                    EntryDate = user.EntryDate,
                    LeavingDate = user.LeavingDate
                });
            }
            return Users;
        }
    }
}