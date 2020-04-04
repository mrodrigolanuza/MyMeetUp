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
            _logger.LogInformation($"Groups/Details: Id grupo = {id} encontrado.. OK");
            var groupEventsSelected = await _context.Events.Where(e => e.GroupId == groupSelected.Id).OrderByDescending(e=>e.FechaHora).ToListAsync();

            _logger.LogInformation($"Groups/Details: Id grupo = {id} encontrado.. OK");
            int numGroupMembers = _context.GroupMembers.Where(gm => gm.GroupId == groupSelected.Id).Count();

            var groupDetailsViewModel = new GroupDetailsViewModel
            {
                GroupInfo = groupSelected,
                GroupEvents = groupEventsSelected,
                MembersTotalNumber = numGroupMembers
            };

            return View(groupDetailsViewModel);
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
