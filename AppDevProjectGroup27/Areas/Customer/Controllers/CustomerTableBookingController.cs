using AppDevProjectGroup27.Data;
using AppDevProjectGroup27.Models;
using AppDevProjectGroup27.Models.ViewModels;
using AppDevProjectGroup27.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]
    public class CustomerTableBookingController : Controller
    {
        private ApplicationDbContext _db;

        private readonly IWebHostEnvironment _hostEnviroment;

        public CustomerTableBookingController(ApplicationDbContext db, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _hostEnviroment = hostEnvironment;
        }


        public async Task<IActionResult> Index()
        {
            CustomerBookingVM objCB = new CustomerBookingVM
            {
                TableTracks = null,
                ChosenTableId = 0,
                Quantity = 0
            };
            var TableObj = await _db.Table.AnyAsync();
            if (TableObj) objCB.TableList = await _db.Table.Where(t => t.Active == true).OrderBy(t=>t.SeatingName).Select(t => new SelectListItem
            {
                Text = t.SeatingName,
                Value = t.Id.ToString()
            }).ToListAsync();
            else
                objCB.TableList = new List<SelectListItem>();

            return View(objCB);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CustomerBookingVM objCB)
        {
            if (objCB.ChosenTableId <= 0 || objCB.DateChosen == null || objCB.TimeChosen == null)
                return RedirectToAction(nameof(Index));

            var FindIdInTableTrack = _db.TableTrack.Include(t => t.Table).Where(t => t.TableId == objCB.ChosenTableId
             && t.DateTable.Date == objCB.DateChosen.Value.Date
             && t.TimeTable == objCB.TimeChosen.Value.TimeOfDay);

            objCB.TableTracks = new TableTrack();

            if (FindIdInTableTrack.Any())
                objCB.TableTracks = await FindIdInTableTrack.FirstAsync();
            else
            {
                var GetTable = await _db.Table.FindAsync(objCB.ChosenTableId);
                int MaxQuan = GetTable.MaxTables;
                TableTrack tableTrack = new TableTrack
                {
                    TableId = objCB.ChosenTableId,
                    AmtAva = MaxQuan,
                    DateTable = objCB.DateChosen.Value.Date,
                    TimeTable = objCB.TimeChosen.Value.TimeOfDay
                };

                _db.TableTrack.Add(tableTrack);
                await _db.SaveChangesAsync();
                objCB.TableTracks = await _db.TableTrack.Include(t => t.Table).Where(t => t.Id == tableTrack.Id).Select(t => t).FirstAsync();
            }

            var TableObj = await _db.Table.AnyAsync();
            if (TableObj) objCB.TableList = await _db.Table.Where(t => t.Active == true).OrderBy(t=>t.SeatingName).Select(t => new SelectListItem
            {
                Text = t.SeatingName,
                Value = t.Id.ToString()
            }).ToListAsync();
            else
                objCB.TableList = new List<SelectListItem>();

            objCB.Quantity = 0;

            return View(objCB);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToTable(int? Quantity, int? TableTrackId)
        {
            if (Quantity.HasValue)
            {
                if (Quantity.Value <= 0)
                    return RedirectToAction(nameof(Index));
            }
            if (!Quantity.HasValue || !TableTrackId.HasValue)
            {
                return RedirectToAction(nameof(Index));
            }


            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var tableTrack = await _db.TableTrack.Include(t => t.Table).Where(t => t.Id == TableTrackId.Value).FirstOrDefaultAsync();

            if (tableTrack == null) return RedirectToAction(nameof(Index));

            TableBookingHeader objTBH = new TableBookingHeader
            {
                UserId = claim.Value,
                DateBookingMade = SharedMethods.GetDateTime(),
                SitInDate = tableTrack.DateTable,
                SitInTime = tableTrack.TimeTable,
                TableName = tableTrack.Table.SeatingName,
                TableBooked = Quantity.Value,
                Status = SD.TableStatusSubmitted,
                BookStatus = SD.BookTableStatusPending,
                ApprovedBy = "",
                RejectedBy = "",
                TimeApproved = null,
                TimeCheckOut = null,
                TimeRejected = null,
                TimeSitIn = null,
                Duration = ""
            };

            await _db.TableBookingHeader.AddAsync(objTBH);

            TableTrack objTT = await _db.TableTrack.FindAsync(tableTrack.Id);
            objTT.AmtAva -= Quantity.Value;

            await _db.SaveChangesAsync();

            //Send Email
            var CustomerInfo = _db.ApplicationUser.Where(u => u.Id == claim.Value).FirstOrDefault();
            var CustomerEmail = CustomerInfo.Email;
            var CustomerName = CustomerInfo.Name;


            SendEmail(CustomerName, CustomerEmail, "Table Booking : " + objTBH.Id + " - Received", "Your Table Booking has been received","Thank you for your table booking, it is awaiting approval from the Admin.", objTBH);
            //End Send Email

            return View("Confirm", objTBH);
        }

        public void SendEmail(string Name, string Email, string Sub, string Heading, string Body, TableBookingHeader objDetails)
        {
            try
            {
                var BusEmail = new MailAddress("rendezvousrestaurantdut@gmail.com", "Rendezvous Restaurant");
                var email = new MailAddress(Email, Name);
                var pass = "DUTRendezvous123";
                var subject = Sub;
                //  var body = "Good day, <strong>" + Name
                //   + "</strong>.<br /><br />" + Body;

                var PathToFile = _hostEnviroment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                  + "Templates" + Path.DirectorySeparatorChar.ToString() + "EmailTemplates"
                  + Path.DirectorySeparatorChar.ToString() + "TableTemplate.htm";

                string HtmlBody = "";
                using (StreamReader streamReader = System.IO.File.OpenText(PathToFile))
                {
                    HtmlBody = streamReader.ReadToEnd();
                }

                string Details = "";
                if (objDetails != null)
                {
                    Details += "<br /><br /><table border =" + 1 + " cellpadding=" + 0 + " cellspacing=" + 0 + " width = " + 400 + "><tr><th>Table Name</th><th>Number of Tables Booked</th><th>Date</th><th>Time</th></tr>";
                    Details += "<tr><td>" + objDetails.TableName + "</td><td>" + objDetails.TableBooked + "</td><td>" + objDetails.SitInDate.ToString("MM/dd/yyyy") + "</td><td>";
                    Details += objDetails.SitInTime.ToString(@"hh\:mm") + "</td></tr></table>";
                }

                HtmlBody = HtmlBody.Replace("#heading#", Heading);
                HtmlBody = HtmlBody.Replace("#details#", Body);
                HtmlBody = HtmlBody.Replace("#torder#", Details);
                HtmlBody = HtmlBody.Replace("#final#", "<br /><br />Have A Lovely day.<br/>The Rendezvous-Restaurant Team.");
                // body += "<br /><br />Have A Lovely day.<br/>The Rendezvous-Restaurant Team.";

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
                    Body = HtmlBody,
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
