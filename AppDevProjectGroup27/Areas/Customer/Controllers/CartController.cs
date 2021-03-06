using AppDevProjectGroup27.Data;
using AppDevProjectGroup27.Models;
using AppDevProjectGroup27.Models.ViewModels;
using AppDevProjectGroup27.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


using PayFast;
using PayFast.AspNetCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AppDevProjectGroup27.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly PayFastSettings payFastSettings;
        private readonly ILogger logger;
        


        [BindProperty]
        public OrderDetailsCart detailsCart { get; set; }

        public CartController(ApplicationDbContext db, IOptions<PayFastSettings> payFastSettings, ILogger<CartController> logger)
        {
            _db = db;
            this.payFastSettings = payFastSettings.Value;
            this.logger = logger;
            

        }


        public OrderDetailsCart GetDetailsCartObj(string StatusMessage)
        {
            detailsCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader(),
                StatusMessage = StatusMessage
            };

            detailsCart.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var cart = _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value);
            if (cart != null)
            {
                detailsCart.listCart = cart.ToList();
            }

            var MenuItems = _db.MenuItems.ToList();

            foreach (var list in detailsCart.listCart)
            {
                list.MenuItems = _db.MenuItems.FirstOrDefault(m => m.Id == list.MenuItemId);
                if (list.MenuItems.AvaQuantity < list.Count)
                {
                    if (detailsCart.StatusMessage.Contains("Error"))
                        detailsCart.StatusMessage += ", \"" + list.MenuItems.Name + "\"";
                    else
                    {
                        if (list.MenuItems.AvaQuantity > 0)
                            detailsCart.StatusMessage += "Error : Please decrease the quantity of the following item(s): " + list.MenuItems.Name;
                        else
                            detailsCart.StatusMessage += "Error : Please remove the following item(s): " + list.MenuItems.Name;
                    }
                }

                detailsCart.OrderHeader.OrderTotal = Math.Round(detailsCart.OrderHeader.OrderTotal + (list.MenuItems.Price * list.Count), 2);
                list.MenuItems.Descriptions = SD.ConvertToRawHtml(list.MenuItems.Descriptions);
                if (list.MenuItems.Descriptions.Length > 100)
                {
                    list.MenuItems.Descriptions = list.MenuItems.Descriptions.Substring(0, 99) + "...";
                }
            }
            if (!string.IsNullOrEmpty(detailsCart.StatusMessage))
                detailsCart.StatusMessage += ".";

            detailsCart.OrderHeader.OrderTotalOriginal = detailsCart.OrderHeader.OrderTotal;

            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                detailsCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = _db.Coupon.Where(c => c.Name.ToLower() == detailsCart.OrderHeader.CouponCode.ToLower()).FirstOrDefault();
                detailsCart.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, detailsCart.OrderHeader.OrderTotalOriginal);
            }

            return detailsCart;
        }

        public IActionResult Index()
        {
            return View(GetDetailsCartObj(""));
        }

        public async Task<IActionResult> Summary()
        {

            detailsCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader()
            };

            detailsCart.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ApplicationUser applicationUser = await _db.ApplicationUser.Where(c => c.Id == claim.Value).FirstOrDefaultAsync();
            var cart = _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value);
            if (cart != null)
            {
                detailsCart.listCart = cart.ToList();
            }

            foreach (var list in detailsCart.listCart)
            {
                list.MenuItems = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == list.MenuItemId);
                detailsCart.OrderHeader.OrderTotal = Math.Round(detailsCart.OrderHeader.OrderTotal + (list.MenuItems.Price * list.Count), 2);

            }
            detailsCart.OrderHeader.OrderTotalOriginal = detailsCart.OrderHeader.OrderTotal;
            detailsCart.OrderHeader.PickUpName = applicationUser.Name;
            detailsCart.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            detailsCart.OrderHeader.PickUpTime = SharedMethods.GetDateTime();


            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                detailsCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(c => c.Name.ToLower() == detailsCart.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detailsCart.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, detailsCart.OrderHeader.OrderTotalOriginal);
            }


            return View(detailsCart);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            detailsCart.listCart = await _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value).ToListAsync();

            detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            detailsCart.OrderHeader.OrderDate = SharedMethods.GetDateTime();
            detailsCart.OrderHeader.UserId = claim.Value;
            detailsCart.OrderHeader.Status = SD.PaymentStatusPending;
            detailsCart.OrderHeader.PickUpTime = Convert.ToDateTime(detailsCart.OrderHeader.PickUpDate.ToShortDateString() + " " + detailsCart.OrderHeader.PickUpTime.ToShortTimeString());
            detailsCart.OrderHeader.StartDateTime = null;
            detailsCart.OrderHeader.PickedUpOrder = null;
            detailsCart.OrderHeader.CompleteDateTime = null;
            detailsCart.OrderHeader.Duration = "";
            detailsCart.OrderHeader.OrderStartedBy = "";
            detailsCart.OrderHeader.OrderCompletedBy = "";

            int CalcMins = 0;
            foreach (var item in detailsCart.listCart)
            {
                MenuItems objM = await _db.MenuItems.FindAsync(item.MenuItemId);
                int ETA = objM.ETAEstimate;
                CalcMins += ETA * item.Count;
            }

            TimeSpan tempTime = TimeSpan.FromMinutes(CalcMins);
            double Hours = Math.Abs(tempTime.Hours);
            double Min = Math.Abs(tempTime.Minutes);
            if (Hours > 0)
            {
                if (Min == 0)
                    detailsCart.OrderHeader.EstimatedTimeComplete = Hours.ToString() + " hours";
                else
                    detailsCart.OrderHeader.EstimatedTimeComplete = Hours.ToString() + " h " + Min.ToString();
            }
            else if (Min > 0)
                detailsCart.OrderHeader.EstimatedTimeComplete = Min.ToString() + " mins";
            else
                detailsCart.OrderHeader.EstimatedTimeComplete = "0 mins";


            List<OrderDetails> orderDetailsList = new List<OrderDetails>();
            _db.OrderHeader.Add(detailsCart.OrderHeader);
            await _db.SaveChangesAsync();

            detailsCart.OrderHeader.OrderTotalOriginal = 0;


            foreach (var item in detailsCart.listCart)
            {
                MenuItems objMenuItem = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == item.MenuItemId);
                item.MenuItems = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == item.MenuItemId);
                OrderDetails orderDetails = new OrderDetails
                {
                    MenuItemId = item.MenuItemId,
                    OrderId = detailsCart.OrderHeader.Id,
                    Description = item.MenuItems.Descriptions,
                    Name = item.MenuItems.Name,
                    Price = item.MenuItems.Price,
                    Count = item.Count
                };
                objMenuItem.AvaQuantity -= item.Count;
                detailsCart.OrderHeader.OrderTotalOriginal += orderDetails.Count * orderDetails.Price;
                _db.OrderDetails.Add(orderDetails);

            }

            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                detailsCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(c => c.Name.ToLower() == detailsCart.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detailsCart.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, detailsCart.OrderHeader.OrderTotalOriginal);
            }
            else
            {
                detailsCart.OrderHeader.OrderTotal = detailsCart.OrderHeader.OrderTotalOriginal;
            }
            detailsCart.OrderHeader.CouponCodeDiscount = detailsCart.OrderHeader.OrderTotalOriginal - detailsCart.OrderHeader.OrderTotal;

            _db.ShoppingCart.RemoveRange(detailsCart.listCart);
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, 0);
            await _db.SaveChangesAsync();





            //------------ PAYFAST ------------
            //Gets passsword from appsettings
            var onceOffRequest = new PayFastRequest(this.payFastSettings.PassPhrase);

            // Merchant Details, getting various keys,IDs,URLs
            onceOffRequest.merchant_id = this.payFastSettings.MerchantId;
            onceOffRequest.merchant_key = this.payFastSettings.MerchantKey;
            onceOffRequest.return_url = this.payFastSettings.ReturnUrl + detailsCart.OrderHeader.Id;
            onceOffRequest.cancel_url = this.payFastSettings.CancelUrl + detailsCart.OrderHeader.Id;
            onceOffRequest.notify_url = this.payFastSettings.NotifyUrl;

            // Buyer Details
            onceOffRequest.email_address = "sbtu01@payfast.co.za";

            // Transaction Details, details for the order
            onceOffRequest.m_payment_id = detailsCart.OrderHeader.Id.ToString();
            onceOffRequest.amount = detailsCart.OrderHeader.OrderTotal;

            onceOffRequest.item_name = $"Order Number:{detailsCart.OrderHeader.Id}.";
            onceOffRequest.item_name += $"Order Pickup name:{detailsCart.OrderHeader.PickUpName}";


            /*
             * Order #13
             * Order Items  Quantity
             * Medium Rare    5
             */







            onceOffRequest.item_description = "Some details about the once off payment";

            // Transaction Options
            onceOffRequest.email_confirmation = true;
            onceOffRequest.confirmation_address = "sbtu01@payfast.co.za";

            var redirectUrl = $"{this.payFastSettings.ProcessUrl}{onceOffRequest.ToString()}";


            // Moved Payment Approved to Confirm Action method

            return Redirect(redirectUrl);
            //------------ PAYFAST ------------

            //Return
            //return RedirectToAction("Confirm", "Order", new { id = detailCart.OrderHeader.Id });

        }

        public IActionResult AddCoupon()
        {
            if (detailsCart.OrderHeader.CouponCode == null)
            {
                detailsCart.OrderHeader.CouponCode = "";
            }
            HttpContext.Session.SetString(SD.ssCouponCode, detailsCart.OrderHeader.CouponCode);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveCoupon()
        {

            HttpContext.Session.SetString(SD.ssCouponCode, string.Empty);

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> plus(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);
            var MenuItem = await _db.MenuItems.FindAsync(cart.MenuItemId);
            if ((cart.Count + 1) > MenuItem.AvaQuantity)
                return View(nameof(Index), GetDetailsCartObj("Error : Cannot add more " + MenuItem.Name + " items"));
            else
                cart.Count += 1;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> minus(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);
            if (cart.Count == 1)
            {
                _db.ShoppingCart.Remove(cart);
                await _db.SaveChangesAsync();

                var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);
            }
            else
            {
                cart.Count -= 1;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> remove(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);

            _db.ShoppingCart.Remove(cart);
            await _db.SaveChangesAsync();

            var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);


            return RedirectToAction(nameof(Index));
        }




    }
}

