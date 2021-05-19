using AppDevProjectGroup27.Data;
using AppDevProjectGroup27.Models.ViewModels;
using AppDevProjectGroup27.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
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

        //POST-Create
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            //Converting the java code from the view for the SubCategory and assigning it to the Binded MenuItemVM SubCatId
            MenuItemVM.MenuItems.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if(!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }

            _db.MenuItems.Add(MenuItemVM.MenuItems);
            await _db.SaveChangesAsync();


            //Image saving Section
            //Name of image will be the ID of the menuItem number


            //Extracting the root path for where images will be saved
            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            //Recieving MenuItem From DB
            var menuItemFromDb = await _db.MenuItems.FindAsync(MenuItemVM.MenuItems.Id);

            if(files.Count>0)
            {
                //File was upload

                //Combining the webroot path with images
                var uploads = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);

                using (var filesStream = new FileStream(Path.Combine(uploads,MenuItemVM.MenuItems.Id + extension),FileMode.Create))
                {
                    //will copy the image to the file location on the sever and rename it
                    files[0].CopyTo(filesStream);
                }

                //In database the name will be changed to te loctaion where the image is saved
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItems.Id + extension;

            }
            else
            {
                //No file uploaded so default will be used
                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemVM.MenuItems.Id + ".png");

                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItems.Id + ".png";

            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));




        }

        //GET-Edit
        public async Task<IActionResult> Edit(int? id)
        {

            if(id==null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItems = await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItems.CategoryId).ToListAsync();

            if(MenuItemVM.MenuItems==null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }

        //POST-Edit
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPOST(int? id)
        {

            if(id==null)
            {
                return NotFound();
            }


            //Converting the java code from the view for the SubCategory and assigning it to the Binded MenuItemVM SubCatId
            MenuItemVM.MenuItems.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (!ModelState.IsValid)
            {
                MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItems.CategoryId).ToListAsync();
                return View(MenuItemVM);
            }

            
            //Image saving Section
            //Name of image will be the ID of the menuItem number

            //Extracting the root path for where images will be saved
            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            //Recieving MenuItem From DB
            var menuItemFromDb = await _db.MenuItems.FindAsync(MenuItemVM.MenuItems.Id);

            if (files.Count > 0)
            {
                //New Image File was upload

                //Combining the webroot path with images
                var uploads = Path.Combine(webRootPath, "images");
                var extension_new = Path.GetExtension(files[0].FileName);

                // Delete Image File that was in Database
                var imagePath = Path.Combine(webRootPath,menuItemFromDb.Image.TrimStart('\\'));

                if(System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                // New Image File will be uploaded
                using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItems.Id + extension_new), FileMode.Create))
                {
                    //will copy the image to the file location on the sever and rename it
                    files[0].CopyTo(filesStream);
                }

                //In database the name will be changed to te loctaion where the image is saved
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItems.Id + extension_new;

            }

            menuItemFromDb.Name = MenuItemVM.MenuItems.Name;
            menuItemFromDb.Descriptions = MenuItemVM.MenuItems.Descriptions;
            menuItemFromDb.Price = MenuItemVM.MenuItems.Price;
            menuItemFromDb.Spicyness = MenuItemVM.MenuItems.Spicyness;
            menuItemFromDb.CategoryId = MenuItemVM.MenuItems.CategoryId;
            menuItemFromDb.SubCategoryId = MenuItemVM.MenuItems.SubCategoryId;


            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));




        }
    }
}
