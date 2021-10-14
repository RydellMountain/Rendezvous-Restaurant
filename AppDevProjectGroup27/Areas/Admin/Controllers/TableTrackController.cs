using AppDevProjectGroup27.Data;
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
    public class TableTrackController : Controller
    {
        public readonly ApplicationDbContext _db;

        public TableTrackController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _db.TableTrack.Include(t => t.Table).OrderByDescending(t => t.DateTable).ThenByDescending(t => t.TimeTable).ToListAsync());
        }
    }
}
