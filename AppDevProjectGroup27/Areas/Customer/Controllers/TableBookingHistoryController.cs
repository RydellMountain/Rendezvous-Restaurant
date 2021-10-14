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
using System.Security.Claims;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]
    public class TableBookingHistoryController : Controller
    {
        private ApplicationDbContext _db;
        private int PageSize = 2;

        public TableBookingHistoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(int productPage = 1)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            List<TableBookingHeader> objTableHeader = await _db.TableBookingHeader.Include(t => t.ApplicationUser)
                .Where(t => t.UserId == claims.Value).ToListAsync();

            var count = objTableHeader.Count;

            TableBookingHistVM objTBHVM = new TableBookingHistVM();

            objTBHVM.tableBookingHeaders = objTableHeader.OrderByDescending(t => t.SitInDate)
                .ThenByDescending(t => t.SitInTime).Skip((productPage - 1) * PageSize)
                .Take(PageSize).ToList();

            objTBHVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemPerPage = PageSize,
                TotalItems = count,
                UrlParam = "/Customer/TableBookingHistory/Index?productPage=:"
            };

            return View(objTBHVM);
        }
    }
}
