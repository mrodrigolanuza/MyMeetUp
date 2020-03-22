using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyMeetUp.Web.Data;
using MyMeetUp.Web.Models;

namespace MyMeetUp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GroupMemberProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GroupMemberProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/GroupMemberProfiles
        public async Task<IActionResult> Index()
        {
            return View(await _context.GroupMemberProfiles.ToListAsync());
        }

        // GET: Admin/GroupMemberProfiles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupMemberProfile = await _context.GroupMemberProfiles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (groupMemberProfile == null)
            {
                return NotFound();
            }

            return View(groupMemberProfile);
        }

        // GET: Admin/GroupMemberProfiles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/GroupMemberProfiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Id")] GroupMemberProfile groupMemberProfile)
        {
            if (ModelState.IsValid)
            {
                _context.Add(groupMemberProfile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(groupMemberProfile);
        }

        // GET: Admin/GroupMemberProfiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupMemberProfile = await _context.GroupMemberProfiles.FindAsync(id);
            if (groupMemberProfile == null)
            {
                return NotFound();
            }
            return View(groupMemberProfile);
        }

        // POST: Admin/GroupMemberProfiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Id")] GroupMemberProfile groupMemberProfile)
        {
            if (id != groupMemberProfile.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(groupMemberProfile);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupMemberProfileExists(groupMemberProfile.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(groupMemberProfile);
        }

        // GET: Admin/GroupMemberProfiles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupMemberProfile = await _context.GroupMemberProfiles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (groupMemberProfile == null)
            {
                return NotFound();
            }

            return View(groupMemberProfile);
        }

        // POST: Admin/GroupMemberProfiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var groupMemberProfile = await _context.GroupMemberProfiles.FindAsync(id);
            _context.GroupMemberProfiles.Remove(groupMemberProfile);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupMemberProfileExists(int id)
        {
            return _context.GroupMemberProfiles.Any(e => e.Id == id);
        }
    }
}
