using AppDevProjectGroup27.Data;
using AppDevProjectGroup27.Models;
using AppDevProjectGroup27.Models.ViewModels;
using AppDevProjectGroup27.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TableController : Controller
    {

        public readonly ApplicationDbContext _db;

        private readonly IWebHostEnvironment _hostingEnvironment;
        public TableController(ApplicationDbContext db, IWebHostEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
        }


        [TempData]
        public string StatusMessage { get; set; }


        public async Task<IActionResult> Index()
        {
            return View(await _db.Table.ToListAsync());
        }

        //Get - Create
        public IActionResult Create()
        {
            TableCreateVM objT = new TableCreateVM();
            return View(objT);
        }

        //Post - Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TableCreateVM objTable)
        {
            if (objTable.Table.SeatingName == null)
            {
                ModelState.AddModelError("Table.SeatingName", "Please enter a number");
                return View(objTable);
            }
            if (ModelState.IsValid)
            {
                string TableName = objTable.Table.SeatingName + " Seater";
                var DoesTableAlreadyExist = _db.Table.Where(t => t.SeatingName == TableName);
                if (DoesTableAlreadyExist.Any())
                {
                    StatusMessage = "Error : A " + TableName + " Table already exists.";
                }
                else
                {
                    objTable.Table.SeatingName += " Seater";
                    _db.Table.Add(objTable.Table);
                    await _db.SaveChangesAsync();

                    // BEGINNING OF IMAGE
                    //Image saving Section
                    //Name of image will be the ID of the menuItem number


                    //Extracting the root path for where images will be saved
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    var files = HttpContext.Request.Form.Files;

                    //Recieving MenuItem From DB
                    var tableFromDb = await _db.Table.FindAsync(objTable.Table.Id);

                    if (files.Count > 0)
                    {
                        //File was upload

                        //Combining the webroot path with images
                        var uploads = Path.Combine(webRootPath, @"images\Table");
                        var extension = Path.GetExtension(files[0].FileName);

                        using (var filesStream = new FileStream(Path.Combine(uploads, objTable.Table.Id + extension), FileMode.Create))
                        {
                            //will copy the image to the file location on the sever and rename it
                            files[0].CopyTo(filesStream);
                        }

                        //In database the name will be changed to te loctaion where the image is saved
                        tableFromDb.Image = @"\images\Table\" + objTable.Table.Id + extension;

                    }
                    else
                    {
                        //No file uploaded so default will be used
                        var uploads = Path.Combine(webRootPath, @"images\Table\" + SD.DefaultTableImage);
                        System.IO.File.Copy(uploads, webRootPath + @"\images\Table\" + objTable.Table.Id + ".png");

                        tableFromDb.Image = @"\images\Table\" + objTable.Table.Id + ".png";

                    }

                    await _db.SaveChangesAsync();

                    //END of IMAGE
                    return RedirectToAction(nameof(Index));
                }

            }
            objTable.StatusMessage = StatusMessage;
            return View(objTable);
        }

        //Get-Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var table = await _db.Table.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            var integerValue = GetIntValue(table.SeatingName);

            if (integerValue == 0) return NotFound();

            table.SeatingName = integerValue.ToString();
            TableEditVM objT = new TableEditVM { Table = table, OldMaxValue = table.MaxTables };

            return View(objT);
        }

        /*
         ADMIN:
            ID: 1
            Name: 1 Seater     >> 1 Seater 
            Max Value: 5 Tables
            Available: 2 Tables
            ID: 2
            Name: 2 Seater
            Max Value: 4 Tables
            Available: 1 Tables
        New:
            Name: 1 Seater
            Max Value: 3 Tables
            Available: 0 Tables
         
            AVA --- NEW MAX VALUE  - OLD MAX VALUE
                        3          -       5        =   -2
            AVAILABLE += AVA // -2;       ----->>>> 0
           
         
         */

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(TableEditVM objTable)
        {
            if (ModelState.IsValid)
            {
                if (objTable.Table.MaxTables != objTable.OldMaxValue)
                {
                    int SubtractTab = objTable.Table.MaxTables - objTable.OldMaxValue;

                    var TableTrackObj = await _db.TableTrack.Include(t => t.Table).Where(t => t.TableId == objTable.Table.Id).ToListAsync();

                    if (TableTrackObj.Any())
                    {
                        foreach (var item in TableTrackObj)
                        {
                            TableTrack objTT = await _db.TableTrack.FindAsync(item.Id);
                            objTT.AmtAva += SubtractTab;
                            if (objTT.AmtAva < 0) objTT.AmtAva = 0;
                        }
                    }
                }

                var TableFromDb = await _db.Table.FindAsync(objTable.Table.Id);

                //Begin Image
                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    //New Image File was upload

                    //Combining the webroot path with images
                    var uploads = Path.Combine(webRootPath, @"images\Table");
                    var extension_new = Path.GetExtension(files[0].FileName);

                    // Delete Image File that was in Database
                    var imagePath = Path.Combine(webRootPath, TableFromDb.Image.TrimStart('\\'));

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                    // New Image File will be uploaded
                    using (var filesStream = new FileStream(Path.Combine(uploads, objTable.Table.Id + extension_new), FileMode.Create))
                    {
                        //will copy the image to the file location on the sever and rename it
                        files[0].CopyTo(filesStream);
                    }

                    //In database the name will be changed to te loctaion where the image is saved
                    TableFromDb.Image = @"\images\Table\" + objTable.Table.Id + extension_new;

                }
                //End Image


                TableFromDb.MaxTables = objTable.Table.MaxTables;
                TableFromDb.Active = objTable.Table.Active;

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(objTable);
        }

        //Get-Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var table = await _db.Table.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }
            return View(table);

        }

        //Post-Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var table = await _db.Table.FindAsync(id);
            if (table == null)
            {
                return View();
            }

            // Delete Image
            string webRootPath = _hostingEnvironment.WebRootPath;
            var imagePath = Path.Combine(webRootPath, table.Image.TrimStart('\\'));

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            //End Delete Image
            // Send Email to All pending and approved bookings for specific item that is being deleted
            // Inform Customers that the table is no longer available etc.

            /*
             Following code is to remove all table tracks in TableTrack that is linked to the table being
            deleted.
             */
            var CheckTableTrack = await _db.TableTrack.Where(t => t.TableId == id).ToListAsync();
            if (CheckTableTrack.Any())
            {
                foreach (var item in CheckTableTrack)
                {
                    TableTrack tableTrack = await _db.TableTrack.FindAsync(item.Id);
                    _db.TableTrack.Remove(tableTrack);
                }
            }

            /*
            Following code is to cancel/reject all bookings that have the soon to be deleted table name
            */

            //Reject Pending Ones:
            var CheckTableHeaderPending = await _db.TableBookingHeader.Where(t => t.TableName == table.SeatingName && t.Status != SD.TableStatusCancelled && t.BookStatus == SD.BookTableStatusPending && t.SitInDate.Date >= DateTime.Now.Date).ToListAsync();
            if (CheckTableHeaderPending.Any())
            {
                foreach (var item in CheckTableHeaderPending)
                {
                    TableBookingHeader tableHeader = _db.TableBookingHeader.Find(item.Id);
                    tableHeader.BookStatus = SD.BookTableStatusRejected;
                    tableHeader.TimeRejected = DateTime.Now;

                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var StaffName = _db.ApplicationUser.Where(a => a.Id == claim.Value).SingleOrDefault().Name;

                    tableHeader.RejectedBy = StaffName;
                    // Send Email saying their booking was rejected because table is unavailable
                    var CustomerInfo = _db.ApplicationUser.Where(u => u.Id == tableHeader.UserId).FirstOrDefault();
                    var CustomerEmail = CustomerInfo.Email;
                    var CustomerName = CustomerInfo.Name;

                    SendEmail(CustomerName, CustomerEmail, "Table Booking : "+ item.Id +" - Rejected", "The following table booking has been rejected:", tableHeader);
                }
            }

            //Cancel Approved Ones:
            var CheckTableHeaderApproved = await _db.TableBookingHeader.Where(t => t.TableName == table.SeatingName && t.BookStatus == SD.BookTableStatusApproved && t.Status == SD.TableStatusSubmitted && t.SitInDate.Date > DateTime.Now.Date).ToListAsync();
            if (CheckTableHeaderApproved.Any())
            {
                foreach (var item in CheckTableHeaderApproved)
                {
                    TableBookingHeader tableHeader = _db.TableBookingHeader.Find(item.Id);
                    tableHeader.Status = SD.TableStatusCancelled;
                    tableHeader.TimeRejected = DateTime.Now;
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    var StaffName = _db.ApplicationUser.Where(a => a.Id == claim.Value).SingleOrDefault().Name;

                    tableHeader.RejectedBy = StaffName;
                    // Send Email saying their booking was cancelled because table is unavailable
                    var CustomerInfo = _db.ApplicationUser.Where(u => u.Id == tableHeader.UserId).FirstOrDefault();
                    var CustomerEmail = CustomerInfo.Email;
                    var CustomerName = CustomerInfo.Name;

                    SendEmail(CustomerName, CustomerEmail, "Table Booking : " + item.Id + " - Cancelled", "The following table booking has been cancelled, we apologise for the inconvenience:", tableHeader);
                }
            }




            _db.Table.Remove(table);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        //GET - Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
            var table = await _db.Table.FindAsync(id);
            if (table == null)
                return NotFound();

            return View(table);
        }

        public int GetIntValue(string Temp)
        {
            var integerValue = 0;
            var numericString = new string(Temp.Where(x => char.IsDigit(x)).ToArray());
            int.TryParse(numericString, out integerValue);
            return integerValue;

        }

        public void SendEmail(string Name, string Email, string Sub, string Body, TableBookingHeader objDetails)
        {
            try
            {
                var BusEmail = new MailAddress("rendezvousrestaurantdut@gmail.com", "Rendezvous Restaurant");
                var email = new MailAddress(Email, Name);
                var pass = "DUTRendezvous123";
                var subject = Sub;
                var body = "Good day, <strong>" + Name
                    + "</strong>.<br /><br />" + Body;

                    body += "<br /><br /><table border =" + 1 + " cellpadding=" + 0 +  " width = " + 100 + "%><tr><th>Table Name</th><th>Number of Tables Booked</th><th>Date</th><th>Time</th></tr>";
                        body += "<tr><td>" + objDetails.TableName + "</td><td>" + objDetails.TableBooked + "</td><td>" + objDetails.SitInDate.ToString("MM/dd/yyyy") + "</td><td>";
                        body += objDetails.SitInTime.ToString(@"hh\:mm") + "</td></tr></table>";

                body += "<br /><br />Have A Lovely day.<br/>The Rendezvous-Restaurant Team.";

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
