using AppDevProjectGroup27.Data;
using AppDevProjectGroup27.Models;
using AppDevProjectGroup27.Models.ViewModels;
using AppDevProjectGroup27.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {

        private ApplicationDbContext _db;

        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _hostEnviroment;

        private int PageSize = 2;

        [BindProperty]
        public OrderDetailsCart detailsCart { get; set; }

        public OrderController(ApplicationDbContext db, IEmailSender emailSender, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _emailSender = emailSender;
            _hostEnviroment = hostEnvironment;
        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> RefundOrder(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusCancelled;
            orderHeader.PaymentStatus = SD.PaymentStatusRefunded;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var StaffName = _db.ApplicationUser.Where(a => a.Id == claim.Value).SingleOrDefault().Name;

            orderHeader.OrderCancelledBy = StaffName;
            orderHeader.OrderRefundedBy = StaffName;

            var CustomerInfo = _db.ApplicationUser.Where(u => u.Id == orderHeader.UserId).FirstOrDefault();
            var CustomerEmail = CustomerInfo.Email;
            var CustomerName = CustomerInfo.Name;

            List<OrderDetails> objDetails = await _db.OrderDetails.Where(o => o.OrderId == OrderId).ToListAsync();

            SendEmail(CustomerName, CustomerEmail, "Order : " + OrderId+" - Cancelled & Refund", "Order number "+OrderId+", has been cancelled, and is in the process of being refunded by our staff.<br />The order details are as follows:", objDetails,orderHeader.CouponCode, orderHeader.CouponCodeDiscount);

            await _db.SaveChangesAsync();
            return RedirectToAction("ManageOrder", "Order");
        }
        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> Refund(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.PaymentStatus = SD.PaymentStatusRefunded;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var StaffName = _db.ApplicationUser.Where(a => a.Id == claim.Value).SingleOrDefault().Name;

            orderHeader.OrderRefundedBy = StaffName;

            var CustomerInfo = _db.ApplicationUser.Where(u => u.Id == orderHeader.UserId).FirstOrDefault();
            var CustomerEmail = CustomerInfo.Email;
            var CustomerName = CustomerInfo.Name;

            SendEmail(CustomerName, CustomerEmail, "Order : " + OrderId+" - Refund", "Order number "+OrderId+", is in the process of being refunded by our staff.", null,null,0);

            await _db.SaveChangesAsync();
            string StatusMessage = "Order " + OrderId + " has been marked as " + SD.PaymentStatusRefunded;
            return View(nameof(CusOrderHistory), GetCusOrderVM(SD.StatusCancelled, SD.PaymentStatusRefunded, orderHeader.OrderDate.Date, StatusMessage));
        }

        public async Task<IActionResult> GetCusOrderDetails(int id)
        {
            OrderDetailsViewModel orderDetailsVM = new OrderDetailsViewModel()
            {
                OrderHeader = await _db.OrderHeader.FirstOrDefaultAsync(m => m.Id == id),
                OrderDetails = await _db.OrderDetails.Where(m => m.OrderId == id).ToListAsync()

            };

            orderDetailsVM.OrderHeader.ApplicationUser = await _db.ApplicationUser.FirstOrDefaultAsync(u => u.Id == orderDetailsVM.OrderHeader.UserId);

            return PartialView("_IndividualCusOrderDetails", orderDetailsVM);


        }


        public CusOrderHistoryVM GetCusOrderVM(string Status, string PaymentStatus, DateTime? dateChosen, string StatusMessage)
        {
            CusOrderHistoryVM cusOrderHistoryVM = new CusOrderHistoryVM()
            {
                Orders = new List<OrderDetailsViewModel>(),
                Status = new List<SelectListItem>()
                {
                    new SelectListItem() { Text = SD.StatusSubmitted, Value = SD.StatusSubmitted},
                    new SelectListItem() { Text = SD.StatusInProcess, Value = SD.StatusInProcess},
                    new SelectListItem() { Text = SD.StatusReady, Value = SD.StatusReady},
                     new SelectListItem() { Text = SD.StatusCompleted, Value = SD.StatusCompleted },
                    new SelectListItem() { Text = SD.StatusCancelled, Value = SD.StatusCancelled }
                },
                PaymentStatus = new List<SelectListItem>()
                {
                    new SelectListItem() { Text = SD.PaymentStatusApproved, Value = SD.PaymentStatusApproved},
                    new SelectListItem() { Text = SD.PaymentStatusRejected, Value = SD.PaymentStatusRejected},
                    new SelectListItem() { Text = SD.PaymentStatusRefunded, Value = SD.PaymentStatusRefunded},
                }
                ,
                CurrentDate = SharedMethods.GetDateTime().Date,
                StatusChosen = Status,
                StatusMessage = StatusMessage,
                PaymentStatusChosen = PaymentStatus
            };

            var orderHeaderTbl = _db.OrderHeader.Select(o => o.OrderDate);

            if (orderHeaderTbl.Any())
                cusOrderHistoryVM.EarliestDate = orderHeaderTbl.Min().Date;
            else
                cusOrderHistoryVM.EarliestDate = SharedMethods.GetDateTime().Date;



            List<OrderHeader> orderHeadersList = new List<OrderHeader>();

            if (!string.IsNullOrEmpty(Status) && !string.IsNullOrEmpty(PaymentStatus) && dateChosen != null)
                orderHeadersList = _db.OrderHeader.Include(o => o.ApplicationUser).Where(o => o.Status == Status && o.PaymentStatus == PaymentStatus && o.OrderDate.Date == dateChosen.Value.Date).ToList();
            else if (!string.IsNullOrEmpty(Status) && string.IsNullOrEmpty(PaymentStatus) && dateChosen == null)
                orderHeadersList = _db.OrderHeader.Include(o => o.ApplicationUser).Where(o => o.Status == Status).ToList();
            else if (string.IsNullOrEmpty(Status) && !string.IsNullOrEmpty(PaymentStatus) && dateChosen == null)
                orderHeadersList = _db.OrderHeader.Include(o => o.ApplicationUser).Where(o => o.PaymentStatus == PaymentStatus).ToList();
            else if (string.IsNullOrEmpty(Status) && string.IsNullOrEmpty(PaymentStatus) && dateChosen != null)
                orderHeadersList = _db.OrderHeader.Include(o => o.ApplicationUser).Where(o => o.OrderDate.Date == dateChosen.Value.Date).ToList();
            else if (!string.IsNullOrEmpty(Status) && !string.IsNullOrEmpty(PaymentStatus) && dateChosen == null)
                orderHeadersList = _db.OrderHeader.Include(o => o.ApplicationUser).Where(o => o.Status == Status && o.PaymentStatus == PaymentStatus).ToList();
            else if (!string.IsNullOrEmpty(Status) && string.IsNullOrEmpty(PaymentStatus) && dateChosen != null)
                orderHeadersList = _db.OrderHeader.Include(o => o.ApplicationUser).Where(o => o.Status == Status && o.OrderDate.Date == dateChosen.Value.Date).ToList();
            else if (string.IsNullOrEmpty(Status) && !string.IsNullOrEmpty(PaymentStatus) && dateChosen != null)
                orderHeadersList = _db.OrderHeader.Include(o => o.ApplicationUser).Where(o => o.PaymentStatus == PaymentStatus && o.OrderDate.Date == dateChosen.Value.Date).ToList();
            else
            {
                Status = SD.StatusCompleted;
                PaymentStatus = SD.PaymentStatusApproved;
                dateChosen = SharedMethods.GetDateTime().Date;
                orderHeadersList = _db.OrderHeader.Include(o => o.ApplicationUser).Where(o => o.Status == SD.StatusCompleted && o.PaymentStatus == SD.PaymentStatusApproved && o.OrderDate.Date == SharedMethods.GetDateTime().Date).ToList();
            }


            foreach (OrderHeader item in orderHeadersList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = _db.OrderDetails.Where(o => o.OrderId == item.Id).ToList()
                };
                cusOrderHistoryVM.Orders.Add(individual);
            }

            cusOrderHistoryVM.Orders = cusOrderHistoryVM.Orders.OrderByDescending(o => o.OrderHeader.Id).ToList();

            cusOrderHistoryVM.DisplayDate = dateChosen;
            return cusOrderHistoryVM;
        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public IActionResult CusOrderHistory()
        {
            return View(GetCusOrderVM(SD.StatusCompleted, SD.PaymentStatusApproved, SharedMethods.GetDateTime().Date, ""));
        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CusOrderHistory(string Status, string PaymentStatus, DateTime? datepicker)
        {
            return View(GetCusOrderVM(Status, PaymentStatus, datepicker, ""));

        }




        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = await _db.OrderHeader.Include(o => o.ApplicationUser).FirstOrDefaultAsync(o => o.Id == id && o.UserId == claim.Value),
                OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == id).ToListAsync()

            };

            //await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email, "Spice - Order Created " + detailsCart.OrderHeader.Id.ToString(), "Test email");

            var CustomerInfo = _db.ApplicationUser.Where(u => u.Id == claim.Value).FirstOrDefault();
            var CustomerEmail = CustomerInfo.Email;
            var CustomerName = CustomerInfo.Name;


            SendEmail(CustomerName, CustomerEmail, "Order : " + id+" - Confirmation", "Thank you for your order, your order details are as follows:", orderDetailsViewModel.OrderDetails,orderDetailsViewModel.OrderHeader.CouponCode,orderDetailsViewModel.OrderHeader.CouponCodeDiscount);
  
           
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

            var CustomerInfo = _db.ApplicationUser.Where(u => u.Id == claim.Value).FirstOrDefault();
            var CustomerEmail = CustomerInfo.Email;
            var CustomerName = CustomerInfo.Name;


            SendEmail(CustomerName, CustomerEmail, "Order : " + id+" - Cancelled", "Your order has been cancelled, the order details are as follows:", orderDetailsViewModel.OrderDetails,orderDetailsViewModel.OrderHeader.CouponCode,orderDetailsViewModel.OrderHeader.CouponCodeDiscount);
            //Changes
            orderDetailsViewModel.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            orderDetailsViewModel.OrderHeader.Status = SD.StatusCancelled;
            orderDetailsViewModel.OrderHeader.OrderCancelledBy = "Customer";

            await _db.SaveChangesAsync();
            //Changes
            return View(orderDetailsViewModel);
        }

        [Authorize]
        public async Task<IActionResult> OrderHistory(int productPage = 1)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderListViewModel orderListVM = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()

            };

            List<OrderHeader> orderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser).Where(u => u.UserId == claim.Value).ToListAsync();

            foreach (OrderHeader item in orderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == item.Id).ToListAsync()
                };
                orderListVM.Orders.Add(individual);
            }

            var count = orderListVM.Orders.Count;
            orderListVM.Orders = orderListVM.Orders.OrderByDescending(p => p.OrderHeader.Id)
                .Skip((productPage - 1) * PageSize)
                .Take(PageSize).ToList();

            orderListVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemPerPage = PageSize,
                TotalItems = count,
                UrlParam = "/Customer/Order/OrderHistory?productPage=:"
            };

            return View(orderListVM);
        }

        public IActionResult GetOrderStatus(int Id)
        {
            return PartialView("_OrderStatus", _db.OrderHeader.Where(m => m.Id == Id).FirstOrDefault().Status);

        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> ManageOrder(int productPage = 1)
        {
            List<OrderDetailsViewModel> orderDetailsVMs = new List<OrderDetailsViewModel>();
            List<OrderHeader> orderHeadersList = await _db.OrderHeader
                .Where(o => o.Status == SD.StatusSubmitted || o.Status == SD.StatusInProcess)
                .OrderByDescending(o => o.PickUpTime).ToListAsync();


            foreach (OrderHeader item in orderHeadersList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == item.Id).ToListAsync()
                };

                orderDetailsVMs.Add(individual);
            }

            return View(orderDetailsVMs.OrderBy(o => o.OrderHeader.PickUpTime).ToList());
        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderPrepare(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusInProcess;
            orderHeader.StartDateTime = DateTime.Now;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var StaffName =  _db.ApplicationUser.Where(a => a.Id == claim.Value).SingleOrDefault().Name;

            orderHeader.OrderStartedBy = StaffName;
            // orderHeader.OrderStartedBy 

            var CustomerInfo = _db.ApplicationUser.Where(u => u.Id == orderHeader.UserId).FirstOrDefault();
            var CustomerEmail = CustomerInfo.Email;
            var CustomerName = CustomerInfo.Name;

            SendEmail(CustomerName, CustomerEmail, "Order : " + OrderId+" - Being Prepared", "Order number "+OrderId+", is currently being prepared by our awesome cooks.<br />The Estimated Cooking Time is "+orderHeader.EstimatedTimeComplete+".", null,null,0);


            await _db.SaveChangesAsync();
            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderReady(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusReady;

            orderHeader.CompleteDateTime = DateTime.Now;
            TimeSpan SubtractQuan = orderHeader.CompleteDateTime.Value.Subtract(orderHeader.StartDateTime.Value);
            double Hours = Math.Abs(SubtractQuan.Hours);
            double Min = Math.Abs(SubtractQuan.Minutes);
            double Secs = Math.Abs(SubtractQuan.Seconds);
            if (Hours > 0)
            {
                if (Min == 0)
                    orderHeader.Duration = Hours.ToString() + "hours";
                else
                    orderHeader.Duration = Hours.ToString() + " h " + Min.ToString();
            }
            else if (Min > 0)
                orderHeader.Duration = Min.ToString() + " mins";
            else
                orderHeader.Duration = Secs.ToString() + " secs";




            var CustomerInfo = _db.ApplicationUser.Where(u => u.Id == orderHeader.UserId).FirstOrDefault();
            var CustomerEmail = CustomerInfo.Email;
            var CustomerName = CustomerInfo.Name;

            SendEmail(CustomerName, CustomerEmail, "Order : " + OrderId+" - Ready for pick up", "Order number "+OrderId+", is ready.<br />Your order awaits your arrival.", null,null,0);


            await _db.SaveChangesAsync();

            //will need email logic here
            //await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == orderHeader.UserId).FirstOrDefault().Email, "Spice - Order: " + orderHeader.Id.ToString() + " - Ready", "Order is ready for pickup");


            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize]
        public async Task<IActionResult> CusOrderCancel(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusCancelled;
            orderHeader.OrderCancelledBy = "Customer";

            var CustomerInfo = _db.ApplicationUser.Where(u => u.Id == orderHeader.UserId).FirstOrDefault();
            var CustomerEmail = CustomerInfo.Email;
            var CustomerName = CustomerInfo.Name;

            List<OrderDetails> objDetails = await _db.OrderDetails.Where(o => o.OrderId == OrderId).ToListAsync();
            // Send To customer
            SendEmail(CustomerName, CustomerEmail, "Order : " + OrderId + " - Cancelled", "You have successfully cancelled your order, the order details are as follows:", objDetails,orderHeader.CouponCode,orderHeader.CouponCodeDiscount);
            // Send to Admin
            SendEmail("Rendezvous Restaurant", "rendezvousrestaurantdut@gmail.com", "Order : " + OrderId + " - Customer Cancelled", "Customer Name: "+CustomerName+"<br />Customer Email: "+CustomerEmail+"<br /><br />Has decided to cancel their order, the order details are as follows:", objDetails, orderHeader.CouponCode, orderHeader.CouponCodeDiscount);
            
            await _db.SaveChangesAsync();

            //will need email logic here
            //await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == orderHeader.UserId).FirstOrDefault().Email, "Spice - Order: " + orderHeader.Id.ToString() + " - Cancelation", "Order has been Cancelled");

            return RedirectToAction(nameof(OrderHistory), "Order");
        }
        [Authorize]
        public async Task<IActionResult> SpecCusOrderCancel(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusCancelled;
            

            var CustomerInfo = _db.ApplicationUser.Where(u => u.Id == orderHeader.UserId).FirstOrDefault();
            var CustomerEmail = CustomerInfo.Email;
            var CustomerName = CustomerInfo.Name;

            orderHeader.OrderCancelledBy = CustomerName;

            List<OrderDetails> objDetails = await _db.OrderDetails.Where(o => o.OrderId == OrderId).ToListAsync();
            // Send To customer
            SendEmail(CustomerName, CustomerEmail, "Order : " + OrderId + " - Cancelled", "You have successfully cancelled your order, the order details are as follows:", objDetails, orderHeader.CouponCode, orderHeader.CouponCodeDiscount);
            // Send to Admin
            SendEmail("Rendezvous Restaurant", "rendezvousrestaurantdut@gmail.com", "Order : " + OrderId + " - Customer Cancelled", "Customer Name: " + CustomerName + "<br />Customer Email: " + CustomerEmail + "<br /><br />Has decided to cancel their order, the order details are as follows:", objDetails, orderHeader.CouponCode, orderHeader.CouponCodeDiscount);

            await _db.SaveChangesAsync();

            //will need email logic here
            //await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == orderHeader.UserId).FirstOrDefault().Email, "Spice - Order: " + orderHeader.Id.ToString() + " - Cancelation", "Order has been Cancelled");

            return RedirectToAction(nameof(OrderHistory), "Order");
        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderCancel(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusCancelled;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var StaffName = _db.ApplicationUser.Where(a => a.Id == claim.Value).SingleOrDefault().Name;

            orderHeader.OrderCancelledBy = StaffName;

            var CustomerInfo = _db.ApplicationUser.Where(u => u.Id == orderHeader.UserId).FirstOrDefault();
            var CustomerEmail = CustomerInfo.Email;
            var CustomerName = CustomerInfo.Name;

            List<OrderDetails> objDetails = await _db.OrderDetails.Where(o => o.OrderId == OrderId).ToListAsync();

            SendEmail(CustomerName, CustomerEmail, "Order : " + OrderId+" - Cancelled", "Your order has been cancelled, the order details are as follows:", objDetails,orderHeader.CouponCode,orderHeader.CouponCodeDiscount);
            await _db.SaveChangesAsync();

            //will need email logic here
            //await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == orderHeader.UserId).FirstOrDefault().Email, "Spice - Order: " + orderHeader.Id.ToString() + " - Cancelation", "Order has been Cancelled");

            return RedirectToAction("ManageOrder", "Order");
        }


        public async Task<IActionResult> GetOrderDetails(int id)
        {
            OrderDetailsViewModel orderDetailsVM = new OrderDetailsViewModel()
            {
                OrderHeader = await _db.OrderHeader.FirstOrDefaultAsync(m => m.Id == id),
                OrderDetails = await _db.OrderDetails.Where(m => m.OrderId == id).ToListAsync()

            };

            orderDetailsVM.OrderHeader.ApplicationUser = await _db.ApplicationUser.FirstOrDefaultAsync(u => u.Id == orderDetailsVM.OrderHeader.UserId);

            return PartialView("_IndividualOrderDetails", orderDetailsVM);


        }

        [Authorize]
        public async Task<IActionResult> OrderPickup(int productPage = 1, string searchEmail = null, string searchName = null, string searchPhone = null)
        {
            //var claimsIdentity = (ClaimsIdentity)User.Identity;
            //var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            OrderListViewModel orderListVM = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()
            };

            StringBuilder param = new StringBuilder();
            param.Append("/Customer/Order/OrderPickup?productPage=:");

            param.Append("&searchName=");
            if (searchName != null)
            {
                param.Append(searchName);
            }

            param.Append("&searchEmail=");
            if (searchEmail != null)
            {
                param.Append(searchEmail);
            }

            param.Append("&searchPhone=");
            if (searchPhone != null)
            {
                param.Append(searchPhone);
            }


            List<OrderHeader> orderHeadersList = new List<OrderHeader>();

            if (searchName != null || searchEmail != null || searchPhone != null)
            {
                var user = new ApplicationUser();

                if (searchName != null)
                {
                    orderHeadersList = await _db.OrderHeader.Include(o => o.ApplicationUser)
                                                            .Where(u => u.PickUpName.ToLower()
                                                            .Contains(searchName.ToLower()))
                                                            .OrderByDescending(o => o.OrderDate).ToListAsync();
                }
                else
                {
                    if (searchEmail != null)
                    {
                        user = await _db.ApplicationUser.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower())).FirstOrDefaultAsync();
                        if (user != null)
                            orderHeadersList = await _db.OrderHeader.Include(o => o.ApplicationUser)
                                                                    .Where(o => o.UserId == user.Id)
                                                                    .OrderByDescending(o => o.OrderDate).ToListAsync();
                        else
                            orderHeadersList = await _db.OrderHeader.Include(o => o.ApplicationUser)
                                                                .Where(o => o.UserId == "")
                                                                .OrderByDescending(o => o.OrderDate).ToListAsync();
                    }
                    else
                    {
                        if (searchPhone != null)
                        {
                            orderHeadersList = await _db.OrderHeader.Include(o => o.ApplicationUser)
                                                                    .Where(u => u.PhoneNumber.Contains(searchPhone))
                                                                    .OrderByDescending(o => o.OrderDate).ToListAsync();
                        }
                    }
                }
            }
            else
            {
                orderHeadersList = await _db.OrderHeader.Include(o => o.ApplicationUser).Where(u => u.Status == SD.StatusReady).ToListAsync();
            }

            foreach (OrderHeader item in orderHeadersList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == item.Id).ToListAsync()
                };

                orderListVM.Orders.Add(individual);
            }

            var count = orderListVM.Orders.Count;
            orderListVM.Orders = orderListVM.Orders.OrderByDescending(p => p.OrderHeader.Id)
                                .Skip((productPage - 1) * PageSize)
                                .Take(PageSize).ToList();


            orderListVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemPerPage = PageSize,
                TotalItems = count,
                UrlParam = param.ToString()

            };
            return View(orderListVM);
        }

        [Authorize(Roles = SD.FrontDeskUser + "," + SD.ManagerUser)]
        [HttpPost]
        [ActionName("OrderPickup")]
        public async Task<IActionResult> OrderPickupPost(int orderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(orderId);
            orderHeader.Status = SD.StatusCompleted;
            orderHeader.PickedUpOrder = DateTime.Now;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var StaffName = _db.ApplicationUser.Where(a => a.Id == claim.Value).SingleOrDefault().Name;

            orderHeader.OrderCompletedBy = StaffName;

            var CustomerInfo = _db.ApplicationUser.Where(u => u.Id == orderHeader.UserId).FirstOrDefault();
            var CustomerEmail = CustomerInfo.Email;
            var CustomerName = CustomerInfo.Name;

            SendEmail(CustomerName, CustomerEmail, "Order : " + orderId+" - Completed", "We see that you have picked your order.<br />We hope that you enjoy your meal.", null,null,0);

            await _db.SaveChangesAsync();
            return RedirectToAction("OrderPickup", "Order");
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FilterBy(DateTime? datepicker)
        {
            if (datepicker == null)
                return RedirectToAction(nameof(ManageOrder));


            List<OrderDetailsViewModel> objODVM = new List<OrderDetailsViewModel>();
            List<OrderHeader> orderHeadersList = await _db.OrderHeader
                .Where(o => ((o.Status == SD.StatusSubmitted || o.Status == SD.StatusInProcess) && o.PickUpTime.Date == datepicker.Value.Date))
                .OrderByDescending(o => o.PickUpTime).ToListAsync();
            foreach (OrderHeader item in orderHeadersList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == item.Id).ToListAsync()
                };

                objODVM.Add(individual);
            }

            return View("ManageOrder", objODVM.OrderBy(o => o.OrderHeader.PickUpTime).ToList());

        }

        public void SendEmail(string Name, string Email, string Sub, string Body ,List<OrderDetails> objDetails, string Coupon, double? CouponDiscount)
        {
            try
            {
                var BusEmail = new MailAddress("rendezvousrestaurantdut@gmail.com", "Rendezvous Restaurant");
                var email = new MailAddress(Email, Name);
                var pass = "DUTRendezvous123";
                var subject = Sub;
                var body = "Good day, <strong>" + Name
                    + "</strong>.<br /><br />" + Body;

                double TotalPrice = 0.0;
                if (objDetails != null)
                { 
                    body += "<br /><br /><table border =" + 1 + " cellpadding=" + 0 + " cellspacing=" + 0 + " width = " + 400 + "><tr><th>Item</th><th>Quantity</th><th>Price</th><th>Total Price</th></tr>";
                    foreach (var item in objDetails)
                    {
                        body += "<tr><td>"+ item.Name+"</td><td>"+item.Count+"</td><td>"+item.Price.ToString("C")+"</td><td>";
                        double PriceQuan = item.Count * item.Price;
                        body += PriceQuan.ToString("C") + "</td></tr>";
                        TotalPrice += PriceQuan;

                    }
                    body += "<tr height ="+10+"><td colspan = " + 4 + "></td></tr>";
                    if (!string.IsNullOrEmpty(Coupon) && CouponDiscount != null)
                    {
                        body += "<tr><td colspan = " + 3 + ">Coupon:</td><td>" + Coupon + "</td></tr>"
                            + "<tr><td colspan = " + 3 + ">Discount:</td><td>-"+CouponDiscount.Value.ToString("C")+"</td></tr>"
                            + "<tr><td colspan = " + 3 + ">Total(Excl Discount):</td><td>"+TotalPrice.ToString("C")+"</td></tr>";
                    }
                    if (CouponDiscount != null)
                        body += "<tr><td colspan = " + 3 + "><b>TOTAL:</b></td><td>" + (TotalPrice - CouponDiscount.Value).ToString("C") + "</td></tr></table>";
                    else
                        body += "<tr><td colspan = " + 3 + "><b>TOTAL:</b></td><td>" + TotalPrice.ToString("C") + "</td></tr></table>";
                }

                body += "<br /><br />Have A Lovely day.<br/>The Rendezvous-Restaurant Team.";
                // var body = "Good day, " + Name + "\n\n" + Body+"\n\n" +OrderDetailsBody + "\nHave a Lovely day.\nThe Rendezvous-Restaurant Team.";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(BusEmail.Address, pass)
                };
                using (var message = new MailMessage(BusEmail, email)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
              
            }


        }
    }



}

