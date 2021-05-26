using AppDevProjectGroup27.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        //Dependancy injection
        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            //Parsing all  users except the logged in user to the View
            //Id of the logged in user
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //                          logged in user Id should not be = to logged in user ID    
            return View(await _db.ApplicationUser.Where(m => m.Id !=claim.Value).ToListAsync());
        }
    }
}
