using Microsoft.AspNetCore.Hosting;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;

namespace MyMeetUp.Web.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IQueueService _eventQueueService;
        private readonly IWebHostEnvironment _hostingEnvirontment;
        private TelemetryClient _telemetryClient;

        public GroupsController(ApplicationDbContext context, 
                                SignInManager<ApplicationUser> signInManager, 
                                UserManager<ApplicationUser> userManager, 
                                ILogger<GroupsController> logger,
                                IQueueService eventQueueService,
                                IWebHostEnvironment environment,
                                TelemetryClient telemetry) {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _eventQueueService = eventQueueService;
            _hostingEnvirontment = environment;
            _telemetryClient = telemetry;
        }

        // GET: Groups
        [Route("Groups/Index/{userId}")]
        public async Task<IActionResult> Index(string userId) {
            GroupIndexViewModel model = new GroupIndexViewModel();
            model.UserId = userId;

            if (userId != null && userId != "") {
                model.AllGroups = await _context.GroupMembers
                    .Include(gm => gm.Group)
                    .Where(gm => gm.ApplicationUserId == userId)
                    .Select(gm => gm.Group)
                    .ToListAsync();

                model.ActualUserAsGroupCoordinator = await _context.GroupMembers
                    .Include(gmp => gmp.GroupMemberProfile)
                    .Where(gm => gm.ApplicationUserId == userId && gm.GroupMemberProfile.Name == GroupMemberProfilesData.Coordinator)
                    .Select(gm => gm.GroupId)
                    .ToListAsync();

                model.ActualUserAsGroupMember = await _context.GroupMembers
                    .Include(gmp => gmp.GroupMemberProfile)
                    .Where(gm => gm.ApplicationUserId == userId && gm.GroupMemberProfile.Name == GroupMemberProfilesData.Member)
                    .Select(gm => gm.GroupId)
                    .ToListAsync();

                await GetGroupCategories(model);

                return View(model);
            }
            else {
                model.AllGroups = await _context.Groups.ToListAsync();
                model.ActualUserAsGroupCoordinator = null;
                model.ActualUserAsGroupMember = null;
                await GetGroupCategories(model);
                return View(model);
            }
            
        }

        private async Task GetGroupCategories(GroupIndexViewModel model) {
            model.GroupCategories = new Dictionary<int, List<string>>();
            foreach (Group group in model.AllGroups) {
                List<string> groupCategories = await _context.Group_GroupCategories
                                                .Include(ggc => ggc.GroupCategory)
                                                .Where(ggc => ggc.GroupId == group.Id)
                                                .Select(ggc => ggc.GroupCategory.Name)
                                                .ToListAsync();
                model.GroupCategories.Add(group.Id, groupCategories);
            }
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

            var dictionay = new Dictionary<string, string>();
            dictionay.Add("Group Details", groupSelected.Name);
            _telemetryClient.TrackEvent("UserInteraction", dictionay);

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

                string message = $"{EventQueueMessages.NEW_GROUP_MEMBER};{newGroupMember.GroupId};{newGroupMember.ApplicationUserId}";
                await _eventQueueService.SendMessageAsync(message);
                var dictionay = new Dictionary<string, string>();
                dictionay.Add("Queue Message", message);
                _telemetryClient.TrackEvent("UserInteraction", dictionay);

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

            var dictionay = new Dictionary<string, string>();
            dictionay.Add("Unregisted Group Member", $"userId: {userId}");
            _telemetryClient.TrackEvent("UserInteraction", dictionay);

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
                    
                    await SendNewGroupMessageToEventQueue(newGroupId, group);
                    
                    return RedirectToAction("Index", "Groups", new { userId = userSignedIn });

                } catch (Exception e) {
                    dbContextTransaction.Rollback();
                    _logger.LogError($"EXCEPCIÓN: {e.Message}");
                    return View(group);
                }
            }
                
        }

        private async Task SendNewGroupMessageToEventQueue(int newGroupId, GroupCreateViewModel group) {
            StringBuilder groupCategories = new StringBuilder();
            foreach (int groupCategory in group.GroupCategoriesSelected) {
                groupCategories.Append(";" + groupCategory);
            }
            string newGroupDetailsURI = HttpContext.Request.GetDisplayUrl().Replace("Create", $"Details/{newGroupId}");
            string message = $"{EventQueueMessages.GROUP_CREATED};{newGroupId};{newGroupDetailsURI}{groupCategories.ToString()}";
            await _eventQueueService.SendMessageAsync(message);

            var dictionay = new Dictionary<string, string>();
            dictionay.Add("Queue Message", message);
            _telemetryClient.TrackEvent("UserInteraction", dictionay);
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

        public async Task<IActionResult> ByCategory(int id) {
            GroupIndexViewModel model = new GroupIndexViewModel();
            model.ByCategory = await _context.GroupCategories.Where(gc => gc.Id == id).Select(gc => gc.Name).FirstOrDefaultAsync();
            model.AllGroups = await _context.Group_GroupCategories
                                .Include(ggc => ggc.Group)
                                .Where(ggc => ggc.GroupCategoryId == id)
                                .Select(ggc=>ggc.Group)
                                .ToListAsync();
            await GetGroupCategories(model);

            return View("Index", model);
        }
    }
}
