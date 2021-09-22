﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppDevProjectGroup27.Data;
using AppDevProjectGroup27.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Net;

namespace AppDevProjectGroup27.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]
    [Area("Admin")]
    public class SpecialController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public SpecialController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        //Get
        public async Task<IActionResult> Index()
        {
            ViewBag.Newsletter = "";
            return View(await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.OnSpecial == true).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string subject)
        {
            ViewBag.Newsletter = "";
            if (string.IsNullOrWhiteSpace(subject))
                return RedirectToAction(nameof(Index));

            else
            {

                var AllCustomersEmails = _userManager.GetUsersInRoleAsync(SD.CustomerEndUser).Result.ToList();
                if (AllCustomersEmails.Count() > 0)
                {
                    List<string> AllCustomersNames = new List<string>(0);
                    try
                    {
                        foreach (var item in AllCustomersEmails)
                        {
                            var tempName = await _db.ApplicationUser.Where(a => a.Email == item.Email).Select(a => a.Name).SingleAsync();
                            AllCustomersNames.Add(tempName);
                        }
                        if (AllCustomersNames.Count > 0)
                        {
                            for (int x = 0; x < AllCustomersNames.Count(); x++)
                            {
                                if (SendEmail(AllCustomersNames[x], AllCustomersEmails[x].Email, subject, "") == true)
                                {
                                    ViewBag.Newsletter = "Emails Sent to Customers Successfully.";
                                }
                                else
                                {
                                    ViewBag.Newsletter = "Unable to Send Emails to Customers.";
                                    return View(await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.OnSpecial == true).ToListAsync());
                                }
                                // In the above send email, the last parameter is left blank until 
                                // Code for the Body is done successfully
                                // Also note the SendEmail will not an email currently, because smtp.SendMessage() is commented below.
                            }
                        }
                        else
                        {
                            ViewBag.Newsletter = "There are no Customer Names.";
                        }
                    }
                    catch
                    {
                        ViewBag.Newsletter = "Unable to retrieve Customers' Names.";
                    }
                }
                else
                {
                    ViewBag.Newsletter = "No Customers exist to send the Newsletter to.";
                }


                // CODE TO SEND EMAIL TO ALL CUSTOMERS
                return View(await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.OnSpecial == true).ToListAsync());
            }
        }
        public bool SendEmail(string Name, string Email, string Sub, string BodMessage)
        {
            try
            {
                var BusEmail = new MailAddress("<Email the Business>", "<Name of the Business>");
                var email = new MailAddress(Email, Name);
                var pass = "<Password for the business>";
                var subject = Sub;
                var body = BodMessage;
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
                    Body = body
                })
                {
                    smtp.Send(message);
                }
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}
