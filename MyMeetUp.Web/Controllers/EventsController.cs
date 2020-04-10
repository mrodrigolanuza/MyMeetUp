using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMeetUp.Web.Data;
using MyMeetUp.Web.Models;
using MyMeetUp.Web.ViewModels;

namespace MyMeetUp.Web.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;

        public EventsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<EventsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        //// GET: Events
        //public async Task<IActionResult> Index()
        //{
        //    var applicationDbContext = _context.Events.Include(@ => @.EventCategory).Include(@ => @.Group);
        //    return View(await applicationDbContext.ToListAsync());
        //}

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) {
                _logger.LogWarning($"Detalles del Evento {id} no encontrados");
                return NotFound();
            }
                
            //Event Info
            var eventSelected = await _context.Events
                .Include(ec => ec.EventCategory)
                .Include(g => g.Group)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (eventSelected == null) {
                _logger.LogError($"Consulta de base de datos NOK sobre el Evento {id}. No encontrado.");
                return NotFound();
            }

            //SignedIn User Info
            bool signedInUserWillAttend = false;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            if(userId != null) {
                signedInUserWillAttend = _context.EventAttendances.Where(ea => ((ea.ApplicationUserId == userId.Value) && (ea.EventId == eventSelected.Id))).Any();
            }


            //List of Users Will Attend the Event
            List<ApplicationUser> eventAttendeesList = await _context.EventAttendances
                                                                    .Include(ea => ea.ApplicationUser)
                                                                    .Where(ea => ea.EventId == id)
                                                                    .Select(ea => ea.ApplicationUser)
                                                                    .ToListAsync();

            //List of Comments associated to the event
            List<EventComment> eventComments = await _context.EventComments
                                                    .Include(ec => ec.ApplicationUser)
                                                    .Where(ec => ec.EventId == id)
                                                    .ToListAsync();

            EventDetailsViewModel eventInfo = new EventDetailsViewModel { 
                EventInfo = eventSelected,
                SignedInUserWillAttend = signedInUserWillAttend,
                EventAttendees = eventAttendeesList,
                EventComments = eventComments
            };
            return View(eventInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewAttendee(string userId, int eventId) 
        {
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            Event eventSelected = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            EventAttendanceState eventAttendaceState = await _context.EventAttendanceStates.Where(ea => ea.State == "I WILL ATTEND").FirstOrDefaultAsync();
            if ((user is null) || (eventSelected is null))
                return Ok(new { success = false });

            EventAttendance eventAttendance = new EventAttendance { 
                ApplicationUser = user,
                ApplicationUserId = user.Id,
                Event = eventSelected,
                EventId = eventSelected.Id,
                EventAttendanceState = eventAttendaceState,
                EventAttendanceStateId = eventAttendaceState.Id
            };

            try {
                if (_context.EventAttendances.Any(ea => ((ea.ApplicationUserId == userId) && (ea.EventId == eventSelected.Id)))) {
                        EventAttendance actualEventAttendace = await _context.EventAttendances.Where(ea => ((ea.ApplicationUserId == userId) && (ea.EventId == eventSelected.Id))).AsNoTracking().FirstOrDefaultAsync();
                    eventAttendance.Id = actualEventAttendace.Id;
                    _context.EventAttendances.Update(eventAttendance);
                }
                else {
                    _context.EventAttendances.Add(eventAttendance);
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Usuario {userId} inscrito en evento {eventId}.");
            } catch (Exception e) {
                _logger.LogCritical($"EXCEPCIÓN: {e.Message}");
                return Ok(new { success = false });
            }

            return Ok(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveMySeat(string userId, int eventId) 
        {
            try {

                EventAttendance e = await _context.EventAttendances.FirstOrDefaultAsync(ea => ea.EventId == eventId && ea.ApplicationUserId == userId);
                _context.Remove(e);
                await _context.SaveChangesAsync();
            } catch (Exception e) {
                _logger.LogCritical($"EXCEPCIÓN: {e.Message}");
                return Json(new { success = false });
            }
            return Json(new { success = true });
        }

        //GET: Events/AttendeesList/5
        [HttpGet]
        public async Task<PartialViewResult> AttendeesList(int eventId) 
        {
            if (eventId != 0) {
                var attendeesToEvent = await _context.EventAttendances.Include(ea => ea.ApplicationUser).Where(ea => ea.EventId == eventId).ToListAsync();
                List<ApplicationUser> attendeesList = new List<ApplicationUser>();
                foreach (EventAttendance eventAttendance in attendeesToEvent) {
                    attendeesList.Add(eventAttendance.ApplicationUser);
                }
                return PartialView("_AttendeesToEvent", attendeesList);
            }
            _logger.LogError($"Lista de asistentes al evento {eventId} no encontrada.");
            return null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<PartialViewResult> NewComment(string userId, int eventId, string commentMessage) 
        {
            if (userId == "" || eventId == 0 || commentMessage == "")
                return null;

            EventComment newEventComment = new EventComment {
                ApplicationUserId = userId,
                EventId = eventId,
                Text = commentMessage,
                PublicationDate = DateTime.Now,
                ParentEventCommentId = null
            };

            try {
                _context.EventComments.Add(newEventComment);
                await _context.SaveChangesAsync();
            } 
            catch (Exception e) {
                _logger.LogCritical($"EXCEPCION en NewComment: {e.Message}");
                return null;
            }

            List<EventComment> comments = await _context.EventComments
                                                        .Include(c => c.ApplicationUser)
                                                        .Where(c => c.EventId == eventId)
                                                        .ToListAsync();
            return PartialView("_EventComments", comments);
        }

        // GET: Events/Create
        public IActionResult Create(int idGroup) {
            ViewData["EventCategoryId"] = new SelectList(_context.EventCategories, "Id", "Name");
            ViewData["GroupId"] = idGroup;
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Address,City,Country,FechaHora,GroupId,EventCategoryId,Id")] Event groupEvent) {
            if (!ModelState.IsValid) {
                ViewData["EventCategoryId"] = new SelectList(_context.EventCategories, "Id", "Name", groupEvent.EventCategoryId);
                ViewData["GroupId"] = groupEvent.GroupId;
                return View(groupEvent);
            }

            try {
                _context.Events.Add(groupEvent);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Creado nuevo evento OK >> {groupEvent.Title.ToUpper()}");
            } catch (Exception e) {
                _logger.LogCritical($"EXCEPCIÓN: {e.Message}");
            }

            return RedirectToAction("Details", "Groups", new { id = groupEvent.GroupId });
        }

        // GET: Events/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var @event = await _context.Events.FindAsync(id);
        //    if (@event == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["EventCategoryId"] = new SelectList(_context.EventCategories, "Id", "Name", @event.EventCategoryId);
        //    ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "AboutUs", @event.GroupId);
        //    return View(@event);
        //}

        // POST: Events/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Title,Description,Address,City,Country,FechaHora,GroupId,EventCategoryId,Id")] Event @event)
        //{
        //    if (id != @event.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(@event);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!EventExists(@event.Id))
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
        //    ViewData["EventCategoryId"] = new SelectList(_context.EventCategories, "Id", "Name", @event.EventCategoryId);
        //    ViewData["GroupId"] = new SelectList(_context.Groups, "Id", "AboutUs", @event.GroupId);
        //    return View(@event);
        //}

        // GET: Events/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var @event = await _context.Events
        //        .Include(@ => @.EventCategory)
        //        .Include(@ => @.Group)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (@event == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(@event);
        //}

        // POST: Events/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var @event = await _context.Events.FindAsync(id);
        //    _context.Events.Remove(@event);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool EventExists(int id)
        //{
        //    return _context.Events.Any(e => e.Id == id);
        //}
    }
}
