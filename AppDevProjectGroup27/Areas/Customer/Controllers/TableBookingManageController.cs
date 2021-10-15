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
   
    [Area("Customer")]
    public class TableBookingManageController : Controller
    {

        private ApplicationDbContext _db;
        public TableBookingManageController(ApplicationDbContext db)
        {
            _db = db;
        }


        // DONT FORGET: When table booking is involved customers cannot eat in one hour before final meal (Remove 9PM time)

        [Authorize(Roles = SD.FrontDeskUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> ARBooking(string message = "")
        {
            List<TableBookingHeader> tableBookingHeaderList = await _db.TableBookingHeader
                .Where(t => t.BookStatus == SD.BookTableStatusPending && t.Status != SD.TableStatusCancelled).OrderBy(t => t.SitInDate).ThenBy(t => t.SitInTime).ToListAsync();

            TableARVM objTArm = new TableARVM { tableBookingHeaders = tableBookingHeaderList, StatusMessage = message };

            return View(objTArm);
        }
        [Authorize(Roles = SD.FrontDeskUser + "," + SD.ManagerUser)]
        public IActionResult Approve(int TableHeaderId)
        {
            TableBookingHeader tableHeader = _db.TableBookingHeader.Find(TableHeaderId);
            tableHeader.BookStatus = SD.BookTableStatusApproved;
            tableHeader.TimeApproved = DateTime.Now;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var StaffName = _db.ApplicationUser.Where(a => a.Id == claim.Value).SingleOrDefault().Name;

            tableHeader.ApprovedBy = StaffName;

            _db.SaveChanges();

            // Send Email To Customer that their Booking was approved
            //To Get Email:
            // var Email = _db.ApplicationUser.Find(tableHeader.UserId).Email;
            // End of Get Email (Please check if this code works first

            string Message = "Table Booking Approved";

            return RedirectToAction(nameof(ARBooking), new { message = Message });
        }
        [Authorize(Roles = SD.FrontDeskUser + "," + SD.ManagerUser)]
        public IActionResult Reject(int TableHeaderId)
        {
            // Add Quantity back to Table -  if Available
            if (!UpdateQuantity(TableHeaderId))
            {
                return NotFound();
                // Or Can Return Error Message
            }
            TableBookingHeader tableHeader = _db.TableBookingHeader.Find(TableHeaderId);
            tableHeader.BookStatus = SD.BookTableStatusRejected;
            tableHeader.TimeRejected = DateTime.Now;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var StaffName = _db.ApplicationUser.Where(a => a.Id == claim.Value).SingleOrDefault().Name;

            tableHeader.RejectedBy = StaffName;

            _db.SaveChanges();

            // Send Email To Customer that their Booking was rejected
            //To Get Email:
            // var Email = _db.ApplicationUser.Find(tableHeader.UserId).Email;
            // End of Get Email (Please check if this code works first

            return RedirectToAction(nameof(ARBooking), new { message = "Table Booking Rejected" });
        }

        [Authorize(Roles = SD.FrontDeskUser + "," + SD.ManagerUser)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FilterByAR(DateTime? datepicker, DateTime? timepicker)
        {
            List<TableBookingHeader> tableBookingHeaderList = new List<TableBookingHeader>();
            if ((datepicker == null && timepicker == null) || (datepicker == null && timepicker != null))
                return RedirectToAction(nameof(ARBooking));
            else if (datepicker != null && timepicker == null)
                tableBookingHeaderList = await _db.TableBookingHeader
                .Where(t => t.BookStatus == SD.BookTableStatusPending && t.SitInDate.Date == datepicker.Value.Date).OrderBy(t => t.SitInDate).ThenBy(t => t.SitInTime).ToListAsync();
            else if (datepicker != null && timepicker != null)
                tableBookingHeaderList = await _db.TableBookingHeader
                .Where(t => t.BookStatus == SD.BookTableStatusPending && t.SitInDate.Date == datepicker.Value.Date && t.SitInTime == timepicker.Value.TimeOfDay).OrderBy(t => t.SitInDate).ThenBy(t => t.SitInTime).ToListAsync();


            TableARVM objTArm = new TableARVM { tableBookingHeaders = tableBookingHeaderList, StatusMessage = "" };
            return View(nameof(ARBooking), objTArm);

        }

        [Authorize(Roles = SD.FrontDeskUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> ManageBooking()
        {
            List<TableBookingHeader> tableBookingHeaderList = await _db.TableBookingHeader.Where(t => t.Status != SD.TableStatusCancelled && t.Status != SD.TableStatusCompleted && t.BookStatus == SD.BookTableStatusApproved).OrderBy(t => t.SitInDate).ThenBy(t => t.SitInTime).ToListAsync();
            return View(tableBookingHeaderList);
        }

        [Authorize(Roles = SD.FrontDeskUser + "," + SD.ManagerUser)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FilterBy(DateTime? datepicker, DateTime? timepicker)
        {
            List<TableBookingHeader> tableBookingHeaderList = new List<TableBookingHeader>();
            if ((datepicker == null && timepicker == null) || (datepicker == null && timepicker != null))
                return RedirectToAction(nameof(ManageBooking));
            else if (datepicker != null && timepicker == null)
                tableBookingHeaderList = await _db.TableBookingHeader.Where(t => t.Status != SD.TableStatusCancelled && t.Status != SD.TableStatusCompleted && t.BookStatus == SD.BookTableStatusApproved && t.SitInDate.Date == datepicker.Value.Date).OrderBy(t => t.SitInDate).ThenBy(t => t.SitInTime).ToListAsync();
            else if (datepicker != null && timepicker != null)
                tableBookingHeaderList = await _db.TableBookingHeader.Where(t => t.Status != SD.TableStatusCancelled && t.Status != SD.TableStatusCompleted && t.BookStatus == SD.BookTableStatusApproved && t.SitInDate.Date == datepicker.Value.Date && t.SitInTime == timepicker.Value.TimeOfDay).OrderBy(t => t.SitInDate).ThenBy(t => t.SitInTime).ToListAsync();


            return View(nameof(ManageBooking), tableBookingHeaderList);

        }

        [Authorize(Roles = SD.FrontDeskUser + "," + SD.ManagerUser)]
        public IActionResult StartSitIn(int TableHeaderId)
        {
            TableBookingHeader tableHeader = _db.TableBookingHeader.Find(TableHeaderId);
            tableHeader.Status = SD.TableStatusStart;
            tableHeader.TimeSitIn = DateTime.Now;
            _db.SaveChanges();
            return RedirectToAction(nameof(ManageBooking));
        }

        [Authorize(Roles = SD.FrontDeskUser + "," + SD.ManagerUser)]
        public IActionResult CompleteSitIn(int TableHeaderId)
        {
            // Add Quantity back to Table Available
            if (!UpdateQuantity(TableHeaderId))
            {
                return NotFound();
                // Or Can Return Error Message
            }

            TableBookingHeader tableHeader = _db.TableBookingHeader.Find(TableHeaderId);
            tableHeader.Status = SD.TableStatusCompleted;
            tableHeader.TimeCheckOut = DateTime.Now;

            TimeSpan SubtractQuan = tableHeader.TimeCheckOut.Value.Subtract(tableHeader.TimeSitIn.Value);
            double Hours = Math.Abs(SubtractQuan.Hours);
            double Min = Math.Abs(SubtractQuan.Minutes);
            double Secs = Math.Abs(SubtractQuan.Seconds);
            if (Hours > 0)
            {
                if (Min == 0)
                    tableHeader.Duration = Hours.ToString() + "hours";
                else
                    tableHeader.Duration = Hours.ToString() + " h " + Min.ToString();
            }
            else if (Min > 0)
                tableHeader.Duration = Min.ToString() + " mins";
            else
                tableHeader.Duration = Secs.ToString() + " secs";


            _db.SaveChanges();


            //Email Logic to thank user for eating with us

            return RedirectToAction(nameof(ManageBooking));
        }

        [Authorize(Roles = SD.FrontDeskUser + "," + SD.ManagerUser)]
        public IActionResult CancelBooking(int TableHeaderId)
        {
            // Add Quantity back to Table Available
            if (!UpdateQuantity(TableHeaderId))
            {
                return NotFound();
                // Or Can Return Error Message
            }
            TableBookingHeader tableHeader = _db.TableBookingHeader.Find(TableHeaderId);
            tableHeader.Status = SD.TableStatusCancelled;
            tableHeader.TimeRejected = DateTime.Now;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var StaffName = _db.ApplicationUser.Where(a => a.Id == claim.Value).SingleOrDefault().Name;

            tableHeader.RejectedBy = StaffName;

            _db.SaveChanges();
            return RedirectToAction(nameof(ManageBooking));
        }

        [Authorize(Roles = SD.FrontDeskUser + "," + SD.ManagerUser)]
        public IActionResult THCusCancelBooking(int TableHeaderId)
        {
            // Add Quantity back to Table Available
            if (!UpdateQuantity(TableHeaderId))
            {
                return NotFound();
                // Or Can Return Error Message
            }
            TableBookingHeader tableHeader = _db.TableBookingHeader.Find(TableHeaderId);
            tableHeader.Status = SD.TableStatusCancelled;
            tableHeader.TimeRejected = DateTime.Now;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var StaffName = _db.ApplicationUser.Where(a => a.Id == claim.Value).SingleOrDefault().Name;

            tableHeader.RejectedBy = StaffName;

            _db.SaveChanges();
            return RedirectToAction("Index", "TableBookingHistory");
        }

        [Authorize]
        public IActionResult CusCancelBooking(int TableHeaderId)
        {
            // Add Quantity back to Table Available
            if (!UpdateQuantity(TableHeaderId))
            {
                return NotFound();
                // Or Can Return Error Message
            }
            TableBookingHeader tableHeader = _db.TableBookingHeader.Find(TableHeaderId);
            tableHeader.Status = SD.TableStatusCancelled;
            tableHeader.TimeRejected = DateTime.Now;
            tableHeader.RejectedBy = "Customer";

            _db.SaveChanges();
            return RedirectToAction("Index","TableBookingHistory");
        }
        public bool UpdateQuantity(int TableHeaderId)
        {
            try
            {
                var tblDetailsObj = _db.TableBookingHeader.Where(t => t.Id == TableHeaderId).FirstOrDefault();
                if (tblDetailsObj != null)
                {
                    TableTrack objT = _db.TableTrack.Include(t => t.Table).Where(t => t.DateTable.Date == tblDetailsObj.SitInDate.Date && t.TimeTable == tblDetailsObj.SitInTime && t.Table.SeatingName == tblDetailsObj.TableName).FirstOrDefault();
                    if (objT != null)
                    {
                        objT.AmtAva += tblDetailsObj.TableBooked;
                        if (objT.AmtAva > objT.Table.MaxTables)
                            objT.AmtAva = objT.Table.MaxTables;

                        _db.SaveChanges();
                    }
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
