using AppDevProjectGroup27.Data;
using AppDevProjectGroup27.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuItemsController : Controller
    {
        private readonly ApplicationDbContext _db;

        private readonly IWebHostEnvironment _hostingEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }
        //Allows us to not parse this as an agruement in the Create Code




        public MenuItemsController(ApplicationDbContext db, IWebHostEnvironment hostingEnvironment)
        {
            _db = db;
           _hostingEnvironment = hostingEnvironment;
            MenuItemVM = new MenuItemViewModel()
            {
                Category = _db.Category,
                MenuItems = new Models.MenuItems()

            };
        }

        public async Task<IActionResult> Index()
        {
            // Using eager loading to load the CategoryId and SubCatagoryId from MenueItems.cs
            var menuItems = await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync();

            return View(menuItems);
        }

        //GET-Create
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }
    }
}
