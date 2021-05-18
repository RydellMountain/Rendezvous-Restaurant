using AppDevProjectGroup27.Data;
using AppDevProjectGroup27.Models;
using AppDevProjectGroup27.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        [TempData]
        public string StatusMessage { get; set; }


        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
       
        // Get INDEX
        public async Task<IActionResult> Index()
        {
            var subCategories = await _db.SubCategory.Include(s=>s.Category).ToListAsync();
            return View(subCategories);
        }

        // GET - Create
        public async Task<IActionResult> Create()
        {
            SubCatgoryAndCategoryViewModel model = new SubCatgoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList = await _db.SubCategory.OrderBy(p=>p.Name).Select(p=>p.Name).Distinct().ToListAsync()
            };

            return View(model);
        }

        //POST Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCatgoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubcatExists = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);
                if (doesSubcatExists.Count() > 0)
                {
                    //Error
                    StatusMessage = "Error : Subcategory already esxists under " + doesSubcatExists.First().Category.Name + " category Please use another name ";
                }
                else
                {
                    _db.SubCategory.Add(model.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCatgoryAndCategoryViewModel modelVM = new SubCatgoryAndCategoryViewModel
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).ToListAsync(),
                StatusMessage = StatusMessage

            };
            return View(modelVM);
        }


        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {

            List<SubCategory> subCategories = new List<SubCategory>();

            subCategories = await (from subCategory in _db.SubCategory where subCategory.CategoryId == id select subCategory).ToListAsync();

            return Json(new SelectList(subCategories,"Id","Name"));
        }


        // GET - Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }

            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(m => m.Id == id);

            if(subCategory==null)
            {
                return NotFound();
            }

            SubCatgoryAndCategoryViewModel model = new SubCatgoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };

            return View(model);
        }

        //POST Edit

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubCatgoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubcatExists = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);
                if (doesSubcatExists.Count() > 0)
                {
                    //Error
                    StatusMessage = "Error : Subcategory already esxists under " + doesSubcatExists.First().Category.Name + " category Please use another name ";
                }
                else
                {
                    _db.SubCategory.Add(model.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCatgoryAndCategoryViewModel modelVM = new SubCatgoryAndCategoryViewModel
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).ToListAsync(),
                StatusMessage = StatusMessage

            };
            return View(modelVM);
        }

    }
}
