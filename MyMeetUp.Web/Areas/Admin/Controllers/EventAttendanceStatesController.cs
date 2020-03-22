using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMeetUp.Web.Data;
using MyMeetUp.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MyMeetUp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RolesData.Administrator)]
    public class EventAttendanceStatesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventAttendanceStatesController(ApplicationDbContext context) {
            _context = context;
        }

        // GET: Admin/EventAttendanceStates
        public async Task<IActionResult> Index() {
            return View(await _context.EventAttendanceStates.ToListAsync());
        }

        // GET: Admin/EventAttendanceStates/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var eventAttendanceState = await _context.EventAttendanceStates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventAttendanceState == null) {
                return NotFound();
            }

            return View(eventAttendanceState);
        }

        // GET: Admin/EventAttendanceStates/Create
        public IActionResult Create() {
            return View();
        }

        // POST: Admin/EventAttendanceStates/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("State,Id")] EventAttendanceState eventAttendanceState) {
            if (ModelState.IsValid) {
                _context.Add(eventAttendanceState);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eventAttendanceState);
        }

        // GET: Admin/EventAttendanceStates/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var eventAttendanceState = await _context.EventAttendanceStates.FindAsync(id);
            if (eventAttendanceState == null) {
                return NotFound();
            }
            return View(eventAttendanceState);
        }

        // POST: Admin/EventAttendanceStates/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("State,Id")] EventAttendanceState eventAttendanceState) {
            if (id != eventAttendanceState.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(eventAttendanceState);
                    await _context.SaveChangesAsync();
                } catch (DbUpdateConcurrencyException) {
                    if (!EventAttendanceStateExists(eventAttendanceState.Id)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(eventAttendanceState);
        }

        // GET: Admin/EventAttendanceStates/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            var eventAttendanceState = await _context.EventAttendanceStates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventAttendanceState == null) {
                return NotFound();
            }

            return View(eventAttendanceState);
        }

        // POST: Admin/EventAttendanceStates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var eventAttendanceState = await _context.EventAttendanceStates.FindAsync(id);
            _context.EventAttendanceStates.Remove(eventAttendanceState);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventAttendanceStateExists(int id) {
            return _context.EventAttendanceStates.Any(e => e.Id == id);
        }
    }
}
