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

        public ViewResult Create() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ApplicationUserCreateViewModel modelViewUser) {
            if (ModelState.IsValid) {
                ApplicationUser user = AdaptApplicationUserFromCreateViewModel(modelViewUser);
                var result = await _userManager.CreateAsync(user, modelViewUser.Password);
                if (result.Succeeded)
                    return RedirectToAction("Index");
                else {
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }
            
            return View(modelViewUser);
        }

        private ApplicationUser AdaptApplicationUserFromCreateViewModel(ApplicationUserCreateViewModel modelViewUser) {
            var user = new ApplicationUser();
            user.Name = modelViewUser.Name;
            user.UserName = modelViewUser.Name;
            user.Surname = modelViewUser.Surname;
            user.Email = modelViewUser.Email;
            user.LinkedIn = modelViewUser.LinkedIn;
            user.DNI = modelViewUser.DNI;
            user.Phone = modelViewUser.Phone;
            user.City = modelViewUser.City;
            user.Country = modelViewUser.Country;
            user.EntryDate = DateTime.Now;
            return user;
        }
    }
}