using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyMeetUp.Web.Data;
using MyMeetUp.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MyMeetUp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RolesData.Administrator)]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context) {
            _context = context;
        }

        // GET: Admin/Events
        public async Task<IActionResult> Index() {
            var applicationDbContext = _context.Events.Include(ec => ec.EventCategory).Include(g => g.Group);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/Events/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(ec => ec.EventCategory)
                .Include(g => g.Group)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null) {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Admin/Events/Create
        public IActionResult Create() {
            ViewData["EventCategoryId"] = new SelectList(_context.EventCategories, "Id", "Name");
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "AboutUs");
            return View();
        }

        // POST: Admin/Events/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Address,City,Country,FechaHora,GroupId,EventCategoryId,Id")] Event @event) {
            if (ModelState.IsValid) {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventCategoryId"] = new SelectList(_context.EventCategories, "Id", "Name", @event.EventCategoryId);
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "AboutUs", @event.GroupId);
            return View(@event);
        }

        // GET: Admin/Events/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) {
                return NotFound();
            }
            ViewData["EventCategoryId"] = new SelectList(_context.EventCategories, "Id", "Name", @event.EventCategoryId);
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "AboutUs", @event.GroupId);
            return View(@event);
        }

        // POST: Admin/Events/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Description,Address,City,Country,FechaHora,GroupId,EventCategoryId,Id")] Event @event) {
            if (id != @event.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                } catch (DbUpdateConcurrencyException) {
                    if (!EventExists(@event.Id)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventCategoryId"] = new SelectList(_context.EventCategories, "Id", "Name", @event.EventCategoryId);
            ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "AboutUs", @event.GroupId);
            return View(@event);
        }

        // GET: Admin/Events/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(ec => ec.EventCategory)
                .Include(g => g.Group)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null) {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Admin/Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var @event = await _context.Events.FindAsync(id);
            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id) {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
