using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppDevProjectGroup27.Data;
using AppDevProjectGroup27.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]
    [Area("Admin")]
    public class SpecialController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SpecialController(ApplicationDbContext db)
        {
            _db = db;
        }

        //Get
        public async Task<IActionResult> Index()
        {
            return View(await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.OnSpecial == true).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string? subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
                return View(await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.OnSpecial == true).ToListAsync());
            else
            {
                // CODE TO SEND EMAIL TO ALL CUSTOMERS
                return View(await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.OnSpecial == true).ToListAsync());
            }
        }
    }
}
