using AppDevProjectGroup27.Data;
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
    [Area("Customer")]
    public class OrderController : Controller
    {

        private ApplicationDbContext _db;

        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize]
        public async Task <IActionResult> Confirm(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = await _db.OrderHeader.Include(o => o.ApplicationUser).FirstOrDefaultAsync(o => o.Id == id && o.UserId == claim.Value),
                OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == id).ToListAsync()   

            };

            orderDetailsViewModel.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
            orderDetailsViewModel.OrderHeader.Status = SD.StatusSubmitted;

            await _db.SaveChangesAsync();

            return View(orderDetailsViewModel);
        }

        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = await _db.OrderHeader.Include(o => o.ApplicationUser).FirstOrDefaultAsync(o => o.Id == id && o.UserId == claim.Value),
                OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == id).ToListAsync()

            };


            //Changes
            orderDetailsViewModel.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            orderDetailsViewModel.OrderHeader.Status = SD.StatusCancelled;

            await _db.SaveChangesAsync();
            //Changes
            return View(orderDetailsViewModel);
        }
    }
}
