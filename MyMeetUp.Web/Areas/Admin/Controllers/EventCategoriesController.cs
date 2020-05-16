﻿using Microsoft.AspNetCore.Authorization;
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
    public class EventCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventCategoriesController(ApplicationDbContext context) {
            _context = context;
        }

        // GET: Admin/EventCategories
        public async Task<IActionResult> Index() {
            return View(await _context.EventCategories.ToListAsync());
        }

        // GET: Admin/EventCategories/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var eventCategory = await _context.EventCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventCategory == null) {
                return NotFound();
            }

            return View(eventCategory);
        }

        // GET: Admin/EventCategories/Create
        public IActionResult Create() {
            return View();
        }

        // POST: Admin/EventCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Id")] EventCategory eventCategory) {
            if (ModelState.IsValid) {
                _context.Add(eventCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eventCategory);
        }

        // GET: Admin/EventCategories/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var eventCategory = await _context.EventCategories.FindAsync(id);
            if (eventCategory == null) {
                return NotFound();
            }
            return View(eventCategory);
        }

        // POST: Admin/EventCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Id")] EventCategory eventCategory) {
            if (id != eventCategory.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(eventCategory);
                    await _context.SaveChangesAsync();
                } catch (DbUpdateConcurrencyException) {
                    if (!EventCategoryExists(eventCategory.Id)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(eventCategory);
        }

        // GET: Admin/EventCategories/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            var eventCategory = await _context.EventCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventCategory == null) {
                return NotFound();
            }

            return View(eventCategory);
        }

        // POST: Admin/EventCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var eventCategory = await _context.EventCategories.FindAsync(id);
            _context.EventCategories.Remove(eventCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventCategoryExists(int id) {
            return _context.EventCategories.Any(e => e.Id == id);
        }
    }
}
