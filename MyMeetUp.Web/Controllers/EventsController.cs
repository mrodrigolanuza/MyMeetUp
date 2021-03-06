﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMeetUp.Web.Data;
using MyMeetUp.Web.Models;
using MyMeetUp.Web.Services.Interfaces;
using MyMeetUp.Web.ViewModels;
using Microsoft.ApplicationInsights;

namespace MyMeetUp.Web.Controllers
{
    public class EventsController : Controller
    {
        private const string WILL_ATTEND = "I WILL ATTEND";
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;
        private readonly IQueueService _eventQueueService;
        private TelemetryClient _telemetryClient;

        public EventsController(ApplicationDbContext context, 
                                UserManager<ApplicationUser> userManager, 
                                ILogger<EventsController> logger,
                                IQueueService eventQueueService,
                                TelemetryClient telemetry)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _eventQueueService = eventQueueService;
            _telemetryClient = telemetry;
        }

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

            var dictionay = new Dictionary<string, string>();
            dictionay.Add("Event Details", eventSelected.Title);
            _telemetryClient.TrackEvent("UserInteraction", dictionay);
            
            return View(eventInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewAttendee(string userId, int eventId) 
        {
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            Event eventSelected = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            EventAttendanceState eventAttendaceState = await _context.EventAttendanceStates.Where(ea => ea.State == WILL_ATTEND).FirstOrDefaultAsync();
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

            var dictionay = new Dictionary<string, string>();
            dictionay.Add("Event New Attendee", eventAttendance.ApplicationUser.Name);
            _telemetryClient.TrackEvent("UserInteraction", dictionay);

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

            var dictionay = new Dictionary<string, string>();
            dictionay.Add("Event Leave My Seat", $"userId : {userId}");
            _telemetryClient.TrackEvent("UserInteraction", dictionay);

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

                var dictionay = new Dictionary<string, string>();
                dictionay.Add("Event New Comment", $"eventId: {eventId}");
                _telemetryClient.TrackEvent("UserInteraction", dictionay);
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
            ViewData["GroupName"] = _context.Groups.FirstOrDefaultAsync(g => g.Id == idGroup).Result.Name;
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
                await SendNewEventMessageToEventQueue(groupEvent);
                _logger.LogInformation($"Creado nuevo evento OK >> {groupEvent.Title.ToUpper()}");
            } catch (Exception e) {
                _logger.LogCritical($"EXCEPCIÓN: {e.Message}");
            }

            var dictionay = new Dictionary<string, string>();
            dictionay.Add("Event Created", groupEvent.Title);
            _telemetryClient.TrackEvent("UserInteraction", dictionay);

            return RedirectToAction("Details", "Groups", new { id = groupEvent.GroupId });
        }

        private async Task SendNewEventMessageToEventQueue(Event groupEvent) {
            int newEventId = _context.Events.FirstOrDefault(e => e.Title == groupEvent.Title).Id;
            string newEventDetailsURI = HttpContext.Request.GetDisplayUrl().Replace("Create", $"Details/{newEventId}");
            string message = $"{EventQueueMessages.EVENT_CREATED};{newEventId};{newEventDetailsURI}";
            await _eventQueueService.SendMessageAsync(message);
            
            var dictionay = new Dictionary<string, string>();
            dictionay.Add("Queue Message", message);
            _telemetryClient.TrackEvent("UserInteraction", dictionay);
        }
    }
}
