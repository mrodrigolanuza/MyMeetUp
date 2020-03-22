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
    public class EventCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventCommentsController(ApplicationDbContext context) {
            _context = context;
        }

        // GET: Admin/EventComments
        public async Task<IActionResult> Index() {
            var applicationDbContext = _context.EventComments.Include(e => e.ApplicationUser).Include(e => e.Event).Include(e => e.ParentEventComment);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/EventComments/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var eventComment = await _context.EventComments
                .Include(e => e.ApplicationUser)
                .Include(e => e.Event)
                .Include(e => e.ParentEventComment)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventComment == null) {
                return NotFound();
            }

            return View(eventComment);
        }

        // GET: Admin/EventComments/Create
        public IActionResult Create() {
            ViewBag.ApplicationUserList = new SelectList(_context.Users, "Id", "Name");
            ViewBag.EventList = new SelectList(_context.Events, "Id", "Title");
            return View();
        }

        // POST: Admin/EventComments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Text,PublicationDate,ApplicationUserId,EventId,ParentEventCommentId,Id")] EventComment eventComment) {
            if (ModelState.IsValid) {
                _context.Add(eventComment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", eventComment.ApplicationUserId);
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Address", eventComment.EventId);
            ViewData["ParentEventCommentId"] = new SelectList(_context.EventComments, "Id", "ApplicationUserId", eventComment.ParentEventCommentId);
            return View(eventComment);
        }

        // GET: Admin/EventComments/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var eventComment = await _context.EventComments.FindAsync(id);
            if (eventComment == null) {
                return NotFound();
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", eventComment.ApplicationUserId);
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Address", eventComment.EventId);
            ViewData["ParentEventCommentId"] = new SelectList(_context.EventComments, "Id", "ApplicationUserId", eventComment.ParentEventCommentId);
            return View(eventComment);
        }

        // POST: Admin/EventComments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Text,PublicationDate,ApplicationUserId,EventId,ParentEventCommentId,Id")] EventComment eventComment) {
            if (id != eventComment.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(eventComment);
                    await _context.SaveChangesAsync();
                } catch (DbUpdateConcurrencyException) {
                    if (!EventCommentExists(eventComment.Id)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", eventComment.ApplicationUserId);
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Address", eventComment.EventId);
            ViewData["ParentEventCommentId"] = new SelectList(_context.EventComments, "Id", "ApplicationUserId", eventComment.ParentEventCommentId);
            return View(eventComment);
        }

        // GET: Admin/EventComments/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            var eventComment = await _context.EventComments
                .Include(e => e.ApplicationUser)
                .Include(e => e.Event)
                .Include(e => e.ParentEventComment)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventComment == null) {
                return NotFound();
            }

            return View(eventComment);
        }

        // POST: Admin/EventComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var eventComment = await _context.EventComments.FindAsync(id);
            _context.EventComments.Remove(eventComment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventCommentExists(int id) {
            return _context.EventComments.Any(e => e.Id == id);
        }
    }
}
