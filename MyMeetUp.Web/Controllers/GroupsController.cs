using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMeetUp.Web.Data;
using MyMeetUp.Web.Models;
using MyMeetUp.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyMeetUp.Web.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
            

        public GroupsController(ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<GroupsController> logger) {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Groups
        [Route("Groups/Index/{userId}")]
        public async Task<IActionResult> Index(string userId) {
            //if(userId !=null && userId != "")
                return View(await _context.Groups.ToListAsync());

            //return View(await _context.Groups.ContainsAsync(g=>g.Id)
        }

        // GET: Groups/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                _logger.LogWarning($"Groups/Details: id {id} no válido..");
                return NotFound();
            }

            var groupSelected = await _context.Groups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (groupSelected == null) {
                _logger.LogWarning($"Groups/Details: Id grupo = {id} no encontrado..");
                return NotFound();
            }

            var groupEventsSelected = await _context.Events.Where(e => e.GroupId == groupSelected.Id).OrderByDescending(e => e.FechaHora).ToListAsync();
            _logger.LogInformation($"Groups/Details: Id grupo = {id} encontrado.. OK");

            int numGroupMembers = _context.GroupMembers.Where(gm => gm.GroupId == groupSelected.Id).Count();
            _logger.LogInformation($"Groups/Details: Id grupo = {id} encontrado.. OK");

            var groupDetailsViewModel = new GroupDetailsViewModel
            {
                GroupInfo = groupSelected,
                GroupEvents = groupEventsSelected,
                MembersTotalNumber = numGroupMembers
            };

            return View(groupDetailsViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterNewMember(string userId, int groupId) {
            try {

                GroupMemberProfile memberProfile = await _context.GroupMemberProfiles.FirstOrDefaultAsync(gmp => gmp.Name == "MEMBER");

                GroupMembers newGroupMember = new GroupMembers
                {
                    GroupId = groupId,
                    ApplicationUserId = userId,
                    GroupMemberProfileId = memberProfile.Id
                };

                _context.GroupMembers.Add(newGroupMember);
                await _context.SaveChangesAsync();
            } catch (Exception e) {
                _logger.LogCritical($"EXCEPCIÓN: {e.Message}");
                return Json(new { success = false });
            }
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnregisterMember(string userId, int groupId) {
            try {

                List<GroupMembers> groupMemberProfiles = await _context.GroupMembers.Where(gm => gm.GroupId == groupId && gm.ApplicationUserId == userId).ToListAsync();
                foreach (GroupMembers groupMember in groupMemberProfiles) {
                    _context.Remove(groupMember);
                }
                await _context.SaveChangesAsync();
            } catch (Exception e) {
                _logger.LogCritical($"EXCEPCIÓN: {e.Message}");
                return Json(new { success = false });
            }
            return Json(new { success = true });
        }

        // GET: Groups/Create
        [HttpGet]
        public async Task<IActionResult> Create() {
            GroupCreateViewModel model = new GroupCreateViewModel
            {
                GroupCategoriesSelected = new List<int> { 1 },
                GroupCategoriesList =  GetGroupCategories()
            };
            return View(model);
        }

        private SelectList GetGroupCategories() {
            return new SelectList(_context.GroupCategories.OrderBy(gc => gc.Id), "Id", "Name");
        }

        
        // POST: Groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupCreateViewModel group) {
            int newGroupId;
            string userSignedIn;

            if (!ModelState.IsValid) {
                return View(group);
            }
            using (var dbContextTransaction = _context.Database.BeginTransaction()) {
                try {
                    await CreatesNewGroup(group);
                    newGroupId = await CreatesGroupCategories(group);
                    userSignedIn = await CreatesCoordinatorUserByDefault(newGroupId);

                    await _context.SaveChangesAsync();
                    dbContextTransaction.Commit();
                    return RedirectToAction("Index", "Groups", new { userId = userSignedIn });

                } catch (Exception e) {
                    dbContextTransaction.Rollback();
                    _logger.LogError($"EXCEPCIÓN: {e.Message}");
                    return View(group);
                }
            }
                
        }

        private async Task CreatesNewGroup(GroupCreateViewModel group) {
            Group newGroup = new Group
            {
                Name = group.GroupInfo.Name,
                AboutUs = group.GroupInfo.AboutUs,
                City = group.GroupInfo.City,
                Country = group.GroupInfo.Country,
                CreationDate = DateTime.Now,
            };
            _context.Groups.Add(newGroup);
            await _context.SaveChangesAsync();
        }

        private async Task<int> CreatesGroupCategories(GroupCreateViewModel group) {
            int newGroupId = (await _context.Groups.FirstOrDefaultAsync(g => g.Name == group.GroupInfo.Name)).Id;
            foreach (int categorySelected in group.GroupCategoriesSelected) {
                _context.Group_GroupCategories.Add(new Group_GroupCategory()
                {
                    GroupId = newGroupId,
                    GroupCategoryId = categorySelected
                });
            }
            return newGroupId;
        }

        private async Task<string> CreatesCoordinatorUserByDefault(int newGroupId) {
            string userSignedIn = (await _userManager.GetUserAsync(User))?.Id;
            int coordinatorGroupMemberProfile = (await _context.GroupMemberProfiles.FirstOrDefaultAsync(gmp => gmp.Name == GroupMemberProfilesData.Coordinator)).Id;
            _context.GroupMembers.Add(new GroupMembers()
            {
                ApplicationUserId = userSignedIn,
                GroupId = newGroupId,
                GroupMemberProfileId = coordinatorGroupMemberProfile
            });
            return userSignedIn;
        }

        // GET: Groups/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var @group = await _context.Groups.FindAsync(id);
        //    if (@group == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(@group);
        //}

        // POST: Groups/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Name,AboutUs,Country,City,CreationDate,FinalizationDate,Id")] Group @group)
        //{
        //    if (id != @group.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(@group);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!GroupExists(@group.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(@group);
        //}

        // GET: Groups/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var @group = await _context.Groups
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (@group == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(@group);
        //}

        // POST: Groups/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var @group = await _context.Groups.FindAsync(id);
        //    _context.Groups.Remove(@group);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool GroupExists(int id)
        //{
        //    return _context.Groups.Any(e => e.Id == id);
        //}
    }
}
