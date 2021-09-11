﻿using AppDevProjectGroup27.Data;
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
using System.Text;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {

        private ApplicationDbContext _db;
        private int PageSize = 2;
        public OrderController(ApplicationDbContext db)
        {
            _db = db;
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

        [Authorize]
        public async Task<IActionResult> OrderHistory(int productPage=1)
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

            await _db.SaveChangesAsync();
            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderReady(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusReady;

            await _db.SaveChangesAsync();

            //will need email logic here
            //await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == orderHeader.UserId).FirstOrDefault().Email, "Spice - Order: " + orderHeader.Id.ToString() + " - Ready", "Order is ready for pickup");


            return RedirectToAction("ManageOrder", "Order");
        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderCancel(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusCancelled;

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
                        orderHeadersList = await _db.OrderHeader.Include(o => o.ApplicationUser)
                                                                .Where(o => o.UserId == user.Id)
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
            await _db.SaveChangesAsync();
            return RedirectToAction("OrderPickup", "Order");
        }

        
    }
}
