using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMeetUp.Web.Areas.Admin.ViewModels;
using MyMeetUp.Web.Data;
using MyMeetUp.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MyMeetUp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RolesData.Administrator)]
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public GroupsController(ILogger<GroupsController> logger, ApplicationDbContext context) {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Groups
        public async Task<IActionResult> Index() {
            return View(await _context.Groups.ToListAsync());
        }

        // GET: Admin/Groups/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var @group = await _context.Groups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@group == null) {
                return NotFound();
            }

            return View(@group);
        }

        // GET: Admin/Groups/Create
        public IActionResult Create() {
            ViewData["GrupoCategories"] = new SelectList(_context.GroupCategories, "Id", "Name");
            return View();
        }

        // POST: Admin/Groups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupCreateViewModel groupCreateViewModel) {
            if (ModelState.IsValid) {

                //Create Group
                _context.Groups.Add(groupCreateViewModel.Group);
                await _context.SaveChangesAsync();
                
                //Create Group-Categories relations
                Group newGroupCreated = await _context.Groups.Where(g => g.Name == groupCreateViewModel.Group.Name).FirstOrDefaultAsync();
                foreach (int idCategoryGroup in groupCreateViewModel.GroupCategories) {
                    Group_GroupCategory groupCategories = new Group_GroupCategory
                    {
                        GroupId = newGroupCreated.Id,
                        GroupCategoryId = idCategoryGroup
                    };
                    _context.Group_GroupCategories.Add(groupCategories);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(groupCreateViewModel);
        }

        // GET: Admin/Groups/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var @group = await _context.Groups.FindAsync(id);
            if (@group == null) {
                return NotFound();
            }
            return View(@group);
        }

        // POST: Admin/Groups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,AboutUs,Country,City,CreationDate,FinalizationDate,Id")] Group @group) {
            if (id != @group.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(@group);
                    await _context.SaveChangesAsync();
                } catch (DbUpdateConcurrencyException) {
                    if (!GroupExists(@group.Id)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(@group);
        }

        // GET: Admin/Groups/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            var @group = await _context.Groups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@group == null) {
                return NotFound();
            }

            return View(@group);
        }

        // POST: Admin/Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var @group = await _context.Groups.FindAsync(id);
            _context.Groups.Remove(@group);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupExists(int id) {
            return _context.Groups.Any(e => e.Id == id);
        }
    }
}
