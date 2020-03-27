using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyMeetUp.Web.Areas.Admin.ViewModels;
using MyMeetUp.Web.Data;
using MyMeetUp.Web.Models;
using MyMeetUp.Web.Models.RoleModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyMeetUp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RolesData.Administrator)]
    public class RoleController : Controller {
        private RoleManager<IdentityRole> _roleManager;
        private UserManager<ApplicationUser> _userManager;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager) {
            _roleManager = roleManager;
            _userManager = userManager;
        }


        public IActionResult Index() {
            return View(_roleManager.Roles);
        }

        // GET: Roles/Update/5
        public async Task<IActionResult> Update(string id) {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            List<ApplicationUser> members = new List<ApplicationUser>();
            List<ApplicationUser> nonMembers = new List<ApplicationUser>();
            foreach (ApplicationUser user in _userManager.Users) {
                var list = await _userManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
                list.Add(user);
            }
            return View(new RoleEdit
            {
                Role = role,
                Members = members,
                NonMembers = nonMembers
            });
        }
        [HttpPost]
        public async Task<IActionResult> Update(RoleModification model) {
            IdentityResult result;
            if (ModelState.IsValid) {
                foreach (string userId in model.AddIds ?? new string[] { }) {
                    ApplicationUser user = await _userManager.FindByIdAsync(userId);
                    if (user != null) {
                        result = await _userManager.AddToRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
                foreach (string userId in model.DeleteIds ?? new string[] { }) {
                    ApplicationUser user = await _userManager.FindByIdAsync(userId);
                    if (user != null) {
                        result = await _userManager.RemoveFromRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
            }

            if (ModelState.IsValid)
                return RedirectToAction(nameof(Index));
            else
                return await Update(model.RoleId);
        }

        public IActionResult Create() {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(RoleCreateViewModel newRoleModel) 
        {
            if (!ModelState.IsValid)
                return View();

            if (!_roleManager.RoleExistsAsync(newRoleModel.Name).Result) {
                IdentityRole rol = new IdentityRole();
                rol.Name = newRoleModel.Name;
                var roleResult = await _roleManager.CreateAsync(rol);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(string rolename) 
        {
            var role = await _roleManager.FindByNameAsync(rolename);
            if (role != null)
                await _roleManager.DeleteAsync(role);
            
            return RedirectToAction("Index");
        }

        private void Errors(IdentityResult result) {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }
    }
}