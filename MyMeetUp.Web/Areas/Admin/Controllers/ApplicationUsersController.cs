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

        public async Task<IActionResult> Update(string id) {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return RedirectToAction("Index");

            ApplicationUserUpdateViewModel updatedUser = new ApplicationUserUpdateViewModel();
            updatedUser.Id = user.Id;
            updatedUser.Name = user.UserName;
            updatedUser.Surname = user.Surname;
            updatedUser.Email = user.Email;
            updatedUser.LinkedIn = user.LinkedIn;
            updatedUser.DNI = user.DNI;
            updatedUser.Phone = user.Phone;
            updatedUser.City = user.City;
            updatedUser.Country = user.Country;
            
            return View(updatedUser);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ApplicationUserUpdateViewModel modelViewUser) {
            if (ModelState.IsValid) {
                ApplicationUser user = await _userManager.FindByIdAsync(modelViewUser.Id);
                if (user == null)
                    return RedirectToAction("Index");

                user = AdaptApplicationUserFromUpdateViewModel(user, modelViewUser);
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded) {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(modelViewUser);
        }

        private ApplicationUser AdaptApplicationUserFromUpdateViewModel(ApplicationUser user, ApplicationUserUpdateViewModel modelViewUser) {
            user.Id = modelViewUser.Id;
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

        [HttpPost]
        public async Task<IActionResult> Delete(string id) {
            ApplicationUser user = await _userManager.FindByIdAsync(id);

            if(user is null) {
                ModelState.AddModelError("", "Usuario no encontrado");
                return RedirectToAction("Index");
            }

            IdentityResult result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) {
                foreach (IdentityError error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            
            return RedirectToAction("Index");
        }
    }
}