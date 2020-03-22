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
    public class GroupCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GroupCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/GroupCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.GroupCategories.ToListAsync());
        }

        // GET: Admin/GroupCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupCategory = await _context.GroupCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (groupCategory == null)
            {
                return NotFound();
            }

            return View(groupCategory);
        }

        // GET: Admin/GroupCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/GroupCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Id")] GroupCategory groupCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(groupCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(groupCategory);
        }

        // GET: Admin/GroupCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupCategory = await _context.GroupCategories.FindAsync(id);
            if (groupCategory == null)
            {
                return NotFound();
            }
            return View(groupCategory);
        }

        // POST: Admin/GroupCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Id")] GroupCategory groupCategory)
        {
            if (id != groupCategory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(groupCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupCategoryExists(groupCategory.Id))
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
            return View(groupCategory);
        }

        // GET: Admin/GroupCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupCategory = await _context.GroupCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (groupCategory == null)
            {
                return NotFound();
            }

            return View(groupCategory);
        }

        // POST: Admin/GroupCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var groupCategory = await _context.GroupCategories.FindAsync(id);
            _context.GroupCategories.Remove(groupCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupCategoryExists(int id)
        {
            return _context.GroupCategories.Any(e => e.Id == id);
        }
    }
}
