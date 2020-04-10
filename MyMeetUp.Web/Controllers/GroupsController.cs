using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMeetUp.Web.Data;
using MyMeetUp.Web.Models;
using MyMeetUp.Web.ViewModels;

namespace MyMeetUp.Web.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public GroupsController(ApplicationDbContext context, ILogger<GroupsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Groups
        //public async Task<IActionResult> Index() {
        //    return View(await _context.Groups.ToListAsync());
        //}

        // GET: Groups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning($"Groups/Details: id {id} no válido..");
                return NotFound();
            }

            var groupSelected = await _context.Groups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (groupSelected == null)
            {
                _logger.LogWarning($"Groups/Details: Id grupo = {id} no encontrado..");
                return NotFound();
            }
            
            var groupEventsSelected = await _context.Events.Where(e => e.GroupId == groupSelected.Id).OrderByDescending(e=>e.FechaHora).ToListAsync();
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
                foreach(GroupMembers groupMember in groupMemberProfiles) {
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
        //public IActionResult Create()
        //{
        //    return View();
        //}

        // POST: Groups/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Name,AboutUs,Country,City,CreationDate,FinalizationDate,Id")] Group @group)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(@group);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(@group);
        //}

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
