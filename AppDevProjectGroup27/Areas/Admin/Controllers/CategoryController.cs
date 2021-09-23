using AppDevProjectGroup27.Data;
using AppDevProjectGroup27.Models;
using AppDevProjectGroup27.Models.ViewModels;
using AppDevProjectGroup27.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        //GET
        public async Task<IActionResult> Index()
        {

            return View(await _db.Category.ToListAsync());
        }

        //GET-Create
        public IActionResult Create()
        {
            return View();
        }

        //POST - Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if(ModelState.IsValid)
            {
                //if valid
                _db.Category.Add(category);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        //Get-Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = await _db.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _db.Update(category);
                await _db.SaveChangesAsync();

                return RedirectToActionPermanent(nameof(Index));

            }
            return View(category);
        }


        //Get-Delete
        public async Task<IActionResult> Delete(int? id)
        {

            
            if (id == null)
            {
                return NotFound();
            }
            var category = await _db.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            CategoryDeleteVM objCD = new CategoryDeleteVM { Category = category };
            return View(objCD);
        }


        //Post-Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var category = await _db.Category.FindAsync(id);
            if (category == null)
            {
                return View();
            }

            CategoryDeleteVM objCD = new CategoryDeleteVM { Category = category };

            var IsLinkedSubCategory = await _db.SubCategory.Where(s => s.CategoryId.Equals(id)).ToListAsync();
            var IsLinkedMenuItem = await _db.MenuItems.Where(s => s.CategoryId.Equals(id)).ToListAsync();
            if (IsLinkedSubCategory.Count > 0)
            {
                objCD.StatusMessage = "Error : Unable to delete \"" + category.Name + "\", because it is linked to a Sub-Category.";
                return View(objCD);
            }
            else if (IsLinkedMenuItem.Count > 0)
            {
                objCD.StatusMessage = "Error : Unable to delete \"" + category.Name + "\", because it is linked to a Menu Item.";
                return View(objCD);
            }


            _db.Category.Remove(category);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //GET - Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await _db.Category.FindAsync(id);

            if (category == null)
                return NotFound();

            return View(category);
        }
    }
}
