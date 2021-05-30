using AppDevProjectGroup27.Data;
using AppDevProjectGroup27.Models;
using AppDevProjectGroup27.Models.ViewModels;
using AppDevProjectGroup27.Utility;
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
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public OrderDetailsCart detailsCart { get; set; }

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<IActionResult> Index()
        {

            detailsCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader()
            };

            detailsCart.OrderHeader.OrderTotal = 0;

            //Retrivr the user Id of the logged-in user
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //Retrive all the items the uder has added to the cart
            var cart = _db.ShoppingCart.Where(m => m.ApplicationUserId == claims.Value);

            if(cart != null)
            {
                detailsCart.listCart = cart.ToList();
            }

            //To calculate the order total
            foreach(var list in detailsCart.listCart)
            {
                list.MenuItems = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == list.MenuItemId);
                detailsCart.OrderHeader.OrderTotal = detailsCart.OrderHeader.OrderTotal + (list.MenuItems.Price + list.Count);
                list.MenuItems.Descriptions = SD.ConvertToRawHtml(list.MenuItems.Descriptions);

                if (list.MenuItems.Descriptions.Length>100)
                {
                    //Modiffied the description
                    list.MenuItems.Descriptions = list.MenuItems.Descriptions.Substring(0, 99) + "...";

                }

            }
            //Comparing the oderTotal and the orderTotalOriginal
            detailsCart.OrderHeader.OrderTotal = detailsCart.OrderHeader.OrderTotal;

            return View(detailsCart);

        }
    }
}
